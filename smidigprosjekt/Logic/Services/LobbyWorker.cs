﻿using Microsoft.AspNetCore.SignalR;
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

            var hangoutUserCount = _userService.GetHangoutUserCount();
            //Send updates to all the connected clients 
            //_userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("hangoutevent", new HangoutEventMessage() {

            _hub.Clients.All.SendAsync("hangoutevent", new HangoutEventMessage()
            {
                TimeStamp = DateTime.UtcNow,
                TotalUsers = hangoutUserCount
            });
            //_userService.GetConnectedConnections().ToList().ForEach(e => e.SendAsync("infoGlobalEvent", _userService.Count()));

        }

        private void TruncateRoomList()
        {
            foreach (var room in _lobbyService.All().Where(e => e.Joinable == false))
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
Temporary Lobbies: {0}\
Total Lobbies: {1}
"
, _lobbyService.All().Where(e => e.Joinable).Count()
, _lobbyService.All().Count()
);


            foreach (var lob in _lobbyService.GetTemporaryRooms())
            {
                _lobbyService.SendRoom(lob); // TODO: send composition match score for merging
            }
        }
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

            if (hangoutUserCount >= _appConfig.MinimumPerLobby)
            {
                var userSessionList = _userService.GetHangoutUsers();
                // Can be internal value of Lobby.cs as [] or list or map<>
                int Kristiania = 0; 
                //int Skole2 = 0;
                //int Skole3 = 0;
                int Studie1 = 0;
                //int Studie2 = 0;
                //int Studie3 = 0;

                //iterate trough every user that is 
                //queued for hangout and is _not_ in a 
                //room or _temporary_ room
                foreach (var userSession in userSessionList)
                {
                    var temp = _lobbyService.FindMatchingLobby(userSession.user);
                    //Inform all the other clients that a new user joined the room
                    _hub.Clients.Group(temp.LobbyName).SendAsync("userjoin",userSession.user.ConvertToSanitizedUser());
                    //Send lobbyinfo to the new user about the
                    //add curent user to the lobby
                    temp.Members.Add(userSession.user);
                    //current status of the temporary lobby
                    userSession.proxy.SendAsync("lobbyinfo", temp.ConvertToSanitizedLobby());
                    //Add user to SignalR group
                    _hub.Groups.AddToGroupAsync(userSession.user.ConnectionId, temp.LobbyName);

                    //User is now in lobby, switch hangoutsearch to false
                    userSession.user.HangoutSearch = false;

                    userSession.user.Lobbies.Add(temp);

                    if ((temp.Members.Count < temp.MaxUsers+1) || temp.Created.AddSeconds(60) < DateTime.UtcNow)
                    {
                        
                        // Lobby's internal match scores, so that the group chat composistion will change.
                        // Match user to the lobbies composition as close as possible.
                        // Init if lobby was just created, and first member will tag it.
                        if(userSession.user.Institutt == temp.Institutt)
                        {
                            Kristiania++; // adds to composition
                        }
                        if (userSession.user.Studie == temp.Studie)
                        {
                            Studie1++; // adds to composition
                        }
                        // will send the room to sorting, if full will joinable=false 
                        //therefor not become a new temp room in GetTemporary()
                    }
                }
            }


        }
    }

    public class HangoutEventMessage
    {
        public DateTime TimeStamp { get; set; }
        public int TotalUsers { get; set; }
    }
}
