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
        IEnumerable<Lobby> GetTemporaryRooms();

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
        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="lobby"></param>
        public void SendRoom(Lobby lobby)// add parameter of composistion tags
        {
            if (lobby.MaxUsers == lobby.Members.Count) // Room reach max size
            {
                _logger.LogInformation("Lobby full! :) sending joinable room to closed lobby");
                lobby.Joinable = false; // no longer in the pool of temp rooms
                _hub.Clients.Group(lobby.LobbyName).SendAsync("joinroom", lobby);
                Add(lobby); // Firebase
            }
            else if (lobby.Created.AddSeconds(10) < DateTime.UtcNow && lobby.Members.Count > 1)
            {
                _logger.LogInformation("Lobby {0} is getting old, sending lobby to active lobby, has users {1}", lobby.LobbyName, lobby.Members.Count);
                lobby.Joinable = false;
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

        public Lobby FindMatchingLobby(User user)
        {
            var rnd = new Random();
            var rndId = rnd.Next();
            var availableRooms = Lobbies.Where(e => e.Joinable).OrderByDescending(e => e.Members.Count);
            // Creates a new room 
            //fix matching algoritm
            if (availableRooms?.Count() < 1)
            {
                _logger.LogInformation("Creating new room");
                //Create a temporary room
                Lobby room = new Lobby()
                {
                    Studie = user.Studie,
                    Institutt = user.Institutt,
                    LobbyName = $"{user.Institutt.Trim()}-{user.Studie.Trim()}-{rndId}",
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

        public IEnumerable<Lobby> GetTemporaryRooms()
        {
            return Lobbies.Where(e => e.Joinable);
        }
    }
}
