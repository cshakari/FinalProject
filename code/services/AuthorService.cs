using Code.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Services
{
    public class AuthorService
    {

        private readonly IMongoCollection<Author> _authors;
        public AuthorService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _authors = database.GetCollection<Author>("authors");
        }
        public Author Create(Author author)
        {
            _authors.InsertOne(author);
            return author;
        }

        public IList<Author> Read() =>
            _authors.Find(author => true).ToList();

        public Author Find(string email) =>
            _authors.Find(author => author.Email == email).SingleOrDefault();
        //public Author FindByUsername(string userName
        //    ) =>
        //    _authors.Find(author => author.UserName == userName).SingleOrDefault();

        //public void Update(Author author) =>
        //    _authors.ReplaceOne(author => author.Id == author.Id, author);

        public void Delete(string email) =>
            _authors.DeleteOne(author => author.Email == email);

    }
}
