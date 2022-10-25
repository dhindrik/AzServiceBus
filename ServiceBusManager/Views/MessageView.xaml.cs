using System.Runtime.CompilerServices;

namespace ServiceBusManager.Views;

public partial class MessageView
{
    private readonly MessageViewModel? viewModel;

    public static BindableProperty QueueNameProperty = BindableProperty.Create(nameof(QueueName), typeof(string), typeof(MessageView), null, defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) =>
    {
        if (bindable is MessageView messageView  && messageView.Content.BindingContext is MessageViewModel viewModel && newValue is string queueName)
        {
            MainThread.BeginInvokeOnMainThread(async () => await viewModel.LoadQueueMessages(queueName));
        }
    });

    public static BindableProperty DeadLetterQueueNameProperty = BindableProperty.Create(nameof(DeadLetterQueueName), typeof(string), typeof(MessageView), null, defaultBindingMode: BindingMode.TwoWay,
       propertyChanged: (bindable, oldValue, newValue) =>
       {
           if (bindable is MessageView messageView && messageView.Content.BindingContext is MessageViewModel viewModel && newValue is string queueName)
           {
               MainThread.BeginInvokeOnMainThread(async () => await viewModel.LoadQueueMessages(queueName, true));
           }
       });


    public MessageView()
    {
        InitializeComponent();

        viewModel = Resolver.Resolve<MessageViewModel>();

        Content.BindingContext = viewModel;
    }


    public string? QueueName
    {
        get => GetValue(QueueNameProperty) as string;
        set => SetValue(QueueNameProperty, value);
    }


    public string? DeadLetterQueueName
    {
        get => GetValue(DeadLetterQueueNameProperty) as string;
        set => SetValue(DeadLetterQueueNameProperty, value);
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
    }
}
