using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
namespace SuiQemu;

public partial class 内存 : StackPanel
{
    public 内存参数 配置 { get; } = new();
    private readonly NumberBox 额度;
    private readonly ComboBox 插槽, 节点;
    private readonly ToggleSwitch 预分配, 大页, 气球, 加密;
    public 内存()
    {
        Padding = new Thickness(9);
        var 顶栏 = new Grid();
        顶栏.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        顶栏.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        顶栏.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        额度 = new NumberBox { Header = "内存", Value = 2048, SpinButtonPlacementMode = (NumberBoxSpinButtonPlacementMode)1, HorizontalAlignment = (HorizontalAlignment)3 };
        插槽 = new ComboBox { Header = "插槽", ItemsSource = new List<int> { 0, 2, 4, 8, 16, 32 }, HorizontalAlignment = (HorizontalAlignment)3 };
        节点 = new ComboBox { Header = "NUMA", ItemsSource = new List<int> { 1, 2, 4, 8 }, HorizontalAlignment = (HorizontalAlignment)3 };
        顶栏.Children.Add(额度); 顶栏.Children.Add(插槽); 顶栏.Children.Add(节点);
        Grid.SetColumn(额度, 0); Grid.SetColumn(插槽, 1); Grid.SetColumn(节点, 2);
        var 底栏 = new Grid();
        for (int i = 0; i < 4; i++) 底栏.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        预分配 = new ToggleSwitch { Header = "预分配", OnContent = "", OffContent = "" };
        大页 = new ToggleSwitch { Header = "大页", OnContent = "", OffContent = "" };
        气球 = new ToggleSwitch { Header = "气球", OnContent = "", OffContent = "" };
        加密 = new ToggleSwitch { Header = "加密", OnContent = "", OffContent = "" };
        底栏.Children.Add(预分配); 底栏.Children.Add(大页); 底栏.Children.Add(气球); 底栏.Children.Add(加密);
        Grid.SetColumn(预分配, 0); Grid.SetColumn(大页, 1); Grid.SetColumn(气球, 2); Grid.SetColumn(加密, 3);
        Children.Add(顶栏); Children.Add(底栏);
        额度.ValueChanged += (s, e) => 配置.容量 = (int)s.Value;
        插槽.SelectionChanged += (s, e) => { if (插槽.SelectedItem is int v) 配置.插槽 = v; };
        节点.SelectionChanged += (s, e) => { if (节点.SelectedItem is int v) 配置.节点 = v; };
        预分配.Toggled += (s, e) => 配置.预分配 = 预分配.IsOn;
        大页.Toggled += (s, e) => 配置.大页 = 大页.IsOn;
        气球.Toggled += (s, e) => 配置.气球 = 气球.IsOn;
        加密.Toggled += (s, e) => 配置.加密 = 加密.IsOn;
        额度.Value = 配置.容量; 插槽.SelectedItem = 配置.插槽; 节点.SelectedItem = 配置.节点;
    }
}