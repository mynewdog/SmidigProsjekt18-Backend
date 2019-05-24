using Microsoft.AspNetCore.SignalR;
using smidigprosjekt.Logic.Database;
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
        IEnumerable<UserSession> GetHangoutUsers();

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
        void UpdateUserAccessList();
        bool Validate(string username, string password);
        Task<bool> RegisterUser(User user);
    }

    public class UserSession
    {
        public User user;
        public IClientProxy proxy;
    }

    public class UserService : IUserService
    {
        public ConcurrentDictionary<User, IClientProxy> _activeUsers;
        public IEnumerable<User> _userAccessList;

        public UserService()
        {
            _activeUsers = new ConcurrentDictionary<User, IClientProxy>();
            UpdateUserAccessList();
        }

        public void UpdateUserAccessList()
        {
            var _userAccessListm = FirebaseDbConnection.GetUsers();
            _userAccessListm.Wait();
            _userAccessList = _userAccessListm.Result;
        }
        public async Task<bool> RegisterUser(User user)
        {

            if (_userAccessList.FirstOrDefault(e => e.Username.Contains(user.Username, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                var result = await FirebaseDbConnection.CreateUser(user);
                //result.Wait();
                _userAccessList.Append(result.Object);
                return true;
            }
            return false;
        }
        public void Add(User newUser, IClientProxy Caller)
        {
            _activeUsers.TryAdd(newUser, Caller);
            truncateList(); //clean up list so we don't run out of memory
        }
        public void truncateList()
        {
            _activeUsers.Where(e => e.Key.Connected == false).ToList().ForEach(e =>
              {
                  _activeUsers.TryRemove(e.Key, out var value);
              });
        }
        public IEnumerable<User> All()
        {
            return _activeUsers.Keys;
        }

        public int Count()
        {
            return _activeUsers.Where(e => e.Key.Connected).Count();
        }

        public void Disconnect(string connectionId)
        {
            var user = _activeUsers.Keys.FirstOrDefault(e => e.ConnectionId.Equals(connectionId));
            user.Connected = false;
        }

        public IEnumerable<IClientProxy> GetConnectedConnections()
        {
            return _activeUsers.Where(e => e.Key.Connected).Select(e => e.Value);
        }

        public User GetUserFromConnectionId(string connectionId)
        {
            var user = _activeUsers.Keys.FirstOrDefault(e => e.ConnectionId.Equals(connectionId));
            return user;
        }

        public int GetHangoutUserCount()
        {
            return _activeUsers.Where(e => e.Key.Connected && e.Key.HangoutSearch == true).Count();
        }

        public bool Validate(string username, string password)
        {
            if (_userAccessList?.SingleOrDefault(e => e.Username.Contains(username,StringComparison.InvariantCultureIgnoreCase) && e.IsPassword(password)) != null) return true;
            else return false;
        }

        public IEnumerable<UserSession> GetHangoutUsers()
        {
            return _activeUsers.Where(e => e.Key.HangoutSearch).Select(e => new UserSession() { proxy = e.Value, user = e.Key });
        }
    }

}
