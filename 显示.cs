using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
namespace SuiQemu;

public partial class 显示 : StackPanel
{
    public static 显示 实例 { get; } = new();
    public 显示参数 配置 { get; } = new();
    private readonly ComboBox _c设备;
    private readonly ProgressBar _p1;
    public 显示()
    {
        Spacing = 15; Padding = new Thickness(9);
        _p1 = new ProgressBar { IsIndeterminate = true, Visibility = Visibility.Collapsed };
        _c设备 = new ComboBox { Header = "显示设备 (VGA)", HorizontalAlignment = HorizontalAlignment.Stretch };
        _c设备.SelectionChanged += (s, e) => 配置.设备 = _c设备.SelectedValue?.ToString() ?? "";
        Children.Add(_p1);
        Children.Add(_c设备);
    }
    public async Task 刷新设备()
    {
        if (string.IsNullOrEmpty(架构.实例.数据.路径) || string.IsNullOrEmpty(架构.实例.数据.选架构)) return;
        _p1.Visibility = Visibility.Visible;
        string 执行路径 = System.IO.Path.Combine(架构.实例.数据.路径, $"qemu-system-{架构.实例.数据.选架构}.exe");
        var 设备列表 = await Task.Run(() =>
        {
            var 结果 = new List<string>();
            try
            {
                var 启动信息 = new ProcessStartInfo
                {
                    FileName = 执行路径,
                    Arguments = "-device help",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var 进程 = Process.Start(启动信息);
                if (进程 != null)
                {
                    string 所有文本 = 进程.StandardOutput.ReadToEnd() + 进程.StandardError.ReadToEnd();
                    进程.WaitForExit();
                    bool 处于显示段 = false;
                    var 行集合 = 所有文本.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var 原始行 in 行集合)
                    {
                        var 行 = 原始行.Trim();
                        if (行.Contains("Display devices:"))
                        {
                            处于显示段 = true;
                            continue;
                        }
                        if (处于显示段)
                        {
                            if (行.Contains("name \""))
                            {
                                int 起始 = 行.IndexOf("name \"") + 6;
                                int 结束 = 行.IndexOf("\"", 起始);
                                if (起始 > 5 && 结束 > 起始)
                                {
                                    结果.Add(行.Substring(起始, 结束 - 起始));
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(行) && !行.StartsWith(" "))
                            {
                                if (结果.Count > 0) 处于显示段 = false;
                            }
                        }
                    }
                }
            }
            catch { }
            return 结果.Distinct().OrderBy(s => s).ToList();
        });
        _c设备.ItemsSource = 设备列表;
        if (设备列表.Count > 0) _c设备.SelectedIndex = 0;
        _p1.Visibility = Visibility.Collapsed;
    }
}