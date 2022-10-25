namespace ServiceBusManager;

public partial class App
{
    public App(ShellViewModel viewModel)
    {
        InitializeComponent();

        MainPage = new AppShell(viewModel);

        SetMainWindowStartSize(1650, 1000);
    }


    private void SetMainWindowStartSize(int width, int height)
    {
        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(
            nameof(IWindow), (handler, view) =>
            {
                var size = new CoreGraphics.CGSize(width, height);

                if (handler.PlatformView.WindowScene != null && handler.PlatformView.WindowScene.SizeRestrictions != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        handler.PlatformView.WindowScene.SizeRestrictions.MinimumSize = size;
                        handler.PlatformView.WindowScene.SizeRestrictions.MaximumSize = size;
                    });
                }
            });
    }
}

