using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace smidigprosjekt.Hubs
{
  [EnableCors("AllowAllHeaders")]
  public class TjommisHub : Hub
  {
    //Add override on virtual OnConnect()
    //Create connect message in chat

    
    public async Task SendMessage(string user, string message)
    {
      object[] args = { user, message };
      await Clients.All.SendAsync("messageBroadcastEvent", user, message);
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