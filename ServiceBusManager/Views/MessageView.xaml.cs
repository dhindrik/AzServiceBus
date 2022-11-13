using System.Runtime.CompilerServices;

namespace ServiceBusManager.Views;

public partial class MessageView
{
    private readonly MessageViewModel? viewModel;

    public static BindableProperty QueueNameProperty = BindableProperty.Create(nameof(QueueName), typeof(string), typeof(MessageView), null, defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) =>
    {
        if (bindable is MessageView messageView  && messageView.MainContent.BindingContext is MessageViewModel viewModel && newValue is string queueName)
        {
            MainThread.BeginInvokeOnMainThread(async () => await viewModel.LoadMessages(queueName));
        }
    });

    public static BindableProperty DeadLetterQueueNameProperty = BindableProperty.Create(nameof(DeadLetterQueueName), typeof(string), typeof(MessageView), null, defaultBindingMode: BindingMode.TwoWay,
       propertyChanged: (bindable, oldValue, newValue) =>
       {
           if (bindable is MessageView messageView && messageView.Content.BindingContext is MessageViewModel viewModel && newValue is string queueName)
           {
               MainThread.BeginInvokeOnMainThread(async () => await viewModel.LoadMessages(queueName, true));
           }
       });


    public static BindableProperty SubscriptionNameProperty = BindableProperty.Create(nameof(SubscriptionName), typeof(string), typeof(MessageView), null, defaultBindingMode: BindingMode.TwoWay,
     propertyChanged: (bindable, oldValue, newValue) =>
     {
         if (bindable is MessageView messageView && messageView.Content.BindingContext is MessageViewModel viewModel && newValue is string queueName)
         {
             MainThread.BeginInvokeOnMainThread(async () => await viewModel.LoadMessages(queueName, false, true));
         }
     });

    public static BindableProperty DeadLetterSubscriptionNameProperty = BindableProperty.Create(nameof(DeadLetterSubscriptionName), typeof(string), typeof(MessageView), null, defaultBindingMode: BindingMode.TwoWay,
      propertyChanged: (bindable, oldValue, newValue) =>
      {
          if (bindable is MessageView messageView && messageView.Content.BindingContext is MessageViewModel viewModel && newValue is string queueName)
          {
              MainThread.BeginInvokeOnMainThread(async () => await viewModel.LoadMessages(queueName, true, true));
          }
      });


    public MessageView()
    {
        InitializeComponent();

        viewModel = Resolver.Resolve<MessageViewModel>();

        MainContent.BindingContext = viewModel;
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


    public string? SubscriptionName
    {
        get => GetValue(SubscriptionNameProperty) as string;
        set => SetValue(SubscriptionNameProperty, value);
    }


    public string? DeadLetterSubscriptionName
    {
        get => GetValue(DeadLetterSubscriptionNameProperty) as string;
        set => SetValue(DeadLetterSubscriptionNameProperty, value);
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
    }
}
