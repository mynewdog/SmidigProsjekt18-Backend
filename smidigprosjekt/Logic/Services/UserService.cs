using Microsoft.AspNetCore.SignalR;
using smidigprosjekt.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Logic.Services
{
  public interface IUserService
  {
    IEnumerable<User> All();
    IEnumerable<IClientProxy> GetConnectedConnections();
    int Count();
    void Add(User newUser, IClientProxy Caller);
    void Disconnect(string connectionId);
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
      truncateList();
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
  }

}
