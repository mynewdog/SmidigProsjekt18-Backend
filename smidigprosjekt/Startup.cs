using System;
using System.Linq;
using System.Text;
using smidigprosjekt.Hubs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using smidigprosjekt.Logic.Services;
using smidigprosjekt.Models;
using smidigprosjekt.Logic.Database;
using Microsoft.Extensions.Logging;
using DotNetify;
using Microsoft.AspNetCore.SpaServices.Webpack;

namespace smidigprosjekt
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Add OpenID authentication server
            //Injected from .\AuthServer.cs
            services.AddAuthenticationServer();

            // Add functionality to inject IOptions<T>
            services.AddOptions();
            // Add our Config object so it can be injected
            services.Configure<AppConfiguration>(Configuration.GetSection("Configuration"));

            //Add Authentication scheme and JwT bearer
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(config =>
            {
          //Add token valdiation parameters(use AuthServer.client_id) for verification
          config.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AuthServer.Client_id)),
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(0)
                };

          // Move access token so signalR can validate:
          // We have to hook the OnMessageReceived event in order to
          // allow the JWT authentication handler to read the access
          // token from the query string when a WebSocket or 
          // Server-Sent Events request comes in.
          config.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
              {
                    var accessToken = context.Request.Query["access_token"];
              // If the request is for our hub...
              var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                  (path.StartsWithSegments("/tjommisHub")))
                    {
                  // Read the token out of the query string
                  context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
                };
            });

            //Add cross origin policy so Ionic can connect
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                                                                         .AllowAnyMethod()
                                                                          .AllowAnyHeader()
                                                                          .AllowCredentials()));


            //Add signalR service
            services.AddSignalR();
            services.AddDotNetify();
            //Add model view controller for static pages
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.RegisterServices();
            services.AddLogging();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //Use CORS policy
            app.UseCors("AllowAll");

            //Start authentication
            app.UseAuthentication();

            //Use websockets
            app.UseWebSockets();


            //Development trigger
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //Use default files
            app.UseDefaultFiles();
            //Use static files in wwwroot
            app.UseStaticFiles();



            //do not use app.UseHttpsRedirection() -> We need to authorize using proxy during development
            //Use HTTPS redirection when going live

            //Use signalR Hub and map hub classes
            //from TjommisHub.cs
            app.UseSignalR(routes =>
            {
                routes.MapHub<TjommisHub>("/tjommisHub");
                routes.MapDotNetifyHub();
            });

            app.UseDotNetify();

            //Use MVC
            app.UseMvc();


            // Optional: utilize webpack hot reload feature.
            app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
            {
                HotModuleReplacement = true,
                HotModuleReplacementClientOptions = new Dictionary<string, string> { { "reload", "true" } },
            });


            //FirebaseDbConnection.initializeDB();
        }
    }
}
