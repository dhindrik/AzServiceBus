namespace ServiceBusManager.Views;

public partial class MainView
{
    public MainView(MainViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;

        viewModel.AddAction($"open_{nameof(MessageDetailsView)}", ShowPopup);
        viewModel.AddAction($"close_{nameof(MessageDetailsView)}", ClosePopup);
    }



    private void ShowPopup(object message)
    {

        var parameter = ((ServiceBusReceivedMessage Message, bool IsDeadLetter))message;

        if (MessageDetails.BindingContext is MessageDetailsViewModel detailsViewModel)
        {
            MainThread.BeginInvokeOnMainThread(() => detailsViewModel.LoadMessage(parameter.Message, parameter.IsDeadLetter));
        }

        Popup.ScaleTo(1, 250, Easing.SinOut);
        var animation = new Animation((value) =>
        {
            Backdrop.Opacity = value;
        }, 0, 0.8);

        Backdrop.Animate("Opacity", animation, easing: Easing.SinOut);


    }

    private void ClosePopup()
    {
        Popup.ScaleTo(0, 250, Easing.SinOut);
        var animation = new Animation((value) =>
        {
            Backdrop.Opacity = value;
        }, 0.8, 0);

        Backdrop.Animate("Opacity", animation, easing: Easing.SinOut);
    }
}