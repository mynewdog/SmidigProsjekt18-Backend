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
    //Add override on virtual OnConnect()
    //Create connect message in chat
    
    public async Task SendMessage(string message)
    {
      string getuser = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
      await Clients.All.SendAsync("messageBroadcastEvent", getuser, message);
    }

    //
    // Summary:
    //     Called when a new connection is established with the hub.
    //
    // Returns:
    //     A System.Threading.Tasks.Task that represents the asynchronous connect.
    
    public override async Task OnConnectedAsync()
    {
      string getuser = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
      var newUser = new User()
      {
        Id = _userService.Count() + 1,
        ConnectionId = Context.ConnectionId,
        Connected = true,
        Username = getuser,
        Lobbies = new List<Lobby>(),
        HangoutSearch = false,
        Configuration = new UserConfiguration()
        {
          Interests = new List<string>()
          {
            "test"
          }
        }
      };
      _userService.Add(newUser,Clients.Caller);
      await Clients.Caller.SendAsync("infoConnectEvent",getuser);
      await Clients.All.SendAsync("messageBroadcastEvent", "system", getuser + " connected. (" + _userService.Count() + ")");
    }


    //
    // Summary:
    //     Called when a connection with the hub is terminated.
    //
    // Returns:
    //     A System.Threading.Tasks.Task that represents the asynchronous disconnect.
    public override async Task OnDisconnectedAsync(Exception exception)
    {
      User user = _userService.GetUserFromConnectionId(Context.ConnectionId);
      await Clients.All.SendAsync("messageBroadcastEvent", "system", user.Username + " disconnected.");
      _userService.Disconnect(Context.ConnectionId);
    }
  }
}