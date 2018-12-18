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
    void Add(Lobby lobby);
    void Delete(Lobby lobby);
    int Count();
  }
  public class LobbyService : ILobbyService
  {
    public List<Lobby> Lobbies;
    public LobbyService()
    {
      Lobbies = new List<Lobby>();
    }
    public void Add(Lobby lobby)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<Lobby> All()
    {
      throw new NotImplementedException();
    }

    public int Count()
    {
      throw new NotImplementedException();
    }

    public void Delete(Lobby lobby)
    {
      throw new NotImplementedException();
    }
  }
}
