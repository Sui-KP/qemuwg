using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
namespace SuiQemu;

public partial class 磁盘 : StackPanel
{
    public static 磁盘 实例 { get; } = new();
    public 磁盘参数 配置 { get; } = new();
    public nint 窗口句柄 { get; set; }
    private readonly ComboBox _模式框, _磁盘框, _格式框, _分配框, _接口框;
    private readonly TextBox _路径框, _容量框;
    private readonly Button _创建键, _浏览键;
    private readonly ProgressBar _进度条;
    public 磁盘()
    {
        Spacing = 15; Padding = new Thickness(20);
        _进度条 = new() { IsIndeterminate = true, Visibility = (Visibility)1 };
        _模式框 = new() { ItemsSource = new List<string> { "新建镜像", "使用已有", "物理磁盘" }, SelectedIndex = 0 };
        _路径框 = new() { PlaceholderText = "磁盘文件路径", HorizontalAlignment = (HorizontalAlignment)3 };
        _浏览键 = new() { Content = "浏览" };
        var 行1 = new StackPanel { Orientation = (Orientation)1, Spacing = 10 };
        行1.Children.Add(_模式框); 行1.Children.Add(_路径框); 行1.Children.Add(_浏览键);
        _磁盘框 = new() { Header = "选择物理磁盘", Visibility = (Visibility)1, HorizontalAlignment = (HorizontalAlignment)3 };
        var 行2 = new StackPanel { Orientation = (Orientation)1, Spacing = 10 };
        _容量框 = new() { Header = "容量", Text = "20G", Width = 80 };
        _格式框 = new() { Header = "格式", ItemsSource = new List<string> { "qcow2", "raw", "vmdk" }, SelectedValue = "qcow2" };
        _分配框 = new() { Header = "预分配", ItemsSource = new List<string> { "off", "metadata", "falloc", "full" }, SelectedValue = "off" };
        _接口框 = new() { Header = "接口总线", ItemsSource = new List<string> { "Virtio", "IDE", "SATA", "SCSI", "NVMe" }, SelectedValue = "Virtio" };
        _创建键 = new() { Content = "立即创建", VerticalAlignment = (VerticalAlignment)2, Style = (Style)Application.Current.Resources["AccentButtonStyle"] };
        行2.Children.Add(_容量框); 行2.Children.Add(_格式框); 行2.Children.Add(_分配框); 行2.Children.Add(_接口框); 行2.Children.Add(_创建键);
        Children.Add(_进度条); Children.Add(行1); Children.Add(_磁盘框); Children.Add(行2);
        绑定事件();
        初始化路径();
    }
    private void 初始化路径()
    {
        var 目录 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SuiQemuVM");
        if (!Directory.Exists(目录)) Directory.CreateDirectory(目录);
        _路径框.Text = Path.Combine(目录, "drive.qcow2");
        配置.路径 = _路径框.Text;
    }
    private void 绑定事件()
    {
        _模式框.SelectionChanged += async (s, e) =>
        {
            配置.模式 = _模式框.SelectedIndex;
            _路径框.Visibility = _浏览键.Visibility = (Visibility)(配置.模式 < 2 ? 0 : 1);
            _磁盘框.Visibility = (Visibility)(配置.模式 == 2 ? 0 : 1);
            _容量框.IsEnabled = _格式框.IsEnabled = _分配框.IsEnabled = _创建键.IsEnabled = 配置.模式 == 0;
            if (配置.模式 == 2) await 加载磁盘();
        };
        _路径框.TextChanged += (s, e) => 配置.路径 = _路径框.Text;
        _容量框.TextChanged += (s, e) => 配置.容量 = _容量框.Text;
        _格式框.SelectionChanged += (s, e) => 配置.格式 = _格式框.SelectedValue?.ToString() ?? "qcow2";
        _分配框.SelectionChanged += (s, e) => 配置.分配 = _分配框.SelectedValue?.ToString() ?? "off";
        _接口框.SelectionChanged += (s, e) => 配置.接口 = _接口框.SelectedValue?.ToString() ?? "Virtio";
        _磁盘框.SelectionChanged += (s, e) =>
        {
            if (_磁盘框.SelectedItem is string 项)
            {
                var 路径 = "\\\\.\\PhysicalDrive" + 项.Split('|')[0];
                _路径框.Text = 配置.路径 = 路径;
            }
        };
        _创建键.Click += async (s, e) => await 创建磁盘();
        _浏览键.Click += async (s, e) =>
        {
            var 选择器 = new FileOpenPicker();
            if (窗口句柄 != IntPtr.Zero) WinRT.Interop.InitializeWithWindow.Initialize(选择器, 窗口句柄);
            选择器.FileTypeFilter.Add("*");
            var 文件 = await 选择器.PickSingleFileAsync();
            if (文件 != null) _路径框.Text = 文件.Path;
        };
    }
    public async Task 加载磁盘()
    {
        _进度条.Visibility = 0;
        var 列表 = await Task.Run(() =>
        {
            var 结果 = new List<string>();
            try
            {
                var 信息 = new ProcessStartInfo { FileName = "powershell", Arguments = "-NoProfile -Command \"Get-PhysicalDisk | %{ \\\"$($_.DeviceId)|$($_.FriendlyName)\\\" }\"", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                using var 进程 = Process.Start(信息);
                if (进程 != null)
                {
                    string 输出 = 进程.StandardOutput.ReadToEnd();
                    进程.WaitForExit();
                    结果.AddRange(输出.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Where(l => l.Contains('|')));
                }
            }
            catch { }
            return 结果;
        });
        _磁盘框.ItemsSource = 列表;
        if (列表.Count > 0) _磁盘框.SelectedIndex = 0;
        _进度条.Visibility = (Visibility)1;
    }
    private async Task 创建磁盘()
    {
        if (string.IsNullOrEmpty(架构.实例.数据.路径)) return;
        if (string.IsNullOrEmpty(_路径框.Text)) 初始化路径();
        _进度条.Visibility = 0; _创建键.IsEnabled = false;
        var 程序 = Path.Combine(架构.实例.数据.路径, "qemu-img.exe");
        var 参数 = $"create -f {配置.格式} " + (配置.分配 != "off" ? $"-o preallocation={配置.分配} " : "") + $"\"{配置.路径}\" {配置.容量}";
        await Task.Run(() => { try { using var 进程 = Process.Start(new ProcessStartInfo(程序, 参数) { CreateNoWindow = true }); 进程?.WaitForExit(); } catch { } });
        _进度条.Visibility = (Visibility)1; _创建键.IsEnabled = true;
    }
}