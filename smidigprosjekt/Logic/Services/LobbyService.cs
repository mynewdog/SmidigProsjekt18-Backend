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
        void Merge(Lobby lobby1, Lobby lobby2);
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
            return Lobbies.Where(e => e.Joinable || e.Temporary);
        }

        public Lobby FindMatchingLobby(User user)
        {
            List<MatchRoom> matchList;
            // For group chat rooms.
            if (user.HangoutSearch)
            {
                matchList = Lobbies.Where(e => e.Joinable).Select(e => new MatchRoom() { Room = e, Prio = 0 }).ToList();
            }
            // For single 1 to 1 rooms.
            else
            {
                matchList = Lobbies.Where(e => e.Joinable && e.MaxUsers == 2).Select(e => new MatchRoom() { Room = e, Prio = 0 }).ToList();
            }
            foreach (var lob in matchList)
            {
                if (lob.Room.Institutt.Contains(user.Institutt, StringComparison.OrdinalIgnoreCase))
                {
                    lob.Prio += 10; // Need to test scale
                }
                if (lob.Room.Studie.Contains(user.Studie, StringComparison.OrdinalIgnoreCase))
                {
                    lob.Prio += 3; // Need to test scale
                }
                //Match on interests list
                lob.InterestMatch(user);
            }
            var bestRoom = matchList.OrderByDescending(e => e.Prio);
            // Did not match on any room, creates new room based on the user
            if (bestRoom.FirstOrDefault() == null || (bestRoom.FirstOrDefault() != null && bestRoom.First().Prio == 0))
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
       
            Lobby Newroom = NewRoom(user.Studie, 
                user.Institutt, $"{user.Institutt.Trim()}-{user.Studie.Trim()}",
                user.SingleHangoutSearch ? _appConfig.MaximumPerLobby : 2
                ); 
               
            Lobbies.Add(Newroom);
            return Newroom;
        }
        private Lobby NewRoom(string studie, string institutt, string lobbyname, int maxUsers)
        {
            var rnd = new Random();
            var rndId = rnd.Next();

            return new Lobby()
            {
                Studie = studie,
                Institutt = institutt,
                LobbyName = $"{lobbyname}-{rndId}",
                Created = DateTime.UtcNow,
                Joinable = true,
                Id = rndId,
                Members = new HashSet<User>(),
                Messages = new List<Message>(),
                MaxUsers = maxUsers,
                Status = LobbyStatus.Temporary,
                Temporary = true
            };
        }
        /// <summary>
        /// Merge with creating a hashset with uniqe members from both lobbies
        /// populate a new lobby based on the old ones.
        /// adds members to it and send it to client
        /// </summary>
        /// <param name="lobby1"></param>
        /// <param name="lobby2"></param>
        public void Merge(Lobby lobby1, Lobby lobby2)
        {
            HashSet<User> members = new HashSet<User>();
            foreach (var member in lobby1.Members) members.Add(member);
            foreach (var member in lobby2.Members) members.Add(member);
            Lobbies.Remove(lobby1);
            Lobbies.Remove(lobby2);
            var prioStudie= members.GroupBy(e => e.Studie).OrderByDescending(e=>e.Count());
            var prioInstitutt = members.GroupBy(e => e.Institutt).OrderByDescending(e => e.Count());

            string studie = prioStudie.First().Key;
            string institutt = prioStudie.First().Key;
            string lobbyname = $"{institutt.Trim()}-{studie.Trim()}";
            int maxusers = members.Count() > _appConfig.MaximumPerLobby ? members.Count() : _appConfig.MaximumPerLobby;
            var lobby = NewRoom(studie,institutt,lobbyname,maxusers);
            if (lobby.Members.Count >= _appConfig.MaximumPerLobby) lobby.Joinable = false;

            lobby.Members = members;
            // Update user information
            foreach (var member in members)
            {
                member.Lobbies.Add(lobby);
                _hub.Groups.AddToGroupAsync(member.ConnectionId, lobby.LobbyName);
            }
            


            _hub.Clients.Group(lobby.LobbyName).SendAsync("lobbyinfo", lobby.ConvertToSanitizedLobby());

            Lobbies.Add(lobby);
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
                lobby.Temporary = false;
                try
                {
                    _hub.Clients.Group(lobby.LobbyName).SendAsync("joinroom", lobby.ConvertToSanitizedLobby());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Add(lobby); // Firebase
            }
            //If room to old
            else if (lobby.Created.AddSeconds(_appConfig.LobbyHangTimeout) < DateTime.UtcNow && lobby.Members.Count > 1)
            {
                _logger.LogInformation("Lobby {0} is getting old, sending lobby to active lobby, has users {1}", lobby.LobbyName, lobby.Members.Count);
                lobby.Joinable = false; // no longer in the pool of temp rooms
                lobby.Temporary = false;
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
