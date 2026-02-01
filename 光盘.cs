using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage.Pickers;

namespace SuiQemu;

public class 光盘参数 { public string 路径 = ""; public int 索引 = 1; }

public partial class 光盘 : StackPanel
{
    public static 光盘 实例 { get; } = new();
    public 光盘参数 配置 { get; } = new();
    public nint 窗口句柄 { get; set; }
    private readonly TextBox _t路径 = new() { Header = "光盘镜像路径 (ISO)", HorizontalAlignment = HorizontalAlignment.Stretch, IsReadOnly = true };
    private readonly Button _b浏览 = new() { Content = "选择镜像文件" };
    private readonly Button _b清除 = new() { Content = "清除选择" };

    public 光盘()
    {
        Spacing = 15; Padding = new Thickness(20);
        var 按钮栏 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        按钮栏.Children.Add(_b浏览);
        按钮栏.Children.Add(_b清除);

        _b浏览.Click += async (s, e) =>
        {
            var 选择器 = new FileOpenPicker();
            if (窗口句柄 != IntPtr.Zero) WinRT.Interop.InitializeWithWindow.Initialize(选择器, 窗口句柄);
            选择器.FileTypeFilter.Add(".iso");
            选择器.FileTypeFilter.Add(".img");
            var 文件 = await 选择器.PickSingleFileAsync();
            if (文件 != null) { _t路径.Text = 文件.Path; 配置.路径 = 文件.Path; }
        };

        _b清除.Click += (s, e) => { _t路径.Text = ""; 配置.路径 = ""; };

        Children.Add(_t路径);
        Children.Add(按钮栏);
    }
}