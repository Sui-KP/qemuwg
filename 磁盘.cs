using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
namespace SuiQemu;
public partial class 磁盘 : StackPanel
{
    public static 磁盘 实例 { get; } = new();
    public 磁盘参数 配置 { get; } = new();
    public nint 窗口句柄 { get; set; }
    private readonly ComboBox _c模式, _c磁盘, _c格式, _c分配, _c接口;
    private readonly TextBox _t路径, _t容量;
    private readonly Button _b创建, _b浏览;
    private readonly ProgressBar _p1;
    public 磁盘()
    {
        Spacing = 15; Padding = new Thickness(20);
        _p1 = new ProgressBar { IsIndeterminate = true, Visibility = Visibility.Collapsed };
        _c模式 = new ComboBox { ItemsSource = new List<string> { "新建镜像", "使用已有", "物理磁盘" }, SelectedIndex = 0 };
        _t路径 = new TextBox { PlaceholderText = "磁盘文件路径", HorizontalAlignment = HorizontalAlignment.Stretch };
        _b浏览 = new Button { Content = "浏览" };
        var r1 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        r1.Children.Add(_c模式); r1.Children.Add(_t路径); r1.Children.Add(_b浏览);
        _c磁盘 = new ComboBox { Header = "选择物理磁盘", Visibility = Visibility.Collapsed, HorizontalAlignment = HorizontalAlignment.Stretch };
        var r2 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        _t容量 = new TextBox { Header = "容量", Text = "20G", Width = 80 };
        _c格式 = new ComboBox { Header = "格式", ItemsSource = new List<string> { "qcow2", "raw", "vmdk" }, SelectedValue = "qcow2" };
        _c分配 = new ComboBox { Header = "预分配", ItemsSource = new List<string> { "off", "metadata", "falloc", "full" }, SelectedValue = "off" };
        _c接口 = new ComboBox { Header = "接口总线", ItemsSource = new List<string> { "Virtio", "IDE", "SATA", "SCSI", "NVMe" }, SelectedValue = "Virtio" };
        _b创建 = new Button { Content = "立即创建", VerticalAlignment = VerticalAlignment.Bottom, Style = (Style)Application.Current.Resources["AccentButtonStyle"] };
        r2.Children.Add(_t容量); r2.Children.Add(_c格式); r2.Children.Add(_c分配); r2.Children.Add(_c接口); r2.Children.Add(_b创建);
        Children.Add(_p1); Children.Add(r1); Children.Add(_c磁盘); Children.Add(r2);
        绑定事件();
        初始化路径();
    }
    private void 初始化路径()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SuiQemuVM");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        _t路径.Text = Path.Combine(dir, "drive.qcow2");
        配置.路径 = _t路径.Text;
    }
    private void 绑定事件()
    {
        _c模式.SelectionChanged += async (s, e) => {
            配置.模式 = _c模式.SelectedIndex;
            _t路径.Visibility = 配置.模式 < 2 ? Visibility.Visible : Visibility.Collapsed;
            _b浏览.Visibility = 配置.模式 < 2 ? Visibility.Visible : Visibility.Collapsed;
            _c磁盘.Visibility = 配置.模式 == 2 ? Visibility.Visible : Visibility.Collapsed;
            _t容量.IsEnabled = _c格式.IsEnabled = _c分配.IsEnabled = _b创建.IsEnabled = (配置.模式 == 0);
            if (配置.模式 == 2) await 加载磁盘();
        };
        _t路径.TextChanged += (s, e) => 配置.路径 = _t路径.Text;
        _t容量.TextChanged += (s, e) => 配置.容量 = _t容量.Text;
        _c格式.SelectionChanged += (s, e) => 配置.格式 = _c格式.SelectedValue?.ToString() ?? "qcow2";
        _c分配.SelectionChanged += (s, e) => 配置.分配 = _c分配.SelectedValue?.ToString() ?? "off";
        _c接口.SelectionChanged += (s, e) => 配置.接口 = _c接口.SelectedValue?.ToString() ?? "Virtio";
        _c磁盘.SelectionChanged += (s, e) => {
            if (_c磁盘.SelectedItem is string v)
            {
                var path = "\\\\.\\PhysicalDrive" + v.Split('|')[0];
                _t路径.Text = path;
                配置.路径 = path;
            }
        };
        _b创建.Click += async (s, e) => await 创建磁盘();
        _b浏览.Click += async (s, e) => {
            var picker = new FileOpenPicker();
            if (窗口句柄 != IntPtr.Zero) WinRT.Interop.InitializeWithWindow.Initialize(picker, 窗口句柄);
            picker.FileTypeFilter.Add("*");
            var file = await picker.PickSingleFileAsync();
            if (file != null) _t路径.Text = file.Path;
        };
    }
    public async Task 加载磁盘()
    {
        _p1.Visibility = Visibility.Visible;
        var list = await Task.Run(() => {
            var res = new List<string>();
            try
            {
                var info = new ProcessStartInfo { FileName = "powershell", Arguments = "-NoProfile -Command \"Get-PhysicalDisk | %{ \\\"$($_.DeviceId)|$($_.FriendlyName)\\\" }\"", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                using var p = Process.Start(info);
                if (p != null)
                {
                    string outStr = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    res.AddRange(outStr.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Where(l => l.Contains('|')));
                }
            }
            catch { }
            return res;
        });
        _c磁盘.ItemsSource = list;
        if (list.Count > 0) _c磁盘.SelectedIndex = 0;
        _p1.Visibility = Visibility.Collapsed;
    }
    private async Task 创建磁盘()
    {
        if (string.IsNullOrEmpty(架构.实例.数据.路径)) return;
        if (string.IsNullOrEmpty(_t路径.Text)) 初始化路径();
        _p1.Visibility = Visibility.Visible; _b创建.IsEnabled = false;
        var exe = Path.Combine(架构.实例.数据.路径, "qemu-img.exe");
        var args = $"create -f {配置.格式} " + (配置.分配 != "off" ? $"-o preallocation={配置.分配} " : "") + $"\"{配置.路径}\" {配置.容量}";
        await Task.Run(() => { try { using var p = Process.Start(new ProcessStartInfo(exe, args) { CreateNoWindow = true }); p?.WaitForExit(); } catch { } });
        _p1.Visibility = Visibility.Collapsed; _b创建.IsEnabled = true;
    }
}