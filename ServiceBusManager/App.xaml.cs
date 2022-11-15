using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace ServiceBusManager;

public partial class App
{
    public App(ShellViewModel viewModel)
    {
        InitializeComponent();

//#if RELEASE
//        AppCenter.Start(SecretKeys.AppCenterProd, typeof(Analytics), typeof(Crashes));
//#else
//        AppCenter.Start(SecretKeys.AppCenterDev, typeof(Analytics), typeof(Crashes));
//#endif


        MainPage = new AppShell(viewModel);       

       // SetMainWindowStartSize(1650, 1000);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window =  base.CreateWindow(activationState);

        window.MaximumHeight = 1000;
        window.MinimumHeight = 1000;
        window.MaximumWidth = 1650;
        window.MinimumWidth = 1650;

        return window;
    }
}

