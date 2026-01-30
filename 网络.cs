using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
namespace SuiQemu;

public partial class 网络 : StackPanel
{
    public static 网络 实例 { get; } = new();
    public 网络参数 配置 { get; } = new();
    private readonly ComboBox _c设备;
    public 网络()
    {
        Spacing = 15; Padding = new Thickness(20);
        _c设备 = new ComboBox
        {
            Header = "网络设备",
            ItemsSource = new List<string> { "e1000", "virtio-net-pci", "rtl8139" },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            SelectedIndex = 0
        };
        _c设备.SelectionChanged += (s, e) => 配置.设备 = _c设备.SelectedValue?.ToString() ?? "e1000";
        Children.Add(_c设备);
        Children.Add(new TextBlock { Text = "说明: 选择虚拟机使用的网卡硬件型号。", Opacity = 0.6 });
    }
}