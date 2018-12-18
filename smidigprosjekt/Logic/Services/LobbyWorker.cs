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
  /// <summary>
  /// LobbyWorker
  /// Author: Erik Alvarez
  /// Date: 18.12.2018
  ///   Creates lobbies based on UserService parameters
  ///   Runs on a frequent timer
  /// </summary>
  public class LobbyWorker : BackgroundService
  {
    #region init
    private IUserService _userService;
    private ILobbyService _lobbyService;
    private AppConfiguration _appConfig;
    public LobbyWorker(IUserService userService, ILobbyService lobbyService, IOptions<AppConfiguration> appconfig)
    {
      _lobbyService = lobbyService;
      _userService = userService;
      _appConfig = appconfig.Value;
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
    #endregion

    /// <summary>
    /// The task that is executed frequently
    /// Will create lobbies and notify clients
    /// </summary>
    public void ConnectUsersToLobby()
    {
      var hangoutUserCount = _userService.GetHangoutUserCount();
      _userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("hanoutEvent", hangoutUserCount));
      _userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("infoGlobalEvent", _userService.Count()));
      if (hangoutUserCount >= _appConfig.MinimumPerLobby)
      {

      }
    }
  }
}
