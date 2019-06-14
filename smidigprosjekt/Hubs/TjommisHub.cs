using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using smidigprosjekt.Logic.Services;
using smidigprosjekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace smidigprosjekt.Hubs
{
    [EnableCors("AllowAll")]
    [Authorize]
    public class TjommisHub : Hub
    {
        private IInterestProviderService _interestProvider;
        private IUserService _userService;
        private ILobbyService _lobbyService;

        public TjommisHub(IUserService userService, ILobbyService lobbyService, IInterestProviderService interestProvider)
        {
            _interestProvider = interestProvider;
            _userService = userService;
            _lobbyService = lobbyService;
        }
        /// <summary>
        /// Sends a message to all clients
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>A task that represents the asychronous communication</returns>
        public async Task SendMessage(string lobby, string message)
        {
            
            string getuser = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            //Check if user is allowed
            if (_userService.GetUserConfiguration(getuser)?
                .Lobbies.FirstOrDefault(l => l.LobbyName.Contains(lobby)) == null) return;

            Message msg = new Message()
            {
                //Lobby = //_userService.GetUserConfiguration(getuser).Lobbies.First().ConvertToSanitizedLobby(),
                Text = message,
                Timestamp = DateTime.UtcNow,
                Type = MessageType.User,
                User = getuser //_userService.GetUserConfiguration(getuser).ConvertToSanitizedUser()
            };
            
            
            await Clients.Group(lobby).SendAsync("message",lobby, msg);

            var room = _lobbyService.All().Where(e => e.LobbyName == lobby).First();
            //save messages for reloading
            room.Messages.Add(msg);
        }
        public async Task EnterLobby(TjommisUser user,TjommisLobby Lobby)
        {
            var userValidation = _userService.GetUserFromConnectionId(Context.ConnectionId);
            //Check if valid userrequest and that user is still conntected
            if (user.Username.Contains(userValidation.Username)) 
            {
                //check if user is actually in this room
                var lobbyValidation = userValidation.Lobbies.SingleOrDefault(e => e.LobbyName == Lobby.LobbyName);
                if (lobbyValidation != null)
                {
                    await Groups.AddToGroupAsync(userValidation.ConnectionId, Lobby.LobbyName);
                    await Clients.Caller.SendAsync("enterroom", lobbyValidation.ConvertToSanitizedLobby());
                }
            }
            else
            {
                Console.WriteLine("Could not validate user");
            }
        }
        public async Task<ConnectionEventInfo> UpdateInterests(List<string> Interests)
        {
            var user = _userService.GetUserFromConnectionId(Context.ConnectionId);
            if (user != null)
            {
                user.Configuration.Interests = Interests;
                await _userService.UpdateUser(user);


                return new ConnectionEventInfo()
                {
                    UserInfo = new UserConnectionInfo()
                    {
                        Username = user.Username,
                        Interests = user.Configuration.Interests,
                        Lobbies = user.Lobbies.Select(e=> e.ConvertToSanitizedLobby())
                    },
                    InterestList = _interestProvider.GetAll(),
                };
                
            }
            return null;
        }
        public bool HangoutGroup()
        {
            var user = _userService.GetUserFromConnectionId(Context.ConnectionId);
            if (user != null)
            {
                user.HangoutSearch = true;
                user.SingleHangoutSearch = false;
                //await Clients.Caller.SendAsync("hangoutevent", _userService.GetHangoutUserCount());
                return true;
            }
            return false;
        }
        public bool HangoutSingle()
        {
            var user = _userService.GetUserFromConnectionId(Context.ConnectionId);
            if (user != null)
            {
                user.SingleHangoutSearch = true;
                user.HangoutSearch = false;
                //await Clients.Caller.SendAsync("hangoutevent", _userService.GetHangoutUserCount());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when a new connection is established with the hub.
        /// </summary>
        /// <returns>A System.Threading.Tasks.Task that represents the asynchronous connect</returns>
        public override async Task OnConnectedAsync()
        {
            string userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var user = _userService.GetUserConfiguration(userName);

            if (user == null)
            {
                throw new Exception("Cannot identify user with connectionid: " + Context.ConnectionId);
            };
            
            user.Connected = true;
            user.ConnectionId = Context.ConnectionId;

            // Clean up user lobbies before we send the list, we can only send to lobbies that exists in lobbyservice.
            // Could exist older / stale lobbies in User object from earlier
            // This is because of lobbyworker at current state removes old rooms
            // In the future, this will not be an issue, and this function will deprechate
            user.Lobbies.RemoveWhere(e => !_lobbyService.All().Contains(e));

            var connectionEvent = new ConnectionEventInfo()
            {
                UserInfo = new UserConnectionInfo()
                {
                    Username = user.Username,
                    Interests = user.Configuration?.Interests,
                    Lobbies = user.Lobbies.Select(e => e.ConvertToSanitizedLobby())
                },
                InterestList = _interestProvider.GetAll(),
            };
            await Clients.Caller.SendAsync("infoConnectEvent", connectionEvent);
            
            _userService.ConnectUser(user, Clients.Caller);
           
        }
        /// <summary>
        /// Called when a connection with the hub is terminated.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>A System.Threading.Tasks.Task that represents the asynchronous disconnect.</returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            User user = _userService.GetUserFromConnectionId(Context.ConnectionId);
            user.Connected = false;
            await Clients.All.SendAsync("messageBroadcastEvent", "system", user.Username + " disconnected.");
            _userService.Disconnect(Context.ConnectionId);
        }
    }

    public class ConnectionEventInfo
    {
        public UserConnectionInfo UserInfo { get; set; }
        public IList<InterestItem> InterestList { get; set; }
    }

    public class UserConnectionInfo
    {
        public string Username { get; set; }
        public IEnumerable<TjommisLobby> Lobbies {get;set;}
        public List<string> Interests { get; internal set; }
    }
}
