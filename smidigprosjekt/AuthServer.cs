using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace smidigprosjekt
{
  /// <summary>
  /// Written by: Erik Alvarez
  /// Date: 18.12.2018
  /// 
  /// Source: https://github.com/aspnet-contrib/AspNet.Security.OpenIdConnect.Server
  /// If you wanna test OpenID you can do it with Curl using this example
  /// curl -iSsL --user-agent 'Mozilla/5.0' --cookie cookies --cookie-jar cookies 
  /// --data username=testusername
  /// --data password=testpassword
  /// --data grant_type=password
  /// --data client_id=tjommisdemo2018_signing_key_that_should_be_very_long
  /// https://localhost:5001/token -k
  ///
  /// </summary>
  public static class AuthServer
  {
    /// <summary>
    /// Secret key for openId Connect authorization
    /// </summary>
    public static string client_id => "tjommisdemo2018_signing_key_that_should_be_very_long";

    /// <summary>
    /// Inject openid authentication protocol for aspnet core
    /// </summary>
    /// <param name="services">the service collection to add openid connect server to</param>
    public static void AddAuthenticationServer(this IServiceCollection services)
    {
      var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(client_id));
      services.AddAuthentication().AddOpenIdConnectServer(options =>
      {
        options.AccessTokenHandler = new JwtSecurityTokenHandler();
        options.SigningCredentials.AddKey(signingKey);

        options.AllowInsecureHttp = true;
        options.TokenEndpointPath = "/token";

        options.Provider.OnValidateTokenRequest = context =>
        {
          if (string.Equals(context.ClientId, client_id, StringComparison.Ordinal))
          {
            context.Validate();
          }
          
            return Task.CompletedTask;
        };
        
        /*
         * If you wanna add extra parameters to the response
         * stream, you can do this here, this can be useful if we want
         * to add userinfo (profilesettings/etc, before websocket is initiated)
         * */
        options.Provider.OnApplyTokenResponse = response =>
        {
          /*
          if (response.Error == null) { 
            response.Response.AddParameter("", extra unencrypted parameters);
            response.Response.AddParameter("", extra unencrypted parameters);
          }*/
          return Task.CompletedTask;
        };

        /*
         * TODO
         * This is where we handle the tokenrequest,
         * missing work is connecting this to the CosmoDB server
         * and verify username and passwords
         */
        options.Provider.OnHandleTokenRequest = context =>
        {
          if (context.Request.IsPasswordGrantType())
          {
            var identity = new ClaimsIdentity(context.Scheme.Name,
            OpenIdConnectConstants.Claims.Name,
            OpenIdConnectConstants.Claims.Role);

            identity.AddClaim(OpenIdConnectConstants.Claims.Name, context.Request.Username);
            identity.AddClaim(OpenIdConnectConstants.Claims.Subject, context.Request.Username);

            identity.AddClaim(ClaimTypes.Name, context.Request.Username,
              OpenIdConnectConstants.Destinations.AccessToken,
              OpenIdConnectConstants.Destinations.IdentityToken);
            ;

            /* If we want, we can add properties to go with authenticationpacket 
             * late in process by adding props, and injecting it later in ApplyTokenResponse
             */
            //var props = new AuthenticationProperties(new Dictionary<string, string>
            //{});

            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                context.Scheme.Name);

            ticket.SetScopes(
              OpenIdConnectConstants.Scopes.Profile,
              OpenIdConnectConstants.Scopes.OfflineAccess);

            context.Validate(ticket);
          }
          return Task.CompletedTask;
        };
      });
    }
   }
}
