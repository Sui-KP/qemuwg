using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace SuiQemu;

public partial class 网络 : StackPanel
{
    public static 网络 实例 { get; } = new();
    public 网络参数 配置 { get; } = new();
    private readonly ComboBox _c设备 = new() { Header = "网络设备", HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly ProgressBar _p1 = new() { IsIndeterminate = true, Visibility = Visibility.Collapsed };
    public 网络()
    {
        Spacing = 15; Padding = new Thickness(9);
        _c设备.SelectionChanged += (s, e) => 配置.设备 = _c设备.SelectedValue?.ToString() ?? "";
        Children.Add(_p1);
        Children.Add(_c设备);
    }
    public async Task 刷新设备()
    {
        var d = 架构.实例.数据;
        if (string.IsNullOrEmpty(d.路径) || string.IsNullOrEmpty(d.选架构)) return;
        _p1.Visibility = Visibility.Visible;
        string exe = Path.Combine(d.路径, $"qemu-system-{d.选架构}.exe");
        _c设备.ItemsSource = await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo(exe, "-device help") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                using var p = Process.Start(psi);
                string outText = p?.StandardOutput.ReadToEnd() ?? "";
                var match = MyRegex().Match(outText);
                string section = match.Success ? match.Groups[1].Value : outText;
                return [.. MyRegex1().Matches(section)
                    .Select(m => m.Groups[1].Value)
                    .Distinct().OrderBy(s => s)];
            }
            catch { return new List<string>(); }
        });
        if (_c设备.Items.Count > 0) _c设备.SelectedIndex = 0;
        _p1.Visibility = Visibility.Collapsed;
    }

    [GeneratedRegex(@"Network devices:(.*?)\r?\n\r?\n", RegexOptions.Singleline)]
    private static partial Regex MyRegex();
    [GeneratedRegex(@"name ""([^""]+)""")]
    private static partial Regex MyRegex1();
}