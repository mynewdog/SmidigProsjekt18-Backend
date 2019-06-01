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
        private IInterestProviderService _interestService;
        private IUserService _userService;
        private ILobbyService _lobbyService;
        private Timer _timer;
        public string Greetings => "Welcome to tjommis management";
        public DateTime ServerTime => DateTime.Now;
        public IList<InterestItem> Interests => _interestService.GetAll();

        public ManageInterestVM(IUserService userService,
            ILobbyService lobbyService, IInterestProviderService interestProviderService)
        {
            _interestService = interestProviderService;
            _userService = userService;
            _lobbyService = lobbyService;
            _timer = new Timer(state =>
            {
                Changed(nameof(ServerTime));
                PushUpdates();
            }, null, 0, 1000);
        }
        public Action<InterestItem> Add => async interest =>
        {
            var result = await _interestService.Add(interest.Category,interest.Name);
            Console.WriteLine("Added interest {0}, key: {1}", interest.Name,result.Key);
            Changed(nameof(Interests));
            PushUpdates();
        };
        public Action<string> Delete => async index =>
        {
            Console.WriteLine("Deleting interest {0}", index);
            await _interestService.Remove(_interestService.GetAll().First(e => e.Key.Contains(index)));
            Changed(nameof(Interests));
            PushUpdates();
        };


        public override void Dispose()
        {
            _timer.Dispose();
            base.Dispose();
        }
    }
}

