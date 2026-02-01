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
    private readonly StackPanel _列表容器 = new();
    private readonly Button _b浏览 = new() { Content = "添加" };
    private readonly Button _b清空 = new() { Content = "清空" };
    public 光盘()
    {
        Padding = new Thickness(9);
        var g = new Grid();
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _b清空.HorizontalAlignment = HorizontalAlignment.Left;
        _b浏览.HorizontalAlignment = HorizontalAlignment.Right;
        Grid.SetColumn(_b清空, 0); Grid.SetColumn(_b浏览, 1);
        g.Children.Add(_b清空); g.Children.Add(_b浏览);
        _b浏览.Click += async (s, e) =>
        {
            var p = new FileOpenPicker();
            if (窗口句柄 != IntPtr.Zero) WinRT.Interop.InitializeWithWindow.Initialize(p, 窗口句柄);
            p.FileTypeFilter.Add(".iso"); p.FileTypeFilter.Add(".img");
            var fs = await p.PickMultipleFilesAsync();
            if (fs != null) foreach (var f in fs) if (!配置.路径列表.Contains(f.Path)) { 配置.路径列表.Add(f.Path); 添加文件项界面(f.Path); }
        };
        _b清空.Click += (s, e) => { 配置.路径列表.Clear(); _列表容器.Children.Clear(); };
        Children.Add(_列表容器); Children.Add(g);
    }
    private void 添加文件项界面(string 路径)
    {
        var g = new Grid();
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var t = new TextBox { Text = 路径, IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch };
        var r = new Button { Content = "移除" };
        r.Click += (s, e) => { 配置.路径列表.Remove(路径); _列表容器.Children.Remove(g); };
        Grid.SetColumn(t, 0); Grid.SetColumn(r, 1);
        g.Children.Add(t); g.Children.Add(r);
        _列表容器.Children.Add(g);
    }
}