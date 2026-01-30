using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
namespace SuiQemu;

public partial class 网络 : StackPanel
{
    public static 网络 实例 { get; } = new();
    public 网络参数 配置 { get; } = new();
    private readonly ComboBox _c设备;
    private readonly ProgressBar _p1;
    public 网络()
    {
        Spacing = 15; Padding = new Thickness(20);
        _p1 = new ProgressBar { IsIndeterminate = true, Visibility = Visibility.Collapsed };
        _c设备 = new ComboBox { Header = "网络设备", HorizontalAlignment = HorizontalAlignment.Stretch };
        _c设备.SelectionChanged += (s, e) => 配置.设备 = _c设备.SelectedValue?.ToString() ?? "";
        Children.Add(_p1);
        Children.Add(_c设备);
    }
    public async Task 刷新设备()
    {
        if (string.IsNullOrEmpty(架构.实例.数据.路径) || string.IsNullOrEmpty(架构.实例.数据.选架构)) return;
        _p1.Visibility = Visibility.Visible;
        var exe = System.IO.Path.Combine(架构.实例.数据.路径, $"qemu-system-{架构.实例.数据.选架构}.exe");
        var list = await Task.Run(() =>
        {
            var res = new List<string>();
            try
            {
                string script = $"& '{exe}' -device help 2>&1 | %{{ if($_ -match 'Network devices:'){{$s=1}} elseif($s){{ if($_ -match '^\\S'){{$s=0}} elseif($_ -match 'name \"([^\"]+)\"'){{ $Matches[1] }} }} }}";
                var info = new ProcessStartInfo { FileName = "powershell", Arguments = $"-NoProfile -Command \"{script}\"", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                using var p = Process.Start(info);
                if (p != null)
                {
                    while (!p.StandardOutput.EndOfStream)
                    {
                        var line = p.StandardOutput.ReadLine()?.Trim();
                        if (!string.IsNullOrEmpty(line)) res.Add(line);
                    }
                    p.WaitForExit();
                }
            }
            catch { }
            return res.Distinct().OrderBy(s => s).ToList();
        });
        _c设备.ItemsSource = list;
        if (list.Count > 0) _c设备.SelectedIndex = 0;
        _p1.Visibility = Visibility.Collapsed;
    }
}