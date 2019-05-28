using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DotNetify;
using DotNetify.Routing;
using DotNetify.Security;

namespace marble
{
   [Authorize]
   public class AppLayout : BaseVM, IRoutable
   {
      private enum Route
      {
        Home,
         StartPage,
            InterestPage,
        };

      public static string FormPagePath => "Form";

      public RoutingState RoutingState { get; set; }

      public object Menus => new List<object>()
      {
         new { Title = "Statistics",    Icon = "assessment", Route = this.GetRoute(nameof(Route.StartPage)) },
         new { Title = "Interests",    Icon = "assessment", Route = this.GetRoute(nameof(Route.InterestPage)) },
      };

      public string UserName { get; set; }
      public string UserAvatar { get; set; }

      public AppLayout(IPrincipalAccessor principalAccessor)
      {
         var userIdentity = principalAccessor.Principal.Identity as ClaimsIdentity;

         UserName = userIdentity.Name;
         UserAvatar = userIdentity.Claims.FirstOrDefault(i => i.Type == ClaimTypes.Uri)?.Value;

         this.RegisterRoutes("/", new List<RouteTemplate>
            {
                new RouteTemplate(nameof(Route.Home)) { UrlPattern = "", ViewUrl = nameof(Route.StartPage) },
                new RouteTemplate(nameof(Route.StartPage)),
                new RouteTemplate(nameof(Route.InterestPage)),
            });
      }
   }
}
