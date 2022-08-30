using Code.Models;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Services
{
    public class LoginService
    {
        private readonly IMongoCollection<User> _logins;
        public LoginService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _logins = database.GetCollection<User>("users");
        }
        public LoginService() { }
        public User Create(User login)
        {
            _logins.InsertOne(login);
            return login;
        }

        public IList<User> Read() =>
            _logins.Find(log => true).ToList();

        public User Find(string userName) =>
            //_logins.Find(log => log.Id == id).SingleOrDefault();
            _logins.Find(log => log.UserName.ToUpper() == userName.ToUpper()).SingleOrDefault();

        public User FindById(string id) =>
            _logins.Find(log => log.Id == id).SingleOrDefault();

        public void Update(User user) =>
            _logins.ReplaceOne(log => log.Id == user.Id, user);

        public void Delete(string id) =>
            _logins.DeleteOne(log => log.Id == id);

        public IList<User> ReadByName(string name) =>
            _logins.Find(log => log.Name.ToLower().Contains(name.ToLower())).ToList();
    }
}
