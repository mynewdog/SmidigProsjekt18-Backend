﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace smidigprosjekt.Models
{
    public class User
    {
        public int Id { get; set; }
        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public UserConfiguration Configuration { get; set; }
        public bool HangoutSearch { get; set; }
        public bool SingleHangoutSearch { get; set; }
        public HashSet<Lobby> Lobbies { get; set; }
        //The client connected state
        public bool Connected { get; set; }
        
        public string upwd_salt { get; set; }
        public string encrypted_pwd { get; set; }
        public string Institutt { get;  set; }
        public string Studie { get;  set; }
        public string LastName { get;  set; }
        public string Email { get;  set; }
        public string Key { get;  set; }

        public bool IsPassword(string password)
        {
            int DEFAULT_SALT_SIZE = 5;
            if (string.IsNullOrEmpty(upwd_salt)) upwd_salt = CreateSalt(DEFAULT_SALT_SIZE);
            if (EncryptPassword(password,upwd_salt).Contains(encrypted_pwd))
            {
                return true;
            }
            return false;
        }
        public void storePassword(string password)
        {
            var salt = CreateSalt(5);
            this.upwd_salt = salt;
            this.encrypted_pwd = EncryptPassword(password,salt);
        }
        public static string EncryptPassword(string password, string salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();
            var plainTextWithSaltBytes = password + salt;
            var computedHash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(plainTextWithSaltBytes));
            return Convert.ToBase64String(computedHash);//Encoding.ASCII.GetString(computedHash, 0, computedHash.Length);
        }
        public static string CreateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }
        public override string ToString()
        {
            return $"${this.Username} : {this.LastName}";
        }
    }
    public class UserConfiguration
    {
        public List<string> Interests { get; set; }
    }
}
