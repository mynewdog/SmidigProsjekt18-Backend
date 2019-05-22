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
        private IUserService _userService { get; set; }
        private ILobbyService _lobbyService { get; set; }
        private AppConfiguration _appConfig;
        private IHubContext<TjommisHub> _hub { get; set; }
        
        public LobbyWorker(IUserService userService, ILobbyService lobbyService,
            IOptions<AppConfiguration> appconfig, IHubContext<TjommisHub> hubContext)
        {
            _lobbyService = lobbyService;
            _userService = userService;
            _appConfig = appconfig.Value;
            _hub = hubContext;
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
            
            //Send updates to all the connected clients 
            _userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("hangoutEvent", hangoutUserCount));
            _userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("infoGlobalEvent", _userService.Count()));

            if (hangoutUserCount >= _appConfig.MinimumPerLobby)
            {
                var userList = _userService.GetHangoutUsers();
                var rnd = new Random();

                Lobby room = new Lobby()
                {
                    Id = rnd.Next(),
                    Members = new System.Collections.Concurrent.ConcurrentDictionary<int, User>(),
                    Messages = new List<Message>()
                };
                foreach (var user in userList)
                {
                    if (room.Members.TryAdd(user.user.Id, user.user))
                    {
                        user.user.HangoutSearch = false;
                        _hub.Groups.AddToGroupAsync(user.user.ConnectionId, room.LobbyName);
                    }
                }
                _hub.Clients.Group(room.LobbyName).SendAsync("JoinRoom", room) ;

            }
        }
    }
}
