using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace smidigprosjekt.Hubs
{
  [EnableCors("AllowAll")]
  [Authorize]
  public class TjommisHub : Hub
  {
    //Add override on virtual OnConnect()
    //Create connect message in chat

    
    public async Task SendMessage(string user, string message)
    {
      string getuser = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
      await Clients.All.SendAsync("messageBroadcastEvent", getuser, message);
    }
    public object Authenticate(string username, string password) { 
      if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) { 
        return new { authenticated=true } ;
      }
      else
      {
        return new { authenticated = false };
      }
    }
  }
}