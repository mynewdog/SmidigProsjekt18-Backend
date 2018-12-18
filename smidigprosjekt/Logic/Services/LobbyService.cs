using smidigprosjekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Logic.Services
{
  public interface ILobbyService
  {
    IEnumerable<Lobby> All();
    int Count();
  }
  public class LobbyService : ILobbyService
  {
    public IEnumerable<Lobby> All()
    {
      throw new NotImplementedException();
    }

    public int Count()
    {
      throw new NotImplementedException();
    }
  }
}
