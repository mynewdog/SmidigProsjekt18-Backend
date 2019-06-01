using DotNetify;
using DotNetify.Security;
using smidigprosjekt.Logic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace smidigprosjekt.Hubs
{
    //[Authorize(Role="Admin")]
    [Authorize]
    public class FrontEndManagementVM : BaseVM
    {
        private IUserService _userService;
        private ILobbyService _lobbyService;
        private Timer _timer;
        public string Greetings => "Welcome to tjommis management";
        public DateTime ServerTime => DateTime.Now;
        public int TotalUsers => _userService.Count();
        public int TotalLobbies => _lobbyService.Count();


        public FrontEndManagementVM(IUserService userService, ILobbyService lobbyService)
        {
            _userService = userService;
            _lobbyService = lobbyService;
            _timer = new Timer(state =>
            {
                Changed(nameof(ServerTime));
                Changed(nameof(TotalUsers));
                Changed(nameof(TotalLobbies));
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

