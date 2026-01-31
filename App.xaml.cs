using Microsoft.UI.Xaml;
using Microsoft.Windows.Globalization;
namespace SuiQemu;
public partial class App : Application
{
    private Window? _窗口;
    public App()
    {
        ApplicationLanguages.PrimaryLanguageOverride = "zh-Hans";
        InitializeComponent();
    }
    protected override void OnLaunched(LaunchActivatedEventArgs 参数)
    {
        _窗口 = new 主窗口();
        _窗口.Activate();
    }
}