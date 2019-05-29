using DotNetify;
using DotNetify.Security;
using smidigprosjekt.Logic.Services;
using smidigprosjekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace smidigprosjekt.Hubs
{
    //[Authorize(Role="Admin")]
    [Authorize]
    public class ManageInterestVM : MulticastVM
    {
        private IUserService _userService;
        private ILobbyService _lobbyService;
        private Timer _timer;
        public string Greetings => "Welcome to tjommis management";
        public DateTime ServerTime => DateTime.Now;
        public IList<InterestItem> Interests => _userService.Interests;


        public ManageInterestVM(IUserService userService, ILobbyService lobbyService)
        {
            _userService = userService;
            _lobbyService = lobbyService;
            _timer = new Timer(state =>
            {
                Changed(nameof(ServerTime));
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

