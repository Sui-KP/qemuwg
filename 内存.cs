using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
namespace SuiQemu;

public partial class 内存 : StackPanel
{
    public 内存参数 配置 { get; } = new();
    private readonly NumberBox _n1, _n2;
    private readonly ComboBox _c1, _c2;
    private readonly ToggleSwitch _s1, _s2, _s3, _s4;
    public 内存()
    {
        Spacing = 20; Padding = new Thickness(20);
        _n1 = new NumberBox { Header = "内存容量 (MB)", Value = 2048, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline };
        _n2 = new NumberBox { Header = "最大限制 (MB)", Value = 4096, Visibility = Visibility.Collapsed, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline };
        _c1 = new ComboBox { Header = "内存插槽", ItemsSource = new List<int> { 0, 2, 4, 8, 16, 32 }, HorizontalAlignment = HorizontalAlignment.Stretch };
        _c2 = new ComboBox { Header = "NUMA 节点", ItemsSource = new List<int> { 1, 2, 4, 8 }, HorizontalAlignment = HorizontalAlignment.Stretch };
        _s1 = new ToggleSwitch { Header = "预分配内存" }; _s2 = new ToggleSwitch { Header = "大页内存" };
        _s3 = new ToggleSwitch { Header = "内存气球" }; _s4 = new ToggleSwitch { Header = "内存加密" };
        Children.Add(_n1); Children.Add(_n2); Children.Add(_c1); Children.Add(_c2);
        Children.Add(_s1); Children.Add(_s2); Children.Add(_s3); Children.Add(_s4);
        _n1.ValueChanged += (s, e) => 配置.容量 = (int)s.Value;
        _n2.ValueChanged += (s, e) => 配置.最大容量 = (int)s.Value;
        _c1.SelectionChanged += (s, e) => { if (_c1.SelectedItem is int v) { 配置.插槽 = v; _n2.Visibility = v > 0 ? Visibility.Visible : Visibility.Collapsed; } };
        _c2.SelectionChanged += (s, e) => { if (_c2.SelectedItem is int v) 配置.节点 = v; };
        _s1.Toggled += (s, e) => 配置.预分配 = _s1.IsOn;
        _s2.Toggled += (s, e) => 配置.大页 = _s2.IsOn;
        _s3.Toggled += (s, e) => 配置.气球 = _s3.IsOn;
        _s4.Toggled += (s, e) => 配置.加密 = _s4.IsOn;
        _n1.Value = 配置.容量; _c1.SelectedItem = 配置.插槽; _c2.SelectedItem = 配置.节点;
    }
}