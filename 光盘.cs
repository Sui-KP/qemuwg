using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;
namespace SuiQemu;

public class 光盘参数 { public List<string> 路径列表 = []; }
public partial class 光盘 : StackPanel
{
    public static 光盘 实例 { get; } = new();
    public 光盘参数 配置 { get; } = new();
    public nint 窗口句柄 { get; set; }
    private readonly StackPanel _列表容器 = new() { Spacing = 5 };
    private readonly Button _b浏览 = new() { Content = "添加镜像文件" };
    private readonly Button _b清空 = new() { Content = "清空列表" };
    public 光盘()
    {
        Spacing = 15; Padding = new Thickness(20);
        var 按钮栏 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        按钮栏.Children.Add(_b浏览);
        按钮栏.Children.Add(_b清空);
        _b浏览.Click += async (s, e) =>
        {
            var 选择器 = new FileOpenPicker();
            if (窗口句柄 != IntPtr.Zero) WinRT.Interop.InitializeWithWindow.Initialize(选择器, 窗口句柄);
            选择器.FileTypeFilter.Add(".iso");
            选择器.FileTypeFilter.Add(".img");
            var 文件列表 = await 选择器.PickMultipleFilesAsync();
            if (文件列表 != null)
            {
                foreach (var 文件 in 文件列表)
                {
                    if (!配置.路径列表.Contains(文件.Path))
                    {
                        配置.路径列表.Add(文件.Path);
                        添加文件项界面(文件.Path);
                    }
                }
            }
        };
        _b清空.Click += (s, e) => { 配置.路径列表.Clear(); _列表容器.Children.Clear(); };
        Children.Add(new TextBlock { Text = "已加载的光盘镜像列表", Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"] });
        Children.Add(_列表容器);
        Children.Add(按钮栏);
    }
    private void 添加文件项界面(string 路径)
    {
        var 行 = new Grid { ColumnSpacing = 10 };
        行.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        行.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var 路径框 = new TextBox { Text = 路径, IsReadOnly = true };
        var 移除按钮 = new Button { Content = "移除" };
        移除按钮.Click += (s, e) =>
        {
            配置.路径列表.Remove(路径);
            _列表容器.Children.Remove(行);
        };
        Grid.SetColumn(路径框, 0);
        Grid.SetColumn(移除按钮, 1);
        行.Children.Add(路径框);
        行.Children.Add(移除按钮);
        _列表容器.Children.Add(行);
    }
}