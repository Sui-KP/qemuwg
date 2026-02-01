using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
namespace SuiQemu;

public partial class 详情 : Grid
{
    private Process? _虚拟机进程;
    private readonly string _启动命令;
    public Action? 返回回调 { get; set; }
    public 详情(string 命令)
    {
        this.InitializeComponent();
        _启动命令 = 命令;
        配置文本.Text = 命令;
    }
    private void 处理控制点击(object sender, RoutedEventArgs e)
    {
        if (_虚拟机进程 == null || _虚拟机进程.HasExited) 启动虚拟机();
        else 停止虚拟机();
    }
    private void 启动虚拟机()
    {
        日志框.Text = "[系统] 正在建立重定向管道...\n";
        try
        {
            string 文件, 参数;
            if (_启动命令.StartsWith('\"'))
            {
                int 偏移 = _启动命令.IndexOf('\"', 1);
                文件 = _启动命令[1..偏移];
                参数 = _启动命令[(偏移 + 1)..].Trim();
            }
            else
            {
                int 空格 = _启动命令.IndexOf(' ');
                if (空格 == -1) { 文件 = _启动命令; 参数 = ""; }
                else { 文件 = _启动命令[..空格]; 参数 = _启动命令[空格..].Trim(); }
            }
            var 信息 = new ProcessStartInfo
            {
                FileName = 文件,
                Arguments = 参数,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                WorkingDirectory = Path.GetDirectoryName(文件)
            };
            _虚拟机进程 = new Process { StartInfo = 信息, EnableRaisingEvents = true };
            _虚拟机进程.OutputDataReceived += (s, args) => 写入日志(args.Data);
            _虚拟机进程.ErrorDataReceived += (s, args) => 写入日志(args.Data);
            _虚拟机进程.Exited += (s, args) => DispatcherQueue.TryEnqueue(() =>
            {
                操作按钮.Content = "启动";
                状态文本.Text = "已停止";
                写入日志("[系统] 虚拟机进程已退出");
            });
            _虚拟机进程.Start();
            _虚拟机进程.BeginOutputReadLine();
            _虚拟机进程.BeginErrorReadLine();
            操作按钮.Content = "停止";
            状态文本.Text = "运行中";
        }
        catch (Exception 错) { 写入日志($"[错误] {错.Message}"); }
    }
    private void 写入日志(string? 内容)
    {
        if (string.IsNullOrEmpty(内容)) return;
        DispatcherQueue.TryEnqueue(() =>
        {
            日志框.Text += 内容 + "\n";
            日志框.Select(日志框.Text.Length, 0);
        });
    }
    private void 停止虚拟机()
    {
        try { if (_虚拟机进程 != null && !_虚拟机进程.HasExited) _虚拟机进程.Kill(); }
        catch (Exception 错) { 写入日志($"[系统] 强行停止失败: {错.Message}"); }
    }
    private void 处理返回点击(object sender, RoutedEventArgs e)
    {
        停止虚拟机();
        返回回调?.Invoke();
    }
}