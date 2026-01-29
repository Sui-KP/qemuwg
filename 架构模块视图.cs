using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
namespace SuiQemu;
[SupportedOSPlatform("Windows10.0.22621.0")]
public partial class 架构模块视图 : Grid
{
    private readonly 架构服务 S = 架构服务.实例;
    private readonly ComboBox C1, C2, C3;
    private readonly TextBox T1;
    private readonly ProgressBar P1;
    public 架构模块视图()
    {
        var sp = new StackPanel { Spacing = 10, Padding = new Thickness(20) };
        sp.Children.Add(P1 = new ProgressBar { IsIndeterminate = true, Visibility = Visibility.Collapsed });
        sp.Children.Add(C1 = new ComboBox { Header = "架构", HorizontalAlignment = HorizontalAlignment.Stretch });
        sp.Children.Add(C2 = new ComboBox { Header = "机器", HorizontalAlignment = HorizontalAlignment.Stretch });
        sp.Children.Add(C3 = new ComboBox { Header = "处理器", HorizontalAlignment = HorizontalAlignment.Stretch });
        sp.Children.Add(T1 = new TextBox { Header = "预览", IsReadOnly = true, TextWrapping = TextWrapping.Wrap, MinHeight = 80 });
        C1.SelectionChanged += async (s, e) => { if (C1.SelectedItem is string v && v != S.选架构) await S.更新(v); };
        C2.SelectionChanged += (s, e) => { S.选机器 = C2.SelectedItem as string; T1.Text = S.指令(); };
        C3.SelectionChanged += (s, e) => { S.选处理器 = C3.SelectedItem as string; T1.Text = S.指令(); };
        Children.Add(new ScrollViewer { Content = sp });
        S.刷新 += () => DispatcherQueue.TryEnqueue(() => {
            P1.Visibility = S.加载中 ? Visibility.Visible : Visibility.Collapsed;
            绑定(C1, S.架构, S.选架构);
            绑定(C2, S.机器, S.选机器);
            绑定(C3, S.处理器, S.选处理器);
            T1.Text = S.指令();
        });
    }
    private static void 绑定(ComboBox c, List<string> d, string? v)
    {
        if (c.ItemsSource != d) { c.ItemsSource = null; c.ItemsSource = d; }
        if (v != null && d.Contains(v)) c.SelectedItem = v;
        else if (d.Count > 0) c.SelectedIndex = 0;
    }
}