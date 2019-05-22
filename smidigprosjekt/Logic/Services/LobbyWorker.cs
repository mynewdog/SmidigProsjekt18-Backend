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

        /// <summary>
        /// Keep track of tags and weight of the rooms.
        /// if same tag match, add 1 to weight.
        /// </summary>
        public void tempRoomInit()
        {
            /*
             >30s>1min
             <get>
             <Fag[]> ++R
             <skole> ++R
             <interesser[]> ++R
             */
        }
        #endregion
        /// <summary>
        /// Create new rooms every 10 sek
        /// sudo sudo sudo
        /// </summary>
        /// <param name="user"></param>
        public void pulseLobby(User user)
        {
            var userList = _userService.GetHangoutUsers();
            //10 sek
            foreach (var User in userList)
            {
                //getTempRoom()

                //add.user -> tempRoom()
            }
        }
        

        /// <summary>
        /// OLD CODE
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

                // MatchUsers(userList)

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
        /// OLD CODE
        // MatchUsers(var userList )
        /*
         * Get tempRoom() = new temp;
         * get Room() = new room;
         * foreach(var user in userList)
         * {
         *  if(user.user.fag[] == user.user.fag[]) // can be Klasse[] instead of fag[]
         *      user.user.weightMatch = 4* // number of fag they match on
         *  if(user.user.linje == user.user.linje)  
         *      user.user.weightMatch = 3
         *  if(user.user.skole == user.user.skole)  
         *      user.user.weightMatch = 2
         *  if(user.user.interresser[] == user.user.interesser[])  
         *      user.user.weightMatch = 1+1* // number if interesser they match on // If they don't match on anything, have weight atleast 1
         *  _hub.Groups.AddToGroupsAsync(user.user.ConnectonID, room.temp);    
         * }     
         * _hub.Groups. AddToGroupsAsync -> lamba function to only max users with the highest weightMatch left ();
         * _hub.Clients.Group(room.LobbyName).SendAsync("JoinRoom", room);
         * */

    }
}
