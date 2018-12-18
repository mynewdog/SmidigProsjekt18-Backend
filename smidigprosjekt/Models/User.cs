using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Models
{
  public class User
  {
    public int Id { get; set; }
    public string ConnectionId { get; set; }

    public string Username { get; set; }
    public UserConfiguration Configuration { get; set; }
    public bool HangoutSearch { get; set; }
    public List<Lobby> Lobbies {get;set;}
    //The client connected state
    public bool Connected { get; set; }
  }
  public class UserConfiguration
  {
    public List<string> Interests { get; set; }
  }
}
