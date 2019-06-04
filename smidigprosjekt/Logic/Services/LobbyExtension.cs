using smidigprosjekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Logic.Services
{
    public static class LobbyExtension
    {
        public static void InterestMatch(this MatchRoom PriRoom, User user)
        {
            var roominterests = PriRoom.Room.Members.GroupBy(e => e.Configuration.Interests);
            user.Configuration.Interests.ForEach(e =>
            {
                var roommatch = roominterests.Select(i => i.Key.Contains(e));
                PriRoom.Prio += roommatch.Count();
            });
        }
    }
}
