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
    public int 容量 = 2048, 插槽, 最大容量 = 4096, 节点 = 1;
    public bool 预分配, 气球, 加密, 大页;
}
public class 磁盘参数
{
    public int 模式;
    public string 路径 = "", 容量 = "20G", 格式 = "qcow2", 分配 = "off", 接口 = "Virtio";
}
public class 网络参数 { public string 设备 = ""; }
public class 显示参数 { public string 设备 = ""; }
public class 仿真配置
{
    public 内存参数 内存 = new();
    public 磁盘参数 磁盘 = new();
    public 网络参数 网络 = new();
    public 显示参数 显示 = new();
}
public class 架构数据
{
    public string? 路径, 选架构, 选机器, 选处理器, 选加速 = "自动", 翻译块大小;
    public string? 逻辑核心, 插槽数, 核心数, 线程数;
    public List<string> 架构列表 = [], 机器列表 = [], 处理器列表 = [], 加速列表 = ["自动", "TCG", "TCG(多线程)", "WHPX", "WHPX+TCG"];
    public bool 加载中;
}
public partial class 架构 : Grid
{
    public static 架构 实例 { get; } = new();
    public 架构数据 数据 { get; } = new();
    private readonly Dictionary<string, (List<string>, List<string>)> _缓存 = [];
    private readonly ComboBox _架构框, _机器框, _处理器框, _加速框;
    private readonly TextBox _逻辑框, _插槽框, _核心框, _线程框, _翻译块框;
    private readonly ProgressBar _进度条;
    public 架构()
    {
        var 容器 = new StackPanel { Spacing = 10, Padding = new Thickness(20) };
        容器.Children.Add(_进度条 = new() { IsIndeterminate = true, Visibility = (Visibility)1 });
        容器.Children.Add(_架构框 = new() { Header = "架构", HorizontalAlignment = (HorizontalAlignment)3 });
        容器.Children.Add(_机器框 = new() { Header = "类型", HorizontalAlignment = (HorizontalAlignment)3 });
        var 核心网格 = new Grid { ColumnSpacing = 10 };
        for (int i = 0; i < 5; i++) 核心网格.ColumnDefinitions.Add(new());
        核心网格.Children.Add(_处理器框 = new() { Header = "处理器", HorizontalAlignment = (HorizontalAlignment)3 });
        核心网格.Children.Add(_逻辑框 = new() { Header = "vCPU", HorizontalAlignment = (HorizontalAlignment)3 });
        核心网格.Children.Add(_插槽框 = new() { Header = "插槽", HorizontalAlignment = (HorizontalAlignment)3 });
        核心网格.Children.Add(_核心框 = new() { Header = "核心", HorizontalAlignment = (HorizontalAlignment)3 });
        核心网格.Children.Add(_线程框 = new() { Header = "线程", HorizontalAlignment = (HorizontalAlignment)3 });
        for (int i = 0; i < 5; i++) Grid.SetColumn(核心网格.Children[i] as FrameworkElement, i);
        容器.Children.Add(核心网格);
        var 加速网格 = new Grid { ColumnSpacing = 10 };
        加速网格.ColumnDefinitions.Add(new()); 加速网格.ColumnDefinitions.Add(new());
        加速网格.Children.Add(_加速框 = new() { Header = "加速", HorizontalAlignment = (HorizontalAlignment)3, ItemsSource = 数据.加速列表 });
        加速网格.Children.Add(_翻译块框 = new() { Header = "翻译块", HorizontalAlignment = (HorizontalAlignment)3 });
        Grid.SetColumn(_翻译块框, 1);
        容器.Children.Add(加速网格);
        _加速框.SelectedIndex = 0;
        _架构框.SelectionChanged += async (s, e) => { if (_架构框.SelectedItem is string 选 && 选 != 数据.选架构) await 更新架构(选); };
        _机器框.SelectionChanged += (s, e) => 数据.选机器 = _机器框.SelectedItem as string;
        _处理器框.SelectionChanged += (s, e) => 数据.选处理器 = _处理器框.SelectedItem as string;
        _加速框.SelectionChanged += (s, e) => {
            数据.选加速 = _加速框.SelectedItem as string;
            _翻译块框.IsEnabled = 数据.选加速?.Contains("TCG") ?? false;
        };
        _逻辑框.TextChanged += (s, e) => 数据.逻辑核心 = _逻辑框.Text;
        _插槽框.TextChanged += (s, e) => 数据.插槽数 = _插槽框.Text;
        _核心框.TextChanged += (s, e) => 数据.核心数 = _核心框.Text;
        _线程框.TextChanged += (s, e) => 数据.线程数 = _线程框.Text;
        _翻译块框.TextChanged += (s, e) => 数据.翻译块大小 = _翻译块框.Text;
        Children.Add(容器);
    }
    public string 拼命令(仿真配置 配置)
    {
        if (数据.路径 is not string 根路径 || 数据.选架构 is not string 架构名) return "";
        var (内存, 磁盘, 网络, 显示) = (配置.内存, 配置.磁盘, 配置.网络, 配置.显示);
        var 命令行 = $"\"{Path.Combine(根路径, $"qemu-system-{架构名}.exe")}\" -machine {数据.选机器} -cpu {数据.选处理器}";
        var 拓扑 = new List<string>();
        if (!string.IsNullOrWhiteSpace(数据.逻辑核心)) 拓扑.Add(数据.逻辑核心);
        if (!string.IsNullOrWhiteSpace(数据.插槽数)) 拓扑.Add($"sockets={数据.插槽数}");
        if (!string.IsNullOrWhiteSpace(数据.核心数)) 拓扑.Add($"cores={数据.核心数}");
        if (!string.IsNullOrWhiteSpace(数据.线程数)) 拓扑.Add($"threads={数据.线程数}");
        if (拓扑.Count > 0) 命令行 += $" -smp {string.Join(",", 拓扑)}";
        命令行 += $" -m {内存.容量}";
        if (数据.选加速 is string 模式 && 模式 != "自动")
        {
            if (模式 == "WHPX+TCG") 命令行 += " -accel whpx -accel tcg";
            else if (模式 == "WHPX") 命令行 += " -accel whpx";
            else if (模式.StartsWith("TCG"))
            {
                var 参数 = 模式.Contains("多线程") ? ",thread=multi" : "";
                if (!string.IsNullOrWhiteSpace(数据.翻译块大小)) 参数 += $",tb-size={数据.翻译块大小}";
                命令行 += $" -accel tcg{参数}";
            }
        }
        if (内存.插槽 > 0) 命令行 += $",slots={内存.插槽},maxmem={内存.最大容量}M";
        命令行 += $" -object {(内存.大页 ? "memory-backend-file" : "memory-backend-ram")},id=ram0,size={内存.容量}M{(内存.预分配 ? ",prealloc=on" : "")}";
        if (内存.节点 > 1) for (int i = 0; i < 内存.节点; i++) 命令行 += $" -numa node,mem={内存.容量 / 内存.节点}M,cpus={i},nodeid={i}";
        else 命令行 += " -machine memory-backend=ram0";
        if (!string.IsNullOrEmpty(磁盘.路径)) 命令行 += $" -drive file=\"{磁盘.路径}\",if={磁盘.接口.ToLower()}{(磁盘.模式 != 2 ? $",format={磁盘.格式}" : "")}";
        if (!string.IsNullOrEmpty(网络.设备)) 命令行 += $" -netdev user,id=net0 -device {网络.设备},netdev=net0";
        if (!string.IsNullOrEmpty(显示.设备)) 命令行 += $" -device {显示.设备}";
        if (内存.气球) 命令行 += " -device virtio-balloon";
        if (内存.加密) 命令行 += " -object sev-guest,id=sev0 -machine memory-encryption=sev0";
        return 命令行;
    }
    public async Task 更新架构(string 架构名)
    {
        数据.选架构 = 架构名;
        if (!_缓存.TryGetValue(架构名, out var 列表))
        {
            数据.加载中 = true; 刷新UI();
            var 程序 = Path.Combine(数据.路径!, $"qemu-system-{架构名}.exe");
            var 结果 = await Task.WhenAll(运行(程序, "-machine help"), 运行(程序, "-cpu help"));
            _缓存[架构名] = 列表 = (解析(结果[0]), 解析(结果[1])); 数据.加载中 = false;
        }
        (数据.机器列表, 数据.处理器列表) = 列表;
        (数据.选机器, 数据.选处理器) = (列表.Item1.FirstOrDefault(), 列表.Item2.FirstOrDefault());
        刷新UI(); _ = 网络.实例.刷新设备(); _ = 显示.实例.刷新设备();
    }
    private static async Task<string> 运行(string 程序, string 参数)
    {
        try { using var 进程 = Process.Start(new ProcessStartInfo(程序, 参数) { RedirectStandardOutput = true, CreateNoWindow = true }); return 进程 is null ? "" : await 进程.StandardOutput.ReadToEndAsync(); }
        catch { return ""; }
    }
    public async Task 扫描路径(string 根路径)
    {
        数据.路径 = 根路径; 数据.加载中 = true; 刷新UI();
        if (new DirectoryInfo(根路径) is { Exists: true } 目录)
        {
            数据.架构列表 = [.. 目录.GetFiles("qemu-system-*.exe").Where(f => !f.Name.EndsWith("w.exe")).Select(f => f.Name[12..^4]).OrderBy(n => n)];
            if (数据.架构列表.Count > 0) await 更新架构(数据.架构列表[0]);
        }
        数据.加载中 = false; 刷新UI();
    }
    public void 刷新UI()
    {
        _进度条.Visibility = (Visibility)(数据.加载中 ? 0 : 1);
        绑定(_架构框, 数据.架构列表, 数据.选架构); 绑定(_机器框, 数据.机器列表, 数据.选机器); 绑定(_处理器框, 数据.处理器列表, 数据.选处理器);
    }
    private static List<string> 解析(string 输出) => [.. 输出.Split('\n').Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim().Split(' ')[0]).Distinct().OrderBy(s => s)];
    private static void 绑定(ComboBox 下拉框, List<string> 数据源, string? 选中值)
    {
        if (下拉框.ItemsSource != 数据源) 下拉框.ItemsSource = 数据源;
        下拉框.SelectedItem = (选中值 != null && 数据源.Contains(选中值)) ? 选中值 : (数据源.Count > 0 ? 数据源[0] : null);
    }
}