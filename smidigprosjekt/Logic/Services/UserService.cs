using Microsoft.AspNetCore.SignalR;
using smidigprosjekt.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace smidigprosjekt.Logic.Services
{

  /// <summary>
  /// Author: Erik Alvarez
  /// Created Date: 18.12.2018
  /// Persistent service that keeps track of webserver connectivity
  /// </summary>
  public interface IUserService
  {
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>All users in list</returns>
    IEnumerable<User> All();

    /// <summary>
    /// Get all connected connections
    /// </summary>
    /// <returns>Client websocket connections for online users</returns>
    IEnumerable<IClientProxy> GetConnectedConnections();
    /// <summary>
    /// Total count in list
    /// </summary>
    /// <returns></returns>
    int Count();
    /// <summary>
    /// Adds a new user to the userlist
    /// </summary>
    /// <param name="newUser">An object containing the user attributes</param>
    /// <param name="Caller">An object containing the connection to client</param>
    void Add(User newUser, IClientProxy Caller);

    /// <summary>
    /// Gets the total number of users in hangout
    /// </summary>
    /// <returns></returns>
    int GetHangoutUserCount();

    /// <summary>
    /// Disconnects a user from the userlist
    /// Doesn't delete, because a user might reconnect
    /// </summary>
    /// <param name="connectionId"></param>
    void Disconnect(string connectionId);

    /// <summary>
    /// Returns User object if connectionId exists
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns>User object</returns>
    User GetUserFromConnectionId(string connectionId);
  }


  public class UserService : IUserService
  {
    public ConcurrentDictionary<User, IClientProxy> _users;
    public UserService()
    {
      _users = new ConcurrentDictionary<User, IClientProxy>();
    }

    public void Add(User newUser, IClientProxy Caller)
    {
      _users.TryAdd(newUser,Caller);
      truncateList(); //clean up list so we don't run out of memory
    }
    public void truncateList()
    {
      _users.Where(e => e.Key.Connected == false).ToList().ForEach(e =>
        {
          _users.TryRemove(e.Key, out var value);
        });
    }
    public IEnumerable<User> All()
    {
      return _users.Keys;
    }

    public int Count()
    {
      return _users.Where(e=>e.Key.Connected).Count();
    }

    public void Disconnect(string connectionId)
    {
      var user = _users.Keys.FirstOrDefault(e => e.ConnectionId.Equals(connectionId));
      user.Connected = false;
    }

    public IEnumerable<IClientProxy> GetConnectedConnections()
    {
      return _users.Where(e => e.Key.Connected).Select(e => e.Value);
    }

    public User GetUserFromConnectionId(string connectionId)
    {
      var user = _users.Keys.FirstOrDefault(e => e.ConnectionId.Equals(connectionId));
      return user;
    }

    public int GetHangoutUserCount()
    {
      return _users.Where(e => e.Key.Connected && e.Key.HangoutSearch == true).Count();
    }
  }

}
