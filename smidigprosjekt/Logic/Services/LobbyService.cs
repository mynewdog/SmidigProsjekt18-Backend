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
        Lobby GetTemporary();
    }
    /// <summary>
    /// Keeps a list of lobbies in memory,
    /// will also keep temporary lobbies for lobby
    /// creation.
    /// </summary>
    public class LobbyService : ILobbyService
    {
        //List of the all the Lobbies
        
        public List<Lobby> Lobbies { get; set; }

        public LobbyService()
        {
            Lobbies = new List<Lobby>();
        }
        public void Add(Lobby lobby)
        {
            Lobbies.Add(lobby);
        }

        public IEnumerable<Lobby> All()
        {
            return Lobbies;
        }
        
        public int Count()
        {
            return Lobbies.Count + Temporary.Count;
        }

        public void Delete(Lobby lobby)
        {
            Lobbies.Remove(lobby);
        }

        public Lobby GetTemporary()
        {
            throw new NotImplementedException();
        }
    }
}
