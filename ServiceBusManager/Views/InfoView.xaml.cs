using System.Runtime.CompilerServices;

namespace ServiceBusManager.Views;

public partial class InfoView : ContentView
{
    private readonly InfoViewModel? viewModel;
    private readonly ILogService logService;

    public static BindableProperty QueueNameProperty = BindableProperty.Create(nameof(QueueName), typeof(string), typeof(InfoView), null, defaultBindingMode: BindingMode.TwoWay,
       propertyChanged: (bindable, oldValue, newValue) =>
       {
           if (bindable is InfoView infoView && infoView.Content.BindingContext is InfoViewModel viewModel && newValue is string queueName)
           {
               if (oldValue == newValue)
               {
                   return;
               }

               infoView.TopicName = null;

               MainThread.BeginInvokeOnMainThread(async () => await viewModel.Load(queueName));
           }
       });

    public static BindableProperty TopicNameProperty = BindableProperty.Create(nameof(TopicName), typeof(string), typeof(InfoView), null, defaultBindingMode: BindingMode.TwoWay,
      propertyChanged: (bindable, oldValue, newValue) =>
      {
          if(oldValue == newValue)
          {
              return;
          }

          if (bindable is InfoView infoView && infoView.Content.BindingContext is InfoViewModel viewModel && newValue is string queueName)
          {
              infoView.QueueName = null;

              MainThread.BeginInvokeOnMainThread(async () => await viewModel.LoadTopic(queueName));

              
          }
      });

    public InfoView()
    {
        InitializeComponent();

        viewModel = Resolver.Resolve<InfoViewModel>();
        var log = Resolver.Resolve<ILogService>();

        if(log == null)
        {
            throw new Exception("ILogService need to be added to IoC");
        }

        logService = log;

        Content.BindingContext = viewModel;
    }

    public string? QueueName
    {
        get => GetValue(QueueNameProperty) as string;
        set => SetValue(QueueNameProperty, value);
    }

    public string? TopicName
    {
        get => GetValue(TopicNameProperty) as string;
        set => SetValue(TopicNameProperty, value);
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if(propertyName == nameof(IsVisible) && IsVisible)
        {
            Task.Run(async () => await logService.LogPageView(nameof(InfoView)));
        }
    }
}
