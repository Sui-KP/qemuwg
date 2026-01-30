using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
namespace SuiQemu;

public partial class 主窗口 : Window
{
    readonly TextBlock 显示文本;
    readonly Button 功能按钮;
    readonly Grid 主布局;
    readonly ApplicationDataContainer 本地配置 = ApplicationData.Current.LocalSettings;
    public 主窗口()
    {
        ExtendsContentIntoTitleBar = true;
        SystemBackdrop = new MicaBackdrop { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
        显示文本 = new TextBlock { Text = "速仿", FontSize = 28, FontWeight = Microsoft.UI.Text.FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center };
        功能按钮 = new Button { Content = "选择速仿所在文件夹", HorizontalAlignment = HorizontalAlignment.Center };
        功能按钮.Click += 点击处理;
        var 内容面板 = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Spacing = 20, Children = { 显示文本, 功能按钮 } };
        主布局 = new Grid { Children = { 内容面板 } };
        Content = 主布局;
        执行初始化检查();
    }
    private void 执行初始化检查()
    {
        if (本地配置.Values["上次路径"] is string 路径) 更新界面状态(路径);
    }
    private async void 点击处理(object 来源, RoutedEventArgs 参数)
    {
        if (功能按钮.Content.ToString() == "新仿真")
        {
            if (本地配置.Values["上次路径"] is string 路径) 执行新仿真(路径);
            return;
        }
        FolderPicker 文件夹选择器 = new();
        InitializeWithWindow.Initialize(文件夹选择器, WindowNative.GetWindowHandle(this));
        文件夹选择器.FileTypeFilter.Add("*");
        var 文件夹 = await 文件夹选择器.PickSingleFolderAsync();
        if (文件夹 == null) return;
        本地配置.Values["上次路径"] = 文件夹.Path;
        更新界面状态(文件夹.Path);
    }
    private async void 更新界面状态(string 文件夹路径)
    {
        string 版本文件 = Path.Combine(文件夹路径, "VERSION");
        string 版本信息 = File.Exists(版本文件) ? (await File.ReadAllTextAsync(版本文件)).Trim() : "";
        显示文本.Text = $"速仿 {版本信息}".Trim();
        功能按钮.Content = "新仿真";
    }
    private void 执行新仿真(string 文件夹路径)
    {
        nint handle = WindowNative.GetWindowHandle(this);
        Content = new 新仿真(文件夹路径, handle);
    }
}