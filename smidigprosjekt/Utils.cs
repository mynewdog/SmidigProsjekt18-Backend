using smidigprosjekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt
{
    
    public static class Utils
    {
        public static TjommisUser ConvertToSanitizedUser(this User user)
        {
            return new TjommisUser()
            {
                Username = user.Username,
                Lastname = user.LastName
            };
        }
        public static TjommisLobby ConvertToSanitizedLobby(this Lobby lobby)
        {
            return new TjommisLobby()
            {
                Joinable = lobby.Joinable,
                LobbyName = lobby.LobbyName,
                Members = lobby.Members.Select(e => new TjommisUser() { Lastname = e.LastName, Username = e.Username }),
                Messages = lobby.Messages
            };
        }
    }
    public class TjommisUser
    {
        public string Username { get; set; }
        public string Lastname { get; set; }

    }
}
