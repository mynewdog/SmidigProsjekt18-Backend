using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Models
{
  public enum LobbyDependency { Location, Date, Agreement}
  public class Poll
  {
    public string Description { get; set; }
    public Lobby ParentLobby { get; set; }
    public List<bool> Votes { get; set; }
    public List<User> Voters { get; set; }
    public LobbyDependency Option { get; set; }
  }
}
