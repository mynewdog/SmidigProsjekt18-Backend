using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using smidigprosjekt.Logic.Services;
using smidigprosjekt.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace smidigprosjekt.Hubs
{
    [EnableCors("AllowAll")]
    [Authorize]
    public class TjommisHub : Hub
    {
        private IUserService _userService;
        public TjommisHub(IUserService userService)
        {
            _userService = userService;
        }
        /// <summary>
        /// Sends a message to all clients
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>A task that represents the asychronous communication</returns>
        public async Task SendMessage(string message)
        {
            string getuser = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            await Clients.All.SendAsync("messageBroadcastEvent", getuser, message);
        }

        public async Task Hangout()
        {
            var user = _userService.GetUserFromConnectionId(Context.ConnectionId);
            if (user != null) { 
                user.HangoutSearch = true;
                await Clients.Caller.SendAsync("hangoutEvent", _userService.GetHangoutUserCount());
            }
        }
        public bool TestHangout()
        {
            var user = _userService.GetUserFromConnectionId(Context.ConnectionId);
            if (user != null)
            {
                user.HangoutSearch = true;
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
            _userService.ConnectUser(user, Clients.Caller);

            await Clients.Caller.SendAsync("infoConnectEvent", new ConnectionEventInfo()
            {
                UserInfo = new UserConnectionInfo()
                {
                    Username = user.Username,
                    Lobbies = user.Lobbies
                },
                InterestList = _userService.Interests,
            });

            await Clients.Caller.SendAsync("infoGlobalEvent", _userService.Count());
            await Clients.All.SendAsync("messageBroadcastEvent", "system", userName + " connected. (" + _userService.Count() + ")");
        }

        /// <summary>
        /// Called when a connection with the hub is terminated.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>A System.Threading.Tasks.Task that represents the asynchronous disconnect.</returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            User user = _userService.GetUserFromConnectionId(Context.ConnectionId);
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
        public List<Lobby> Lobbies {get;set;}
    }
}
