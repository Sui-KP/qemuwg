using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace SuiQemu;

public class 内存参数 { public int 容量 = 2048, 插槽, 最大容量 = 4096, 节点 = 1; public bool 预分配, 气球, 加密, 大页; }
public class 磁盘参数 { public int 模式 = 0, 索引 = 0; public string 路径 = "", 容量 = "20G", 格式 = "qcow2", 分配 = "off", 接口 = "Virtio"; }
public class 网络参数 { public string 设备 = ""; }
public class 显示参数 { public string 设备 = ""; }
public class 仿真配置 { public 内存参数 内存 = new(); public 磁盘参数 磁盘 = new(); public 网络参数 网络 = new(); public 显示参数 显示 = new(); }
public class 架构数据 { public string? 路径, 选架构, 选机器, 选处理器, 选加速 = "自动", 翻译块, vCPU, 插槽, 核心, 线程, 虚拟机名; public List<string> 架构列表 = [], 机器列表 = [], 处理器列表 = [], 加速列表 = ["自动", "TCG", "TCG(多线程)", "WHPX", "WHPX+TCG"]; public bool 加载中; }
public partial class 架构 : Grid
{
    public static 架构 实例 { get; } = new();
    public 架构数据 数据 { get; } = new();
    private readonly Dictionary<string, (List<string>, List<string>)> _缓存 = [];
    private readonly ComboBox _架构框, _机器框, _处理器框, _加速框;
    private readonly TextBox _名称框, _vCPU框, _插槽框, _核心框, _线程框, _翻译块框;
    private readonly ProgressBar _进度条;
    public 架构()
    {
        var 根 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SuiQemuVM");
        if (!Directory.Exists(根)) Directory.CreateDirectory(根);
        数据.虚拟机名 = 自动重命名(根, "新仿真");
        var 容器 = new StackPanel { Padding = new Thickness(9) };
        容器.Children.Add(_进度条 = new() { IsIndeterminate = true, Visibility = (Visibility)1 });
        容器.Children.Add(_名称框 = new() { Header = "虚拟机名称", HorizontalAlignment = (HorizontalAlignment)3, Text = 数据.虚拟机名 });
        容器.Children.Add(_架构框 = new() { Header = "架构", HorizontalAlignment = (HorizontalAlignment)3 });
        容器.Children.Add(_机器框 = new() { Header = "类型", HorizontalAlignment = (HorizontalAlignment)3 });
        var 核心网格 = new Grid();
        for (int i = 0; i < 5; i++) 核心网格.ColumnDefinitions.Add(new());
        核心网格.Children.Add(_处理器框 = new() { Header = "处理器", HorizontalAlignment = (HorizontalAlignment)3 });
        核心网格.Children.Add(_vCPU框 = new() { Header = "vCPU", HorizontalAlignment = (HorizontalAlignment)3 });
        核心网格.Children.Add(_插槽框 = new() { Header = "插槽", HorizontalAlignment = (HorizontalAlignment)3 });
        核心网格.Children.Add(_核心框 = new() { Header = "核心", HorizontalAlignment = (HorizontalAlignment)3 });
        核心网格.Children.Add(_线程框 = new() { Header = "线程", HorizontalAlignment = (HorizontalAlignment)3 });
        for (int i = 0; i < 5; i++) Grid.SetColumn(核心网格.Children[i] as FrameworkElement, i);
        容器.Children.Add(核心网格);
        var 加速网格 = new Grid();
        加速网格.ColumnDefinitions.Add(new()); 加速网格.ColumnDefinitions.Add(new());
        加速网格.Children.Add(_加速框 = new() { Header = "加速", HorizontalAlignment = (HorizontalAlignment)3, ItemsSource = 数据.加速列表 });
        加速网格.Children.Add(_翻译块框 = new() { Header = "翻译块", HorizontalAlignment = (HorizontalAlignment)3 });
        Grid.SetColumn(_翻译块框, 1); 容器.Children.Add(加速网格); _加速框.SelectedIndex = 0;
        _名称框.TextChanged += (s, e) => 数据.虚拟机名 = _名称框.Text;
        _架构框.SelectionChanged += async (s, e) => { if (_架构框.SelectedItem is string 选 && 选 != 数据.选架构) await 更新(选); };
        _机器框.SelectionChanged += (s, e) => 数据.选机器 = _机器框.SelectedItem as string;
        _处理器框.SelectionChanged += (s, e) => 数据.选处理器 = _处理器框.SelectedItem as string;
        _加速框.SelectionChanged += (s, e) => { 数据.选加速 = _加速框.SelectedItem as string; _翻译块框.IsEnabled = 数据.选加速?.Contains("TCG") ?? false; };
        _vCPU框.TextChanged += (s, e) => 数据.vCPU = _vCPU框.Text; _插槽框.TextChanged += (s, e) => 数据.插槽 = _插槽框.Text;
        _核心框.TextChanged += (s, e) => 数据.核心 = _核心框.Text; _线程框.TextChanged += (s, e) => 数据.线程 = _线程框.Text;
        _翻译块框.TextChanged += (s, e) => 数据.翻译块 = _翻译块框.Text; Children.Add(容器);
    }
    public static string 自动重命名(string 根, string 基础名)
    {
        string 现名 = 基础名; int 序 = 0;
        while (Directory.Exists(Path.Combine(根, 现名))) { 序++; 现名 = $"{基础名}{序}"; }
        return 现名;
    }
    public string 拼命令(仿真配置 选)
    {
        if (数据.路径 is not string 根 || 数据.选架构 is not string 名) return "";
        var (内存, 磁盘, 网络, 显示) = (选.内存, 选.磁盘, 选.网络, 选.显示);
        var 行 = new List<string> { $"\"{Path.Combine(根, $"qemu-system-{名}.exe")}\"", $"-machine {数据.选机器}", $"-cpu {数据.选处理器}" };
        var 核心 = new List<string>();
        if (!string.IsNullOrWhiteSpace(数据.vCPU)) 核心.Add(数据.vCPU);
        if (!string.IsNullOrWhiteSpace(数据.插槽)) 核心.Add($"sockets={数据.插槽}");
        if (!string.IsNullOrWhiteSpace(数据.核心)) 核心.Add($"cores={数据.核心}");
        if (!string.IsNullOrWhiteSpace(数据.线程)) 核心.Add($"threads={数据.线程}");
        if (核心.Count > 0) 行.Add($"-smp {string.Join(",", 核心)}");
        var 内存行 = $"-m {内存.容量}";
        if (内存.插槽 > 0) 内存行 += $",slots={内存.插槽},maxmem={内存.最大容量}M";
        行.Add(内存行);
        if (数据.选加速 is string 模式 && 模式 != "自动")
        {
            if (模式 == "WHPX+TCG") { 行.Add("-accel whpx"); 行.Add("-accel tcg"); }
            else if (模式 == "WHPX") 行.Add("-accel whpx");
            else if (模式.StartsWith("TCG")) { var 参 = 模式.Contains("多线程") ? ",thread=multi" : ""; if (!string.IsNullOrWhiteSpace(数据.翻译块)) 参 += $",tb-size={数据.翻译块}"; 行.Add($"-accel tcg{参}"); }
        }
        var 后端 = 内存.大页 ? "memory-backend-file" : "memory-backend-ram";
        行.Add($"-object {后端},id=ram0,size={内存.容量}M{(内存.预分配 ? ",prealloc=on" : "")}");
        if (内存.节点 > 1) { for (int i = 0; i < 内存.节点; i++) 行.Add($"-numa node,mem={内存.容量 / 内存.节点}M,cpus={i},nodeid={i}"); } else 行.Add("-machine memory-backend=ram0");
        if (!string.IsNullOrEmpty(磁盘.路径)) 行.Add($"-drive file=\"{磁盘.路径}\",if={磁盘.接口.ToLower()},index=0{(磁盘.模式 != 2 ? $",format={磁盘.格式}" : "")}");
        var 光驱位 = 光盘.实例.配置.路径列表;
        for (int i = 0; i < 光驱位.Count; i++) if (!string.IsNullOrEmpty(光驱位[i])) 行.Add($"-drive file=\"{光驱位[i]}\",media=cdrom,index={i + 1}");
        if (!string.IsNullOrEmpty(网络.设备)) { 行.Add("-netdev user,id=net0"); 行.Add($"-device {网络.设备},netdev=net0"); }
        if (!string.IsNullOrEmpty(显示.设备)) 行.Add($"-device {显示.设备}");
        if (内存.气球) 行.Add("-device virtio-balloon");
        if (内存.加密) { 行.Add("-object sev-guest,id=sev0"); 行.Add("-machine memory-encryption=sev0"); }
        return string.Join("\n", 行);
    }
    public async Task 更新(string 名)
    {
        数据.选架构 = 名;
        if (!_缓存.TryGetValue(名, out var 组))
        {
            数据.加载中 = true; 刷新(); var 程序 = Path.Combine(数据.路径!, $"qemu-system-{名}.exe");
            var 输 = await Task.WhenAll(跑(程序, "-machine help"), 跑(程序, "-cpu help"));
            _缓存[名] = 组 = (拆(输[0]), 拆(输[1])); 数据.加载中 = false;
        }
        (数据.机器列表, 数据.处理器列表) = 组; (数据.选机器, 数据.选处理器) = (组.Item1.FirstOrDefault(), 组.Item2.FirstOrDefault());
        刷新(); _ = 网络.实例.刷新设备(); _ = 显示.实例.刷新设备();
    }
    private static async Task<string> 跑(string 径, string 参) { try { using var p = Process.Start(new ProcessStartInfo(径, 参) { RedirectStandardOutput = true, CreateNoWindow = true }); return p is null ? "" : await p.StandardOutput.ReadToEndAsync(); } catch { return ""; } }
    public async Task 扫描(string 根) { 数据.路径 = 根; 数据.加载中 = true; 刷新(); if (new DirectoryInfo(根) is { Exists: true } d) { 数据.架构列表 = [.. d.GetFiles("qemu-system-*.exe").Where(f => !f.Name.EndsWith("w.exe")).Select(f => f.Name[12..^4]).OrderBy(n => n)]; if (数据.架构列表.Count > 0) await 更新(数据.架构列表[0]); } 数据.加载中 = false; 刷新(); }
    public void 刷新() { _进度条.Visibility = (Visibility)(数据.加载中 ? 0 : 1); 选(_架构框, 数据.架构列表, 数据.选架构); 选(_机器框, 数据.机器列表, 数据.选机器); 选(_处理器框, 数据.处理器列表, 数据.选处理器); }
    private static List<string> 拆(string 文本) => [.. 文本.Split('\n').Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim().Split(' ')[0]).Distinct().OrderBy(s => s)];
    private static void 选(ComboBox 框, List<string> 源, string? 值) { if (框.ItemsSource != 源) 框.ItemsSource = 源; 框.SelectedItem = (值 != null && 源.Contains(值)) ? 值 : (源.Count > 0 ? 源[0] : null); }
}