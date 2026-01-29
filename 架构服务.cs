using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
namespace SuiQemu;
[SupportedOSPlatform("Windows10.0.22621.0")]
public class 架构服务
{
    private 架构服务() { }
    public static 架构服务 实例 { get; } = new 架构服务();
    public string? Qemu路径 { get; private set; }
    public List<string> 架构 { get; private set; } = [];
    public List<string> 机器 { get; private set; } = [];
    public List<string> 处理器 { get; private set; } = [];
    private readonly Dictionary<string, (List<string>, List<string>)> 缓存 = [];
    public string? 选架构, 选机器, 选处理器;
    public bool 加载中;
    public int 内存 = 2048;
    public string 磁盘 = "", 格式 = "qcow2", 总线 = "Virtio";
    public event Action? 刷新;
    public async Task 扫描(string p)
    {
        if (string.IsNullOrEmpty(p) || Qemu路径 == p) return;
        Qemu路径 = p;
        架构 = [];
        加载中 = true;
        刷新?.Invoke();
        var d = new DirectoryInfo(p);
        if (d.Exists)
        {
            架构 = [.. d.GetFiles("qemu-system-*.exe").Where(f => !f.Name.EndsWith("w.exe")).Select(f => f.Name[12..^4]).OrderBy(n => n)];
            if (架构.Count > 0) await 更新(架构[0]);
        }
        加载中 = false;
        刷新?.Invoke();
    }
    public async Task 更新(string n)
    {
        选架构 = n;
        if (缓存.TryGetValue(n, out var d))
        {
            (机器, 处理器) = d;
        }
        else
        {
            加载中 = true;
            刷新?.Invoke();
            var exe = Path.Combine(Qemu路径!, $"qemu-system-{n}.exe");
            var mTask = 运行(exe, "-machine help");
            var cTask = 运行(exe, "-cpu help");
            机器 = 解析(await mTask);
            处理器 = 解析(await cTask);
            缓存[n] = (机器, 处理器);
            加载中 = false;
        }
        选机器 = 机器.FirstOrDefault();
        选处理器 = 处理器.FirstOrDefault();
        刷新?.Invoke();
    }
    private static async Task<string> 运行(string f, string a)
    {
        using var p = Process.Start(new ProcessStartInfo(f, a) { RedirectStandardOutput = true, CreateNoWindow = true });
        return p == null ? "" : await p.StandardOutput.ReadToEndAsync();
    }
    private static List<string> 解析(string c) => [.. c.Split('\n').Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim().Split(' ')[0]).Distinct().OrderBy(s => s)];
    public string 指令() => Qemu路径 == null || 选架构 == null ? "" : $"\"{Path.Combine(Qemu路径, $"qemu-system-{选架构}.exe")}\" -machine {选机器} -cpu {选处理器} -m {内存} {(磁盘 == "" ? "" : $"-drive file=\"{磁盘}\",format={格式},if={总线.ToLower()}")}";
}