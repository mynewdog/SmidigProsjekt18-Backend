using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
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
        
        public static async Task initializeDB()
        {
            var salt = User.CreateSalt(5);
            await CreateUser(new User()
            {
                Configuration = new UserConfiguration()
                {
                    Interests = new List<string>()
                    {
                        "Football","IT"
                    }
                },
                Username = "test",
                upwd_salt = salt,
                encrypted_pwd = User.EncryptPassword("test", salt)
            });
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
                Console.WriteLine("Key:" + user.Key);
                Console.WriteLine("Found user:" + user.Object.Username);
                userList.Add(user.Object);
            }
            return userList;
        }
    }
}
