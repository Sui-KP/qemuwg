using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Runtime.Versioning;
using Windows.UI;
namespace SuiQemu;

[SupportedOSPlatform("Windows10.0.22621.0")]
public partial class 新仿真 : Grid
{
    private readonly string 仿真路径;
    private readonly Grid 侧边栏;
    private readonly Grid 内容区;
    private readonly TextBlock 标题文本;
    private readonly StackPanel 导航组;
    public 新仿真(string 路径)
    {
        仿真路径 = 路径;
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        侧边栏 = new Grid { Background = (SolidColorBrush)Application.Current.Resources["LayerFillColorDefaultBrush"] };
        侧边栏.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        侧边栏.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        var 顶部操作 = new StackPanel();
        顶部操作.Children.Add(创建图标按钮("\uE710", "新建", (s, e) => { }));
        顶部操作.Children.Add(创建图标按钮("\uE713", "设置", (s, e) => { }));
        Grid.SetRow(顶部操作, 0);
        导航组 = new StackPanel();
        导航组.Children.Add(new MenuFlyoutSeparator());
        导航组.Children.Add(创建导航按钮("\uE9CA", 0));
        导航组.Children.Add(创建导航按钮("\uE964", 1));
        导航组.Children.Add(创建导航按钮("\uC107", 2));
        导航组.Children.Add(创建导航按钮("\uE774", 3));
        Grid.SetRow(导航组, 1);
        侧边栏.Children.Add(顶部操作);
        侧边栏.Children.Add(导航组);
        Children.Add(侧边栏);
        Grid.SetColumn(侧边栏, 0);
        var 右侧区域 = new Grid();
        右侧区域.RowDefinitions.Add(new RowDefinition { Height = new GridLength(48) });
        右侧区域.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        var 顶部栏 = new Grid { BorderBrush = (SolidColorBrush)Application.Current.Resources["CardStrokeColorDefaultBrush"], BorderThickness = new Thickness(0, 0, 0, 0.5) };
        标题文本 = new TextBlock { Text = "架构", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
        顶部栏.Children.Add(标题文本);
        var 动作组 = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        动作组.Children.Add(创建图标按钮("\uE74E", "保存", (s, e) => { }, Colors.CornflowerBlue));
        动作组.Children.Add(创建图标按钮("\uE768", "启动", (s, e) => { }, Colors.LimeGreen));
        顶部栏.Children.Add(动作组);
        Grid.SetRow(顶部栏, 0);
        内容区 = new Grid { Padding = new Thickness(20) };
        Grid.SetRow(内容区, 1);
        右侧区域.Children.Add(顶部栏);
        右侧区域.Children.Add(内容区);
        Children.Add(右侧区域);
        Grid.SetColumn(右侧区域, 1);
        切换模块(0);
        _ = 架构服务.实例.扫描(路径);
    }
    private static Button 创建图标按钮(string 符号, string 提示, RoutedEventHandler 点击回调, Color? 颜色 = null)
    {
        var 按钮 = new Button
        {
            Content = new FontIcon { Glyph = 符号, FontSize = 16 },
            Width = 48,
            Height = 40,
            Background = new SolidColorBrush(Colors.Transparent),
            BorderThickness = new Thickness(0),
            Foreground = new SolidColorBrush(颜色 ?? (Color)Application.Current.Resources["SystemBaseHighColor"])
        };
        ToolTipService.SetToolTip(按钮, 提示);
        按钮.Click += 点击回调;
        return 按钮;
    }
    private Button 创建导航按钮(string 符号, int 索引)
    {
        return 创建图标按钮(符号, "", (s, e) => 切换模块(索引));
    }
    private void 切换模块(int 索引)
    {
        标题文本.Text = 索引 switch { 0 => "架构", 1 => "内存", 2 => "硬盘", 3 => "网络", _ => "" };
        内容区.Children.Clear();
        UIElement 模块视图 = 索引 switch
        {
            0 => new 架构模块视图(),
            _ => new TextBlock
            {
                Text = $"正在加载 {标题文本.Text} 模块...",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            }
        };
        内容区.Children.Add(模块视图);
    }
}