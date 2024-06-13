using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Barcos.Services
{
    public class User
    {
        public ObjectId Id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(DatabaseService databaseService)
        {
            _users = databaseService.GetCollection<User>("users");
        }

        public bool AuthenticateUser(string username, string password)
        {
            var user = _users.Find(u => u.username == username && u.password == password).FirstOrDefault();
            return user != null;
        }
    }
}
