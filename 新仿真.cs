using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
namespace SuiQemu;

public partial class 新仿真 : Grid
{
    private readonly Grid _内容区 = new();
    private readonly TextBlock _标题 = new() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
    private readonly 架构 _架构视图 = 架构.实例;
    private readonly 内存 _内存视图 = new();
    private readonly 磁盘 _磁盘视图 = 磁盘.实例;
    private readonly 网络 _网络视图 = 网络.实例;
    private int _步 = 0;
    public 新仿真(string 路径, nint 窗口句柄)
    {
        _磁盘视图.窗口句柄 = 窗口句柄;
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var 侧边 = new StackPanel { Background = (SolidColorBrush)Application.Current.Resources["LayerFillColorDefaultBrush"] };
        侧边.Children.Add(创建按钮("\uE9CA", 0));
        侧边.Children.Add(创建按钮("\uE964", 1));
        侧边.Children.Add(创建按钮("\uE7C3", 2));
        侧边.Children.Add(创建按钮("\uE12B", 3));
        侧边.Children.Add(创建按钮("\uE756", 4));
        var 右侧 = new Grid();
        右侧.RowDefinitions.Add(new RowDefinition { Height = new GridLength(48) });
        右侧.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        右侧.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var 底部 = new Button { Content = "下一步", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(20), Style = (Style)Application.Current.Resources["AccentButtonStyle"] };
        底部.Click += (s, e) => { if (_步 < 4) 切换(++_步); };
        Grid.SetRow(_内容区, 1); Grid.SetRow(底部, 2);
        右侧.Children.Add(_标题); 右侧.Children.Add(_内容区); 右侧.Children.Add(底部);
        Children.Add(侧边); Children.Add(右侧);
        Grid.SetColumn(右侧, 1);
        切换(0); _ = _架构视图.扫描路径(路径);
    }
    private Button 创建按钮(string g, int i)
    {
        var b = new Button { Content = new FontIcon { Glyph = g, FontSize = 16 }, Width = 48, Height = 48, Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent), BorderThickness = new Thickness(0) };
        b.Click += (s, e) => 切换(i);
        return b;
    }
    private void 切换(int i)
    {
        _步 = i; _内容区.Children.Clear();
        _标题.Text = i switch { 0 => "架构设置", 1 => "内存配置", 2 => "存储磁盘", 3 => "网络设置", _ => "命令预览" };
        if (i == 4)
        {
            var p = new 仿真配置 { 内存 = _内存视图.配置, 磁盘 = _磁盘视图.配置, 网络 = _网络视图.配置 };
            _内容区.Children.Add(new TextBox { Text = _架构视图.拼命令(p), IsReadOnly = true, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(20), AcceptsReturn = true });
        }
        else
        {
            UIElement view = i switch { 0 => _架构视图, 1 => _内存视图, 2 => _磁盘视图, 3 => _网络视图, _ => new Grid() };
            _内容区.Children.Add(view);
        }
    }
}