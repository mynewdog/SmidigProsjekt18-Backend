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
using smidigprosjekt.Logic.Services;
using Microsoft.Extensions.Logging;

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
        private ILogger _logger { get; set; }
        
        public LobbyWorker(
            IUserService userService, 
            ILobbyService lobbyService,
            IOptions<AppConfiguration> appconfig, 
            IHubContext<TjommisHub> hubContext,
            ILoggerFactory loggerFactory)
        {
            _lobbyService = lobbyService;
            _userService = userService;
            _appConfig = appconfig.Value;
            _hub = hubContext;
            _logger = loggerFactory.CreateLogger<LobbyWorker>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"LobbyWorker task started doing background work.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ConnectUsersToLobby();
                    ProcessTemporaryRooms();
                    NotifyClients();
                    TruncateRoomList();
                }
                catch (Exception e)
                {
                    _logger.LogError("A fatal error occured somewhere in LobbyWorker: {0}",e?.Message);
                }
                await Task.Delay(_appConfig.LobbyWorkerDelay, stoppingToken);
            }
        }

        private void NotifyClients()
        {

            //Send updates to all the connected clients 
            //_userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("hangoutevent", new HangoutEventMessage() {
            foreach (var lobby in _lobbyService.All().Where(e=>e.Joinable)) {
                var hangoutUserCount = lobby.Members.Count;
                _hub.Clients.Group(lobby.LobbyName).SendAsync("hangoutevent", new HangoutEventMessage()
                {
                    Room = lobby.ConvertToSanitizedLobby(),
                    TimeStamp = DateTime.UtcNow,
                    TotalUsers = hangoutUserCount,
                    TimeRunning = (DateTime.UtcNow - lobby.Created).TotalSeconds
                });
            };
            //_userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("infoGlobalEvent", _userService.Count()));
        }

        private void TruncateRoomList()
        {
            //Clean old rooms
            foreach (var room in _lobbyService.All())
            {
                if (room.Created.AddHours(1) < DateTime.UtcNow) {
                    _logger.LogInformation("Deleting room: ", room.LobbyName);
                    _lobbyService.Delete(room);
                }
            }
        }
        #endregion

        public void ProcessTemporaryRooms()
        {
            // Output useful statistics during debugging to test if this is working as intended
            _logger.LogInformation(
            @"LobbyWorkerInfo:
            Temporary Lobbies:  {0}     Total users in active lobbies: {1}
            Total Lobbies:      {2}     Total users in temp lobbies:   {3}
            "
            , _lobbyService.All().Where(e => e.Joinable).Count()
            , _lobbyService.All().Where(e => !e.Joinable).Sum(e => e.Members.Count)
            , _lobbyService.All().Count()
            , _lobbyService.All().Where(e=>e.Joinable).Sum(e=>e.Members.Count)
            );
            var tempRooms = _lobbyService.GetTemporaryRooms();
            var oldTempRooms = tempRooms.Where(e => e.Created.AddSeconds(_appConfig.LobbyHangTimeout) < DateTime.UtcNow && e.MaxUsers > e.Members.Count());
            if (oldTempRooms.Count() >= 2)
            {
               _lobbyService.Merge(oldTempRooms.First(), oldTempRooms.Last()); // merge old and not full rooms
            };
            foreach (var lob in _lobbyService.GetTemporaryRooms())
            {
                _lobbyService.SendRoom(lob); // sends rooms to client
            };
        }
        /// <summary>
        /// will pulse every 10 sek and swoop up all the users who have activated hangout.
        /// </summary>
        public void ConnectUsersToLobby()
        {
                var userSessionList = _userService.GetHangoutUsers();
                foreach (var userSession in userSessionList)
                {
                    var temp = _lobbyService.FindMatchingLobby(userSession.user);
                    //Inform all the other clients that a new user joined the room
                    _hub.Clients.Group(temp.LobbyName).SendAsync("userjoin",userSession.user.ConvertToSanitizedUser());
                    //Send lobbyinfo to the new user about the
                    //add curent user to the lobby
                    temp.Members.Add(userSession.user);
                    if (temp.Members.Count >= temp.MaxUsers) temp.Joinable = false;
                    //current status of the temporary lobby
                    userSession.proxy.SendAsync("lobbyinfo", temp.ConvertToSanitizedLobby());
                    //Add user to SignalR group
                    _hub.Groups.AddToGroupAsync(userSession.user.ConnectionId, temp.LobbyName);

                    //User is now in lobby, switch hangoutsearch to false
                    userSession.user.HangoutSearch = false;
                    userSession.user.SingleHangoutSearch = false;
                    userSession.user.Lobbies.Add(temp);
                }
        }
    }

    public class HangoutEventMessage
    {
        public DateTime TimeStamp { get; set; }
        public int TotalUsers { get; set; }
        public double TimeRunning { get; internal set; }
        public TjommisLobby Room { get; internal set; }
    }
}
