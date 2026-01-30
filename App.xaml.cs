using Microsoft.UI.Xaml;
namespace SuiQemu;

public partial class App : Application
{
    private Window? _窗口;
    public App()
    {
        InitializeComponent();
    }
    protected override void OnLaunched(LaunchActivatedEventArgs 参数)
    {
        _窗口 = new 主窗口();
        _窗口.Activate();
    }
}