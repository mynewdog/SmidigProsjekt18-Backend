using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Models
{
    public enum LobbyStatus { Open, Closed, Temporary }
    public class Lobby
    {
        public DateTime Created { get; set; }
        public bool Joinable { get; set; } // to add/remove it-> pool of lobbies
        [JsonConverter(typeof(StringEnumConverter))]
        public LobbyStatus Status { get; set; }
        public int Id { get; set; }
        public string LobbyName { get; set; }
        public string Institutt { get; set; } // skole
        public string Studie { get; set; } // fag
        //public string Interesser { get; set; } // ikke brukt atm.
        public HashSet<User> Members { get; set; }
        public List<Message> Messages { get; set; }
        public int MaxUsers { get; set; } // will change depening on 1-1 or (+) button
        public string Key { get; internal set; }
    }
    public class TjommisLobby
    {
        public IEnumerable<TjommisUser> Members { get; set; }
        public IList<Message> Messages { get; set; }
        public bool Joinable { get; set; }
        public string LobbyName { get; set; }
    }
}