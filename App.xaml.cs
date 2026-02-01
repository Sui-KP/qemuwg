using Microsoft.UI.Xaml;
using Microsoft.Windows.Globalization;
using WinRT.Interop;
namespace SuiQemu;

public partial class App : Application
{
    private Window? _窗口;
    public App()
    {
        ApplicationLanguages.PrimaryLanguageOverride = "zh-CN";
        InitializeComponent();
    }
    protected override void OnLaunched(LaunchActivatedEventArgs 参数)
    {
        var 临时窗口 = new Window();
        nint 句柄 = WindowNative.GetWindowHandle(临时窗口);
        临时窗口.Content = new 主页(句柄);
        临时窗口.ExtendsContentIntoTitleBar = true;
        _窗口 = 临时窗口;
        _窗口.Activate();
    }
}