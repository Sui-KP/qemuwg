using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace SuiQemu;

public class 内存参数
{
    public int 容量 = 2048, 插槽 = 0, 最大容量 = 4096, 节点 = 1;
    public bool 预分配 = false, 气球 = false, 加密 = false, 大页 = false;
}
public class 磁盘参数
{
    public int 模式 = 0;
    public string 路径 = "";
    public string 容量 = "20G";
    public string 格式 = "qcow2";
    public string 分配 = "off";
    public string 接口 = "Virtio";
}
public class 网络参数 { public string 设备 = ""; }
public class 仿真配置
{
    public 内存参数 内存 = new();
    public 磁盘参数 磁盘 = new();
    public 网络参数 网络 = new();
}
public class 架构数据
{
    public string? 路径;
    public List<string> 架构列表 = [], 机器列表 = [], 处理器列表 = [];
    public string? 选架构, 选机器, 选处理器;
    public bool 加载中;
}
public partial class 架构 : Grid
{
    public static 架构 实例 { get; } = new();
    public 架构数据 数据 { get; } = new();
    private readonly Dictionary<string, (List<string>, List<string>)> _缓存 = [];
    private readonly ComboBox _c1, _c2, _c3;
    private readonly ProgressBar _p1;
    public 架构()
    {
        var vsp = new StackPanel { Spacing = 10, Padding = new Thickness(20) };
        vsp.Children.Add(_p1 = new ProgressBar { IsIndeterminate = true, Visibility = Visibility.Collapsed });
        vsp.Children.Add(_c1 = new ComboBox { Header = "架构系统", HorizontalAlignment = HorizontalAlignment.Stretch });
        vsp.Children.Add(_c2 = new ComboBox { Header = "机器类型", HorizontalAlignment = HorizontalAlignment.Stretch });
        vsp.Children.Add(_c3 = new ComboBox { Header = "处理器型号", HorizontalAlignment = HorizontalAlignment.Stretch });
        _c1.SelectionChanged += async (s, e) => { if (_c1.SelectedItem is string v && v != 数据.选架构) await 更新架构(v); };
        _c2.SelectionChanged += (s, e) => 数据.选机器 = _c2.SelectedItem as string;
        _c3.SelectionChanged += (s, e) => 数据.选处理器 = _c3.SelectedItem as string;
        Children.Add(vsp);
    }
    public void 刷新UI()
    {
        _p1.Visibility = 数据.加载中 ? Visibility.Visible : Visibility.Collapsed;
        绑定(_c1, 数据.架构列表, 数据.选架构);
        绑定(_c2, 数据.机器列表, 数据.选机器);
        绑定(_c3, 数据.处理器列表, 数据.选处理器);
    }
    public string 拼命令(仿真配置 p)
    {
        if (数据.路径 == null || 数据.选架构 == null) return "";
        var m = p.内存; var d = p.磁盘; var n = p.网络;
        var exe = Path.Combine(数据.路径, $"qemu-system-{数据.选架构}.exe");
        var cmd = $"\"{exe}\" -machine {数据.选机器} -cpu {数据.选处理器} -m {m.容量}";
        if (m.插槽 > 0) cmd += $",slots={m.插槽},maxmem={m.最大容量}M";
        cmd += $" -object {(m.大页 ? "memory-backend-file" : "memory-backend-ram")},id=ram0,size={m.容量}M{(m.预分配 ? ",prealloc=on" : "")}";
        if (m.节点 > 1) for (int i = 0; i < m.节点; i++) cmd += $" -numa node,mem={m.容量 / m.节点}M,cpus={i},nodeid={i}";
        else cmd += " -machine memory-backend=ram0";
        if (!string.IsNullOrEmpty(d.路径))
        {
            var driveStr = $" -drive file=\"{d.路径}\",if={d.接口.ToLower()}";
            if (d.模式 != 2) driveStr += $",format={d.格式}";
            cmd += driveStr;
        }
        if (!string.IsNullOrEmpty(n.设备)) cmd += $" -netdev user,id=net0 -device {n.设备},netdev=net0";
        if (m.气球) cmd += " -device virtio-balloon";
        if (m.加密) cmd += " -object sev-guest,id=sev0 -machine memory-encryption=sev0";
        return cmd;
    }
    public async Task 扫描路径(string p)
    {
        数据.路径 = p; 数据.加载中 = true; 刷新UI();
        var d = new DirectoryInfo(p);
        if (d.Exists)
        {
            数据.架构列表 = [.. d.GetFiles("qemu-system-*.exe").Where(f => !f.Name.EndsWith("w.exe")).Select(f => f.Name[12..^4]).OrderBy(n => n)];
            if (数据.架构列表.Count > 0) await 更新架构(数据.架构列表[0]);
        }
        数据.加载中 = false; 刷新UI();
    }
    public async Task 更新架构(string n)
    {
        数据.选架构 = n;
        if (_缓存.TryGetValue(n, out var d)) (数据.机器列表, 数据.处理器列表) = d;
        else
        {
            数据.加载中 = true; 刷新UI();
            var exe = Path.Combine(数据.路径!, $"qemu-system-{n}.exe");
            var res = await Task.WhenAll(运行(exe, "-machine help"), 运行(exe, "-cpu help"));
            数据.机器列表 = 解析(res[0]); 数据.处理器列表 = 解析(res[1]);
            _缓存[n] = (数据.机器列表, 数据.处理器列表); 数据.加载中 = false;
        }
        数据.选机器 = 数据.机器列表.FirstOrDefault(); 数据.选处理器 = 数据.处理器列表.FirstOrDefault();
        刷新UI();
        _ = 网络.实例.刷新设备();
    }
    private static async Task<string> 运行(string f, string a)
    {
        try { using var p = Process.Start(new ProcessStartInfo(f, a) { RedirectStandardOutput = true, CreateNoWindow = true }); return p == null ? "" : await p.StandardOutput.ReadToEndAsync(); }
        catch { return ""; }
    }
    private static List<string> 解析(string c) => [.. c.Split('\n').Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim().Split(' ')[0]).Distinct().OrderBy(s => s)];
    private static void 绑定(ComboBox c, List<string> d, string? v)
    {
        if (c.ItemsSource != d) { c.ItemsSource = null; c.ItemsSource = d; }
        if (v != null && d.Contains(v)) c.SelectedItem = v;
        else if (d.Count > 0) c.SelectedIndex = 0;
    }
}