using System.Reflection.Metadata;
using CommunityToolkit.Maui.Views;

namespace ServiceBusManager.Views;

public partial class MainView
{
    private readonly ILogService logService;

    public MainView(MainViewModel viewModel, ILogService logService)
	{
		InitializeComponent();

		BindingContext = viewModel;

        viewModel.AddAction($"open_{nameof(MessageDetailsView)}", ShowPopup);
        viewModel.AddAction($"close_{nameof(MessageDetailsView)}", ClosePopup);

        viewModel.AddAction($"open_{nameof(NewMessageView)}", ShowNewPopup);
        viewModel.AddAction($"close_{nameof(NewMessageView)}", CloseNewPopup);

        viewModel.AddAction($"open_premium", ShowPremiumPopup);
        viewModel.AddAction($"close_premium", ClosePremiumPopup);

        this.logService = logService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Task.Run(async () => await logService.LogPageView(nameof(MainView)));
    }

    private void ShowPopup(object message)
    {

        var parameter = ((ServiceBusReceivedMessage Message, bool IsDeadLetter, string? TopicName))message;

        if (MessageDetails.BindingContext is MessageDetailsViewModel detailsViewModel)
        {
            MainThread.BeginInvokeOnMainThread(() => detailsViewModel.LoadMessage(parameter.Message, parameter.IsDeadLetter, parameter.TopicName));
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

    private void ShowNewPopup()
    {

        if (NewMessage.BindingContext is NewMessageViewModel viewModel)
        {
            MainThread.BeginInvokeOnMainThread(async() => await viewModel.Initialize());
        }
        NewPopup.ScaleTo(1, 250, Easing.SinOut);
        var animation = new Animation((value) =>
        {
            Backdrop.Opacity = value;
        }, 0, 0.8);
        Backdrop.Animate("Opacity", animation, easing: Easing.SinOut);
    }

    private void CloseNewPopup()
    {

        NewPopup.ScaleTo(0, 250, Easing.SinOut);
        var animation = new Animation((value) =>
        {
            Backdrop.Opacity = value;
        }, 0.8, 0);

        Backdrop.Animate("Opacity", animation, easing: Easing.SinOut);
    }

    private void ShowPremiumPopup()
    {      

        PremiumPopup.ScaleTo(1, 250, Easing.SinOut);
        var animation = new Animation((value) =>
        {
            Backdrop.Opacity = value;
        }, 0, 0.8);

        Backdrop.Animate("Opacity", animation, easing: Easing.SinOut);


    }

    private void ClosePremiumPopup()
    {
        PremiumPopup.ScaleTo(0, 250, Easing.SinOut);
        var animation = new Animation((value) =>
        {
            Backdrop.Opacity = value;
        }, 0.8, 0);

        Backdrop.Animate("Opacity", animation, easing: Easing.SinOut);
    }
}