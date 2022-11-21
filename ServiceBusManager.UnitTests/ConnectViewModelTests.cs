namespace ServiceBusManager.UnitTests;

public class ConnectViewModelTests
{
    [Fact]
    public async Task OnParameterSetTest()
    {
        //Arrange
        var connectionService = Substitute.For<IConnectionService>();
        var featureService = Substitute.For<IFeatureService>();
        var logService = Substitute.For<ILogService>();

        var viewModel = new ConnectViewModel(connectionService, featureService, logService);
        viewModel.NavigationParameter = "new";

        //Act
        await viewModel.OnParameterSet();

        //Assert
        viewModel.ShowNew.Should().BeTrue();
        viewModel.ShowConnections.Should().BeFalse();
    }

    [Fact]
    public async Task OnParameterSetTest_InvalidValue()
    {
        //Arrange
        var connectionService = Substitute.For<IConnectionService>();
        var featureService = Substitute.For<IFeatureService>();
        var logService = Substitute.For<ILogService>();

        var viewModel = new ConnectViewModel(connectionService, featureService, logService);
        viewModel.NavigationParameter = "old";

        //Act
        await viewModel.OnParameterSet();

        //Assert
        viewModel.ShowNew.Should().BeFalse();
        viewModel.ShowConnections.Should().BeFalse();
    }

    [Fact]
    public async Task OnAppearingTest()
    {
        //Arrange
        var connectionService = Substitute.For<IConnectionService>();
        connectionService.Get().Returns(Task.FromResult(new List<Models.ConnectionInfo>()
        {
            new Models.ConnectionInfo(),
            new Models.ConnectionInfo()
        }));

        var featureService = Substitute.For<IFeatureService>();
        var logService = Substitute.For<ILogService>();

        var viewModel = new ConnectViewModel(connectionService, featureService, logService);

        //Act
        await viewModel.OnAppearing();

        //Assert
        viewModel.Connections.Should().HaveCount(2);
        viewModel.ShowConnections.Should().BeTrue();
    }

    [Fact]
    public async Task OnAppearing_NoSavedConnectionsTest()
    {
        //Arrange
        var connectionService = Substitute.For<IConnectionService>();
        connectionService.Get().Returns(Task.FromResult(new List<Models.ConnectionInfo>()));

        var featureService = Substitute.For<IFeatureService>();
        var logService = Substitute.For<ILogService>();

        var viewModel = new ConnectViewModel(connectionService, featureService, logService);

        //Act
        await viewModel.OnAppearing();

        //Assert
        viewModel.Connections.Should().HaveCount(0);
        viewModel.ShowConnections.Should().BeFalse();
        viewModel.ShowNew.Should().BeTrue();
    }

    [Fact]
    public void OpenConnectionCommandTest()
    {
        //Arrange
        var connectionService = Substitute.For<IConnectionService>();

        var featureService = Substitute.For<IFeatureService>();
        var logService = Substitute.For<ILogService>();

        TinyNavigation.Current = Substitute.For<TinyMvvm.INavigation>();

        var viewModel = new ConnectViewModel(connectionService, featureService, logService);
        viewModel.ConnectionString = "Test";

        //Act
        viewModel.OpenConnectionCommand.Execute(null);

        //Assert
        Received.InOrder(async () =>
        {
            await viewModel.Navigation.NavigateTo($"///{nameof(MainViewModel)}", viewModel.ConnectionString);
        });
    }

    [Fact]
    public void SaveAndConnectToNewCommandTest()
    {
        //Arrange
        var connectionService = Substitute.For<IConnectionService>();

        var featureService = Substitute.For<IFeatureService>();
        var logService = Substitute.For<ILogService>();

        TinyNavigation.Current = Substitute.For<TinyMvvm.INavigation>();

        var name = "Test";
        var connectionString = "Test";

        var viewModel = new ConnectViewModel(connectionService, featureService, logService);
        viewModel.Name = name;
        viewModel.ConnectionString = connectionString;

        //Act
        viewModel.SaveAndConnectToNewCommand.Execute(null);

        //Assert
        Received.InOrder(async () =>
        {
            await connectionService.Save(Arg.Is<ConnectionInfo>(x => x.Name == name && x.Value == connectionString));
            await viewModel.Navigation.NavigateTo($"///{nameof(MainViewModel)}", connectionString);
        });
    }

    [Fact]
    public void ConnectToNewCommandTest()
    {
        //Arrange
        var connectionService = Substitute.For<IConnectionService>();

        var featureService = Substitute.For<IFeatureService>();
        var logService = Substitute.For<ILogService>();

        TinyNavigation.Current = Substitute.For<TinyMvvm.INavigation>();

        var name = "Test";
        var connectionString = "Test";

        var viewModel = new ConnectViewModel(connectionService, featureService, logService);
        viewModel.Name = name;
        viewModel.ConnectionString = connectionString;

        //Act
        viewModel.SaveAndConnectToNewCommand.Execute(null);

        //Assert
        Received.InOrder(async () =>
        {
            await viewModel.Navigation.NavigateTo($"///{nameof(MainViewModel)}", connectionString);
        });
    }

    [Fact]
    public void ValidateSaveTest()
    {
        //Arrange
        var connectionService = Substitute.For<IConnectionService>();

        var featureService = Substitute.For<IFeatureService>();
        var logService = Substitute.For<ILogService>();

        var viewModel = new ConnectViewModel(connectionService, featureService, logService);
        viewModel.Name = string.Empty;
        viewModel.ConnectionString = string.Empty;

        //Act
        viewModel.SaveAndConnectToNewCommand.Execute(null);

        //Assert
        viewModel.ShowSaveValidationMessage.Should().BeTrue();
        viewModel.ShowConnectValidationMessage.Should().BeTrue();
    }

    [Fact]
    public void ValidateConnectTest()
    {
        //Arrange
        var connectionService = Substitute.For<IConnectionService>();

        var featureService = Substitute.For<IFeatureService>();
        var logService = Substitute.For<ILogService>();

        var viewModel = new ConnectViewModel(connectionService, featureService, logService);
        viewModel.Name = "Test";
        viewModel.ConnectionString = string.Empty;

        //Act
        viewModel.SaveAndConnectToNewCommand.Execute(null);

        //Assert
        viewModel.ShowSaveValidationMessage.Should().BeFalse();
        viewModel.ShowConnectValidationMessage.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveCommandTest()
    {
        //Arrange
        var connectionService = Substitute.For<IConnectionService>();

        var featureService = Substitute.For<IFeatureService>();
        var logService = Substitute.For<ILogService>();


        var toRemove = new ConnectionInfo()
        {
            Id = "test"
        };

        var toKeep = new ConnectionInfo()
        {
            Id = "keep"
        };

        connectionService.Get().Returns(Task.FromResult(new List<Models.ConnectionInfo>()
        {
            toKeep,
            toRemove
        }));


        var viewModel = new ConnectViewModel(connectionService, featureService, logService);
        await viewModel.OnAppearing();       

        //Act
        viewModel.RemoveCommand.Execute(toRemove);

        //Assert
        viewModel.Connections.Should().HaveCount(1);
        viewModel.Connections.First().Should().Be(toKeep);

        Received.InOrder(async() => {
            await connectionService.Remove(toRemove);
        });
    }
}
 