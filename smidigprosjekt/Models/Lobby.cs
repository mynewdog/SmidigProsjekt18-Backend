using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Models
{
    public class Lobby
    {
        public bool Joinable { get; set; } // to add/remove it-> pool of lobbies
        public int Id { get; set; }
        public string LobbyName { get; set; }
        public string Institutt { get; set; } // skole
        public string Studie { get; set; } // fag
        //public string Interesser { get; set; } // ikke brukt atm.
        public ConcurrentDictionary<int, User> Members { get; set; }
        public List<Message> Messages { get; set; }
        public int MaxUsers { get; set; } // will change depening on 1-1 or (+) button
    }
}