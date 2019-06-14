using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using smidigprosjekt.Models;

namespace smidigprosjekt.Logic.Database
{
    public class FirebaseDbConnection
    {
        public static string databaseURL = "https://tjommis-a936a.firebaseio.com";
        public static string clientSecret = "<replace with client secret>";
        public static FirebaseClient GetClient()
        {
            var firebaseClient = new FirebaseClient(databaseURL, new FirebaseOptions() { AuthTokenAsyncFactory = () => Task.FromResult(clientSecret) });
            return firebaseClient;
        }
        
        public static void initializeDB()
        {
            //used for first time developer init
        }
        public static async Task<InterestItem> AddInterest(InterestItem e)
        {
            var client = GetClient();
            var ret = await client.Child("Interests").PostAsync(e);
            ret.Object.Key = ret.Key;
            return ret.Object;
        }
        public static async Task RemoveInterest(InterestItem e)
        {
            var client = GetClient();
            await client.Child("Interests").Child(e.Key).DeleteAsync();
        }

        private static async Task updateOrCreateUser(User user)
        {
            var client = GetClient();
            await client.Child("users").PutAsync(user);
        }

        public static async Task<FirebaseObject<User>> CreateUser(User user)
        {
            var client = GetClient();
            var newuser = await client.Child("users").PostAsync(user);
            user.Key = newuser.Key;
            return newuser;
        }
        public static async Task UpdateUser(User user)
        {
            var client = GetClient();
            await client.Child("users").Child(user.Key).PutAsync(user);
            return;
        }
        public static async Task<IList<User>> GetUsers()
        {
            var userList = new List<User>();
            var data = await GetClient().Child("users").OnceAsync<User>();
            foreach (var user in data)
            {
                user.Object.Key = user.Key;
                if (user.Object.Lobbies == null) user.Object.Lobbies = new HashSet<Lobby>();
                if (user.Object.Configuration == null) {
                    user.Object.Configuration = new UserConfiguration();
                    user.Object.Configuration.Interests = new List<string>();
                }
                userList.Add(user.Object);
            }
            return userList;
        }

        public static async Task<List<InterestItem>> GetInterests()
        {
            var client = GetClient();
            var list = new List<InterestItem>();
            var data = await client.Child("Interests").OnceAsync<InterestItem>();
            foreach (var item in data)
            {
                item.Object.Key = item.Key;
                list.Add(item.Object);
            }
            return list;
        }

        public static async Task addRoom(Lobby room)
        {
            var client = GetClient();
            var ret = await client.Child("ActiveRooms").PostAsync(room);
            room.Key = ret.Key;
        }
        public static async Task removeRoom(Lobby r)
        {
            var client = GetClient();
            await client.Child("ActiveRooms").Child(r.Key).DeleteAsync();
        }

        public static async Task<IList<Lobby>> loadRooms()
        {
            var client = GetClient();

            var roomList = new List<Lobby>();
            var data = await client.Child("ActiveRooms").OnceAsync<Lobby>();
            foreach (var room in data)
            {
                room.Object.Key = room.Key;
                roomList.Add(room.Object);
            }
            return roomList;
        }
    }
}
