using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
namespace SuiQemu;

[Microsoft.UI.Xaml.Data.Bindable]
public partial class 主页 : Grid
{
    public ObservableCollection<string> Items { get; set; } = [];
    private readonly ApplicationDataContainer _存储 = ApplicationData.Current.LocalSettings;
    private readonly nint _窗口句柄;
    private List<(string 标题, UIElement 视图)>? _步骤组;
    private readonly TextBox _预览框 = new() { IsReadOnly = true, TextWrapping = TextWrapping.Wrap, Margin = new(18) };
    private int _当前步骤 = 0;
    public 主页(nint 句柄)
    {
        InitializeComponent();
        _窗口句柄 = 句柄;
        DataContext = this;
        Loaded += (s, e) => 刷新列表数据();
    }
    private void 刷新列表数据()
    {
        Items.Clear();
        if (_存储.Values["上次路径"] != null) 按钮标题.Text = "新仿真";
        if (_存储.Values["项目数据"] is string 串)
        {
            foreach (var 项 in 串.Split("||", StringSplitOptions.RemoveEmptyEntries)) Items.Add(项);
        }
    }
    private void 保存数据() => _存储.Values["项目数据"] = string.Join("||", Items);
    private async void 处理浏览按钮点击(object sender, RoutedEventArgs e)
    {
        if (按钮标题.Text == "新仿真") { 开启配置向导(); return; }
        var 选择器 = new FolderPicker { FileTypeFilter = { "*" } };
        InitializeWithWindow.Initialize(选择器, _窗口句柄);
        var 文件夹 = await 选择器.PickSingleFolderAsync();
        if (文件夹 != null) { _存储.Values["上次路径"] = 文件夹.Path; 刷新列表数据(); }
    }
    private void 开启配置向导()
    {
        if (_存储.Values["上次路径"] is not string 路径) return;
        磁盘.实例.窗口句柄 = _窗口句柄;
        _步骤组 ??= [("架构", 架构.实例), ("内存", new 内存()), ("磁盘", 磁盘.实例), ("网络", 网络.实例), ("显示", 显示.实例), ("预览", _预览框)];
        列表视图.Visibility = Visibility.Collapsed;
        仿真视图.Visibility = Visibility.Visible;
        定位步骤页面(0);
        _ = 架构.实例.扫描路径(路径);
    }
    private void 定位步骤页面(int 索引)
    {
        _当前步骤 = 索引;
        步骤标题.Text = _步骤组![索引].标题;
        配置容器.Children.Clear();
        if (索引 == 5)
        {
            _预览框.Text = 架构.实例.拼命令(new() { 内存 = ((内存)_步骤组[1].视图).配置, 磁盘 = 磁盘.实例.配置, 网络 = 网络.实例.配置, 显示 = 显示.实例.配置 });
            确认按钮.Content = "保存项目";
        }
        else 确认按钮.Content = "下一步";
        配置容器.Children.Add(_步骤组[索引].视图);
    }
    private void 切换步骤(object s, RoutedEventArgs e) { if (s is Button b && int.TryParse(b.Tag?.ToString(), out int i)) 定位步骤页面(i); }
    private void 推进流程(object s, RoutedEventArgs e)
    {
        if (_当前步骤 < 5) 定位步骤页面(++_当前步骤);
        else
        {
            if (!string.IsNullOrEmpty(_预览框.Text)) { Items.Add(_预览框.Text); 保存数据(); 刷新列表数据(); }
            关闭仿真视图(null!, null!);
        }
    }
    private void 关闭仿真视图(object s, RoutedEventArgs e) { 仿真视图.Visibility = Visibility.Collapsed; 列表视图.Visibility = Visibility.Visible; }
    public void 删除项目(object s, RoutedEventArgs e) { if (s is Button b && b.DataContext is string c) { Items.Remove(c); 保存数据(); 刷新列表数据(); } }
    public async void 弹出配置详情(object s, RoutedEventArgs e) { if (s is Button b && b.DataContext is string c) { var k = new TextBox { Text = c, IsReadOnly = true, TextWrapping = TextWrapping.Wrap, Height = 150 }; await new ContentDialog { Title = "配置详情", Content = k, CloseButtonText = "关闭", XamlRoot = XamlRoot }.ShowAsync(); } }
    public void 进入虚拟机详情(object s, RoutedEventArgs e)
    {
        if (s is Button b && b.DataContext is string m)
        {
            var x = new 详情(m);
            x.返回回调 = () => { 详情载体.Visibility = Visibility.Collapsed; 列表视图.Visibility = Visibility.Visible; 详情载体.Children.Clear(); };
            列表视图.Visibility = Visibility.Collapsed;
            详情载体.Children.Clear(); 详情载体.Children.Add(x); 详情载体.Visibility = Visibility.Visible;
        }
    }
}