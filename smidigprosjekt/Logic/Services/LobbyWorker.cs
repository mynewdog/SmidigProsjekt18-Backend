using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using smidigprosjekt.Hubs;
using smidigprosjekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace smidigprosjekt.Logic.Services
{
  public class LobbyWorker : BackgroundService
  {
    private IUserService _userService;
    private ILobbyService _lobbyService;
    private AppConfiguration _appAonfig;
    public LobbyWorker(IUserService userService, ILobbyService lobbyService, IOptions<AppConfiguration> appconfig)
    {
      _lobbyService = lobbyService;
      _userService = userService;
      _appAonfig = appconfig.Value;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      
      while (!stoppingToken.IsCancellationRequested)
      {
        Console.WriteLine($"LobbyWorker task doing background work.");
        ConnectUsersToLobby();
        await Task.Delay(10000, stoppingToken);
      }
    }

    public void ConnectUsersToLobby()
    {
      var randomNumber = (new Random()).Next(1,999999);
      _userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("updateGuiTestEvent", randomNumber));
      _userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("infoGlobalEvent", _userService.Count()));
    }
  }
}
