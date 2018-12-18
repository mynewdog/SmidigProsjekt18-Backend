using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Logic.Services
{
  public static class ServiceCollectionExtensions
  {
    //Register our services
    public static IServiceCollection RegisterServices(
        this IServiceCollection services)
    {
      services.AddSingleton<IUserService, UserService>();
      services.AddSingleton<IHostedService, LobbyWorker>();
      // and a lot more Services

      return services;
    }
  }
}
