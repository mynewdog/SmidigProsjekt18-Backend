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
  public static class AuthServer
  {
    //Source: https://github.com/aspnet-contrib/AspNet.Security.OpenIdConnect.Server
    /// If you wanna test OpenID you can do it with Curl using this example
    /// curl -iSsL --user-agent 'Mozilla/5.0' --cookie cookies --cookie-jar cookies 
    /// --data username=testusername
    /// --data password=testpassword
    /// --data grant_type=password
    /// --data client_id=tjommisdemo2018_signing_key_that_should_be_very_long
    /// https://localhost:5001/token -k
    ///


    public static string client_id => "tjommisdemo2018_signing_key_that_should_be_very_long";
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
        options.Provider.OnApplyTokenResponse = response =>
        {
          
          if (response.Error == null) { 
            response.Response.AddParameter("StaffId", response.Ticket.Properties.Items["StaffId"].ToString());
            response.Response.AddParameter("KayakoSessionId", response.Ticket.Properties.Items["KayakoSessionId"].ToString());
          }
          return Task.CompletedTask;
        };*/
        options.Provider.OnHandleTokenRequest = context =>
        {
          /*if (loginResponse.Staffid == 0)
          {
            context.Reject( 
              error: OpenIdConnectConstants.Errors.InvalidGrant,
              description: loginResponse.Error, uri:"https://localhost:20571");
            return Task.CompletedTask;
          }*/
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

            //Create extra properties to go with authenticated, so the client knows what staffID
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
      public static MemoryStream GenerateStreamFromString(string value)
      {
        return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
      }
   }
}
