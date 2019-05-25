using smidigprosjekt.Logic.Database;
using smidigprosjekt.Models;
using smidigprosjekt.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace smidigprosjekt.Logic.Services
{
    public interface ILobbyService
    {
        IEnumerable<Lobby> All();
        void Add(Lobby lobby);
        void Delete(Lobby lobby);
        void SendRoom(Lobby lobby);
        int Count();
        Lobby GetTemporary(User user);
    }
    /// <summary>
    /// Keeps a list of lobbies in memory,
    /// will also keep temporary lobbies for lobby
    /// creation.
    /// </summary>
    public class LobbyService : ILobbyService
    {
        private AppConfiguration _appConfig;
        private IUserService _userService { get; set; }
        private IHubContext<TjommisHub> _hub { get; set; }
        //List of the all the Lobbies
        public List<Lobby> Lobbies { get; set; }

        public LobbyService()
        {
            Lobbies = new List<Lobby>();
        }
        // Active rooms save to Firebase
        public void Add(Lobby lobby)
        {
            Task.Run(() => FirebaseDbConnection.saveRooms(Lobbies));
        }
        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="lobby"></param>
        public void SendRoom(Lobby lobby)// add parameter of composistion tags
        {
            if(Count() > 1) // too few active rooms
            {
                if (lobby.MaxUsers == lobby.Members.Count) // Room reach max size
                {
                    lobby.Joinable = false; // no longer in the pool of temp rooms
                    _hub.Clients.Group(lobby.LobbyName).SendAsync("joinroom", lobby);
                    Add(lobby); // Firebase
                }
                else
                {
                    // Try to merge with most equal room composition.
                    // then various not filled rooms
                    // recursive asking after merge to see if rooms are max.

                }
            }
            // Only 1 room active, not wait for users, no matching
            else
            {
                lobby.Joinable = false; // no longer in the pool of temp rooms
                _hub.Clients.Group(lobby.LobbyName).SendAsync("JoinRoom", lobby);
                Add(lobby); // Firebase
            }            
        }

        /// <summary>
        /// TODO: Will match compostion and move users
        /// </summary>
        /// <param name="lobby1"></param>
        /// <param name="lobby2"></param>
        public void Merge(Lobby lobby1, Lobby lobby2)
        {
            //lobb1 ++ lobby2
            // -> users lobby2) -||-.users
            // delete lobby1
        }

        public IEnumerable<Lobby> All()
        {
            return Lobbies;
        }

        public int Count()
        {
            return Lobbies.Count;
        }

        public void Delete(Lobby lobby)
        {
            Lobbies.Remove(lobby);
        }

        public Lobby GetTemporary(User user)
        {
            var rnd = new Random();
            var rndId = rnd.Next();
            var availableRooms = Lobbies.Where(e => e.Joinable).OrderByDescending(e => e.Members.Count);
            // Creates a new room
            if (availableRooms.Count() == 0)
            {
                Lobby room = new Lobby()
                {
                    Studie = user.Studie,
                    Institutt = user.Institutt,
                    LobbyName = $"{user.Institutt} - {user.Studie}: {rndId}",
                    Created = DateTime.UtcNow,
                    Joinable = true,
                    Id = rndId,
                    Members = new HashSet<User>(),
                    Messages = new List<Message>(),
                    MaxUsers = _appConfig.MaximumPerLobby
                };
                Lobbies.Add(room);
                return room;
            }
            // Returns the most populated room
            else
            {
                //Create algorithm to sort the first room to be returned
                return availableRooms.First();
            }
        }
    }
}
