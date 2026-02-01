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
        Padding = new Thickness(9);
        _进度条 = new() { IsIndeterminate = true, Visibility = Visibility.Collapsed };
        _模式框 = new() { ItemsSource = new List<string> { "新磁盘", "已有盘", "物理盘" }, SelectedIndex = 0 };
        _路径框 = new() { PlaceholderText = "磁盘文件路径", HorizontalAlignment = HorizontalAlignment.Stretch };
        _浏览键 = new() { Content = "浏览" };
        _接口框 = new() { ItemsSource = new List<string> { "Virtio", "IDE", "SATA", "SCSI", "NVMe" }, SelectedValue = "Virtio" };
        var g = new Grid();
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(_模式框, 0); Grid.SetColumn(_路径框, 1); Grid.SetColumn(_浏览键, 2); Grid.SetColumn(_接口框, 3);
        g.Children.Add(_模式框); g.Children.Add(_路径框); g.Children.Add(_浏览键); g.Children.Add(_接口框);
        _磁盘框 = new() { Header = "选择物理磁盘", Visibility = Visibility.Collapsed, HorizontalAlignment = HorizontalAlignment.Stretch };
        var r2 = new StackPanel { Orientation = Orientation.Horizontal };
        _容量框 = new() { Header = "容量", Text = "20G" };
        _格式框 = new() { Header = "格式", ItemsSource = new List<string> { "qcow2", "raw", "vmdk" }, SelectedValue = "qcow2" };
        _分配框 = new() { Header = "预分配", ItemsSource = new List<string> { "off", "metadata", "falloc", "full" }, SelectedValue = "off" };
        _创建键 = new() { Content = "立即创建", VerticalAlignment = VerticalAlignment.Bottom, Style = (Style)Application.Current.Resources["AccentButtonStyle"] };
        r2.Children.Add(_容量框); r2.Children.Add(_格式框); r2.Children.Add(_分配框); r2.Children.Add(_创建键);
        Children.Add(_进度条); Children.Add(g); Children.Add(_磁盘框); Children.Add(r2);
        绑定事件(); 初始化路径(); 刷新布局();
    }
    private void 初始化路径()
    {
        var d = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SuiQemuVM");
        if (!Directory.Exists(d)) Directory.CreateDirectory(d);
        _路径框.Text = Path.Combine(d, "drive.qcow2");
        配置.路径 = _路径框.Text;
    }
    private void 刷新布局()
    {
        bool isNew = 配置.模式 == 0; bool isPhys = 配置.模式 == 2;
        _路径框.Visibility = _浏览键.Visibility = isNew ? Visibility.Collapsed : Visibility.Visible;
        _磁盘框.Visibility = isPhys ? Visibility.Visible : Visibility.Collapsed;
        _容量框.Visibility = _格式框.Visibility = _分配框.Visibility = _创建键.Visibility = isNew ? Visibility.Visible : Visibility.Collapsed;
    }
    private void 绑定事件()
    {
        _模式框.SelectionChanged += async (s, e) =>
        {
            配置.模式 = _模式框.SelectedIndex;
            刷新布局();
            if (配置.模式 == 2) await 加载磁盘();
        };
        _路径框.TextChanged += (s, e) => 配置.路径 = _路径框.Text;
        _容量框.TextChanged += (s, e) => 配置.容量 = _容量框.Text;
        _格式框.SelectionChanged += (s, e) => 配置.格式 = _格式框.SelectedValue?.ToString() ?? "qcow2";
        _分配框.SelectionChanged += (s, e) => 配置.分配 = _分配框.SelectedValue?.ToString() ?? "off";
        _接口框.SelectionChanged += (s, e) => 配置.接口 = _接口框.SelectedValue?.ToString() ?? "Virtio";
        _磁盘框.SelectionChanged += (s, e) =>
        {
            if (_磁盘框.SelectedItem is string t) { var p = "\\\\.\\PhysicalDrive" + t.Split('|')[0]; _路径框.Text = 配置.路径 = p; }
        };
        _创建键.Click += async (s, e) => await 创建磁盘();
        _浏览键.Click += async (s, e) =>
        {
            var p = new FileOpenPicker();
            if (窗口句柄 != IntPtr.Zero) WinRT.Interop.InitializeWithWindow.Initialize(p, 窗口句柄);
            p.FileTypeFilter.Add("*");
            var f = await p.PickSingleFileAsync();
            if (f != null) _路径框.Text = f.Path;
        };
    }
    public async Task 加载磁盘()
    {
        _进度条.Visibility = Visibility.Visible;
        var l = await Task.Run(() =>
        {
            var r = new List<string>();
            try
            {
                var i = new ProcessStartInfo { FileName = "powershell", Arguments = "-NoProfile -Command \"Get-PhysicalDisk | %{ \\\"$($_.DeviceId)|$($_.FriendlyName)\\\" }\"", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                using var p = Process.Start(i);
                if (p != null) { string o = p.StandardOutput.ReadToEnd(); p.WaitForExit(); r.AddRange(o.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Where(x => x.Contains('|'))); }
            }
            catch { }
            return r;
        });
        _磁盘框.ItemsSource = l;
        if (l.Count > 0) _磁盘框.SelectedIndex = 0;
        _进度条.Visibility = Visibility.Collapsed;
    }
    private async Task 创建磁盘()
    {
        if (string.IsNullOrEmpty(架构.实例.数据.路径)) return;
        if (string.IsNullOrEmpty(_路径框.Text)) 初始化路径();
        _进度条.Visibility = Visibility.Visible; _创建键.IsEnabled = false;
        var e = Path.Combine(架构.实例.数据.路径, "qemu-img.exe");
        var a = $"create -f {配置.格式} " + (配置.分配 != "off" ? $"-o preallocation={配置.分配} " : "") + $"\"{配置.路径}\" {配置.容量}";
        await Task.Run(() => { try { using var p = Process.Start(new ProcessStartInfo(e, a) { CreateNoWindow = true }); p?.WaitForExit(); } catch { } });
        _进度条.Visibility = Visibility.Collapsed; _创建键.IsEnabled = true;
    }
}