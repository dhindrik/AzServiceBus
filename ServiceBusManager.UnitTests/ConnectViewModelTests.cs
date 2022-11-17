
using System;
using Microsoft.Maui.Controls;
using ServiceBusManager.Models;
using TinyMvvm;

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
}
