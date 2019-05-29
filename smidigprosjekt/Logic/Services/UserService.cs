using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
        IList<InterestItem> Interests { get; }
        Task<InterestItem> AddInterest(string Category, string Tag);
        Task RemoveInterest(InterestItem item);
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
        void ConnectUser(User newUser, IClientProxy Caller);

        /// <summary>
        /// Gets the total number of users in hangout
        /// </summary>
        /// <returns></returns>
        int GetHangoutUserCount();
        IEnumerable<UserSession> GetHangoutUsers();
        User GetUserConfiguration(string userName);

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
        Task<bool> UpdateUser(User user);
    }

    public class UserSession
    {
        public User user;
        public IClientProxy proxy;
    }

    public class UserService : IUserService
    {
        public ConcurrentDictionary<User, IClientProxy> _activeUsers;
        public IList<User> _userAccessList;
        public IList<InterestItem> Interests { get; set; }
        public ILogger logger { get; }

        public UserService(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<UserService>();
            logger.Log(LogLevel.Information,"User Service Started.");
            _activeUsers = new ConcurrentDictionary<User, IClientProxy>();
            UpdateUserAccessList();
            UpdateInterestList();
        }

        private void UpdateInterestList()
        {
            logger.Log(LogLevel.Information, "Downloading Interest List...");
            var _interestList = FirebaseDbConnection.GetInterests();
            Interests = _interestList.Result;
        }

        public void UpdateUserAccessList()
        {
            logger.Log(LogLevel.Information, "Downloading User Access List...");
            var _userAccessListm = FirebaseDbConnection.GetUsers();
            _userAccessListm.Wait();
            _userAccessList = _userAccessListm.Result;
        }

        public async Task<bool> RegisterUser(User user)
        {
            logger.Log(LogLevel.Information, "Registering user: " + user.ToString());
            if (_userAccessList.FirstOrDefault(e => e.Username.Contains(user.Username, StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                var result = await FirebaseDbConnection.CreateUser(user);
                result.Object.Key = result.Key;
                _userAccessList.Add(result.Object);
                return true;
            }
            return false;
        }
        public void ConnectUser(User newUser, IClientProxy Caller)
        {
            if (_activeUsers.TryAdd(newUser, Caller))
            {
                
                Console.WriteLine("User connected: " + newUser.ToString());
            }
            else
            {
                Console.WriteLine("User allready logged in...");
            }
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

        public User GetUserConfiguration(string userName)
        {
            return _userAccessList.Where(e => e.Username.Contains(userName,StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }

        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                await FirebaseDbConnection.UpdateUser(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<InterestItem> AddInterest(string Category, string Tag)
        {
            return await FirebaseDbConnection.AddInterest(new InterestItem() {Id = Interests.Count(), Category = Category, Name = Tag });
        }

        public async Task RemoveInterest(InterestItem item)
        {
            await FirebaseDbConnection.RemoveInterest(item);
            Interests.Remove(item);
        }
        
    }

}
