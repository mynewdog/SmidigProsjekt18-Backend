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
        public static string apiKey = "AIzaSyDBfx0QnJlGiCC73eBBrwmqerQbclz0VjM";
        public static string authDomain = "tjommis-a936a.firebaseapp.com";
        public static string databaseURL = "https://tjommis-a936a.firebaseio.com";
        public static string projectId = "tjommis-a936a";
        public static string storageBucket = "tjommis-a936a.appspot.com";
        public static string messagingSenderId = "830097904055";
        public static string appId = "1:830097904055:web:27cbc5a3d1898963";
        public static string clientSecret = "VjSjNhSKRWqs7wCCWUZ3UG9KFOPwkuFssQ1uxxOT";
        public static FirebaseClient GetClient()
        {
            var firebaseClient = new FirebaseClient(databaseURL, new FirebaseOptions() { AuthTokenAsyncFactory = () => Task.FromResult(clientSecret) });
            return firebaseClient;
        }
        
        public static void initializeDB()
        {
            /*
            var client = GetClient();
            var i = new List<InterestItem>()
            {
                new InterestItem() {Id = 0,
                    Category = "Skole",
                    Name = "Eksamen"
                },
                new InterestItem() {Id = 1,
                    Category = "Sport",
                    Name = "Fotball"},
                new InterestItem() {Id = 2,
                    Category = "Kultur",
                    Name = "Kino"},
                new InterestItem() {Id = 3,
                    Category = "Jobb",
                    Name = "Jobbsøknad"},
                new InterestItem() {Id = 4,
                    Category = "Mat og Drikke",
                    Name = "Burger"},
                new InterestItem() {Id = 5,
                    Category = "Mat og Drikke",
                    Name = "Øl"
                }
            };
            i.ForEach(async e => {
                await client.Child("Interests").PostAsync(e);
             });*/
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
            return newuser;
        }
        public static async Task<IList<User>> GetUsers()
        {
            var userList = new List<User>();
            var data = await GetClient().Child("users").OnceAsync<User>();
            foreach (var user in data)
            {
                user.Object.Key = user.Key;
                if (user.Object.Lobbies == null) user.Object.Lobbies = new HashSet<Lobby>();
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
