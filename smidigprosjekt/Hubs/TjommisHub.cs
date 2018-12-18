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
      user.HangoutSearch = true;
      await Clients.Caller.SendAsync("hangoutEvent",_userService.GetHangoutUserCount());
    }

    /// <summary>
    /// Called when a new connection is established with the hub.
    /// </summary>
    /// <returns>A System.Threading.Tasks.Task that represents the asynchronous connect</returns>
    public override async Task OnConnectedAsync()
    {
      string userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
      if (string.IsNullOrEmpty(userName))
      {
        throw new Exception("Cannot identify user with connectionid: " + Context.ConnectionId);
      };

      //Create a new userobject and add it to userservice
      _userService.Add(new User()
      {
        Id = _userService.Count() + 1,
        ConnectionId = Context.ConnectionId,
        Connected = true,
        Username = userName,
        Lobbies = new List<Lobby>(), //Empty lobbylist
        HangoutSearch = false,
        Configuration = new UserConfiguration()
        {
          Interests = new List<string>()
          {
            "Computer Science",
            "Painting",
            "Complaining"
          }
        }
      },Clients.Caller);


      await Clients.Caller.SendAsync("infoConnectEvent",userName);
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
}