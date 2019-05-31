using DotNetify;
using DotNetify.Security;
using smidigprosjekt.Logic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using smidigprosjekt.Models;

namespace smidigprosjekt.Hubs
{
    //[Authorize(Role="Admin")]
    [Authorize]
    public class FrontEndManagementVM : MulticastVM
    {
        private IUserService _userService;
        private ILobbyService _lobbyService;
        private Timer _timer;
        public string Greetings => "Welcome to tjommis management";
        public DateTime ServerTime => DateTime.Now;
        public int TotalUsers => _userService.Count();
        public int TotalLobbies => _lobbyService.All().Where(e=>e.Joinable == false).Count();
        public int JoinableLobbies => _lobbyService.All().Where(e=>e.Joinable == true).Count();
        public IEnumerable<TjommisLobby> Lobbies => _lobbyService.All().Select(e=>e.ConvertToSanitizedLobby());
        public IEnumerable<TjommisUser> Users => _userService.All().Where(e=>!e.HangoutSearch).Select(e=>e.ConvertToSanitizedUser());
        public IEnumerable<TjommisUser> UsersInHangout => _userService.GetHangoutUsers().Select(e=>e.user.ConvertToSanitizedUser());





        public FrontEndManagementVM(IUserService userService, ILobbyService lobbyService)
        {
            _userService = userService;
            _lobbyService = lobbyService;
            _timer = new Timer(state =>
            {

                //Notify dotnetify to send these values
                Changed(nameof(ServerTime));
                Changed(nameof(TotalUsers));
                Changed(nameof(TotalLobbies));
                Changed(nameof(Lobbies));
                Changed(nameof(Users));
                Changed(nameof(UsersInHangout));
                
                PushUpdates();
            }, null, 0, 1000);
        }

        public override void Dispose()
        {
            _timer.Dispose();
            base.Dispose();
        }
    }
}

