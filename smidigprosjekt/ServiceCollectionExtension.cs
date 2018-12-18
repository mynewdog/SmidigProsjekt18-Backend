using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Logic.Services
{

  /// <summary>
  /// RegisterServices() register services.
  /// Persistent services and background tasks
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    //Register our services
    public static IServiceCollection RegisterServices(
        this IServiceCollection services)
    {
      services.AddSingleton<IUserService, UserService>();
      services.AddSingleton<ILobbyService, LobbyService>();
      services.AddSingleton<IHostedService, LobbyWorker>();

      return services;
    }
  }
}
