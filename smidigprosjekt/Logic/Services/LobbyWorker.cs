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
        /// NOT ACCURATE
        /// will pulse every 10 sek and swoop up all the users who have activated hangout.
        /// Add these users to a tempRoom(from LobbyService)
        /// Match/Merge func calls on tempRoom
        /// send tempLobby to client/Firebase via Add(from LobbyService
        /// The task that is executed frequently
        /// Will create lobbies and notify clients
        /// </summary>
        public void ConnectUsersToLobby() // PulseLobby
        {
            var hangoutUserCount = _userService.GetHangoutUserCount();
            
            //Send updates to all the connected clients 
            _userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("hangoutEvent", hangoutUserCount));
            _userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("infoGlobalEvent", _userService.Count()));

            if (hangoutUserCount >= _appConfig.MinimumPerLobby)
            {
                var userList = _userService.GetHangoutUsers();

                var temp = _lobbyService.GetTemporary();
                // Can be internal value of Lobby.cs as [] or list or map<>
                int Kristiania = 0; 
                //int Skole2 = 0;
                //int Skole3 = 0;
                int Studie1 = 0;
                //int Studie2 = 0;
                //int Studie3 = 0;

                foreach (var user in userList)
                {
                    if (temp.Members.TryAdd(user.user.Id, user.user) && (temp.Members.Count < temp.MaxUsers+1))
                    {
                        user.user.HangoutSearch = false;
                        
                        // Lobby's internal match scores, so that the group chat composistion will change.
                        // Match user to the lobbies composition as close as possible.
                        // Init if lobby was just created, and first member will tag it.
                        if(temp.Members.Count == 1)
                        {
                            temp.Institutt = user.user.Institutt;
                            temp.Studie = user.user.Studie;
                        }
                        if(user.user.Institutt == temp.Institutt)
                        {
                            Kristiania++; // adds to composition
                        }
                        if (user.user.Studie == temp.Studie)
                        {
                            Studie1++; // adds to composition
                        }
                        // will send the room to sorting, if full will joinable=false 
                        //therefor not become a new temp room in GetTemporary()
                        _lobbyService.SendRoom(temp); // TODO: send composition match score for merging
                        _hub.Groups.AddToGroupAsync(user.user.ConnectionId, temp.LobbyName);
                    }
                }
            }
        }
    }
}
