using smidigprosjekt.Logic.Database;
using smidigprosjekt.Models;
using smidigprosjekt.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace smidigprosjekt.Logic.Services
{
    public interface ILobbyService
    {
        IEnumerable<Lobby> All();
        void Add(Lobby lobby);
        void Delete(Lobby lobby);
        void SendRoom(Lobby lobby);
        int Count();
        //Get temporary room based on user criteria
        Lobby FindMatchingLobby(User user);
        Lobby Newroom(User user);
        IEnumerable<Lobby> GetTemporaryRooms();

    }
    // Lobby map<id,priority> used in FindMatchingLobby(User user)
    public class MatchRoom
    {
        public Lobby Room { get; set; }
        public int Prio { get; set; }
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
        private ILogger<LobbyService> _logger;

        //List of the all the Lobbies
        public List<Lobby> Lobbies { get; set; }

        public LobbyService(IOptions<AppConfiguration> appconfig, ILoggerFactory loggerFactory, IHubContext<TjommisHub> tjommisHub)
        {
            _logger = loggerFactory.CreateLogger<LobbyService>();
            Lobbies = new List<Lobby>();
            _appConfig = appconfig.Value;
            _hub = tjommisHub;
        }
        // Active rooms save to Firebase
        public void Add(Lobby lobby)
        {
            Task.Run(() => FirebaseDbConnection.addRoom(lobby));
        }
        public void Delete(Lobby lobby)
        {
            Task.Run(() => FirebaseDbConnection.removeRoom(lobby));
            Lobbies.Remove(lobby);
        }

        public IEnumerable<Lobby> All()
        {
            return Lobbies;
        }

        public int Count()
        {
            return Lobbies.Count;
        }

        public IEnumerable<Lobby> GetTemporaryRooms()
        {
            return Lobbies.Where(e => e.Joinable);
        }

        public Lobby FindMatchingLobby(User user)
        {
            IEnumerable<MatchRoom> matchList;
            if (user.HangoutSearch)
            {
                matchList = Lobbies.Where(e => e.Joinable).Select(e => new MatchRoom() { Room = e, Prio = 0 });
            }
            else
            {
                matchList = Lobbies.Where(e => e.Joinable && e.MaxUsers == 2).Select(e => new MatchRoom() { Room = e, Prio = 0 });
            }
            foreach (var lob in matchList)
            {
                if (lob.Room.Institutt.Contains(user.Institutt, StringComparison.OrdinalIgnoreCase))
                {
                    lob.Prio += 10;
                }
                if (lob.Room.Studie.Contains(user.Studie, StringComparison.OrdinalIgnoreCase))
                {
                    lob.Prio += 3;
                }
                //Match on interests list
                lob.InterestMatch(user);
            }
            // Match on interests
            //foreach (var lobby in matchList) lobby.InterestMatch(user);
            var bestRoom = matchList.OrderByDescending(e => e.Prio);
            // Did not match on any room, creates new room based on the user
            if (bestRoom.First().Prio == 0)
            {
                return Newroom(user);
            }
            else
            {
                return bestRoom.First().Room;
            }
        }

        /// <summary>
        /// Creates a new lobby based on the first users stats.
        /// Returns this room and add it to Firebase.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Lobby Newroom(User user)
        {
            _logger.LogInformation("Creating new room");
            var rnd = new Random();
            var rndId = rnd.Next();
            Lobby Newroom = new Lobby()
            {
                Studie = user.Studie,
                Institutt = user.Institutt,
                LobbyName = $"{user.Institutt.Trim()}-{user.Studie.Trim()}-{rndId}",
                Created = DateTime.UtcNow,
                Joinable = true,
                Id = rndId,
                Members = new HashSet<User>(),
                Messages = new List<Message>(),
                MaxUsers = user.SingleHangoutSearch ? _appConfig.MaximumPerLobby : 2
            };
            Lobbies.Add(Newroom);
            return Newroom;
        }

        /// <summary>
        /// Not used yet, nice to have
        /// </summary>
        /// <param name="lobby1"></param>
        /// <param name="lobby2"></param>
        public void Merge(Lobby lobby1, Lobby lobby2)
        {
            //lobb1 ++ lobby2
            // -> users lobby2) -||-.users
            // delete lobby1
        }

        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="lobby"></param>
        public void SendRoom(Lobby lobby)
        {
            if (lobby.MaxUsers == lobby.Members.Count) // Room reach max size
            {
                _logger.LogInformation("Lobby full! :) sending joinable room to closed lobby");
                lobby.Joinable = false; // no longer in the pool of temp rooms
                _hub.Clients.Group(lobby.LobbyName).SendAsync("joinroom", lobby);
                Add(lobby); // Firebase
            }
            else if (lobby.Created.AddSeconds(_appConfig.LobbyHangTimeout) < DateTime.UtcNow && lobby.Members.Count > 1)
            {
                _logger.LogInformation("Lobby {0} is getting old, sending lobby to active lobby, has users {1}", lobby.LobbyName, lobby.Members.Count);
                lobby.Joinable = false; // no longer in the pool of temp rooms
                try
                {
                    _hub.Clients.Group(lobby.LobbyName).SendAsync("joinroom", lobby.ConvertToSanitizedLobby());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Add(lobby);
            }
        }
    }
}
