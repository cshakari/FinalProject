using Code.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Services
{
    public class BookService
    {
        private readonly IMongoCollection<Book> _book;
        public BookService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _book = database.GetCollection<Book>("books");
        }
        public Book Create(Book book)
        {
            _book.InsertOne(book);
            return book;
        }

        public IList<Book> Read() =>
            _book.Find(Book => true).ToList();

        //public Book Find(string id) =>
        //    _book.Find(book => book.Id == id).SingleOrDefault();

        ////public IList<Book> FindAll(string authorId) =>
        ////    _book.Find(book => book.AuthorId == authorId).ToList();

        //public void Update(Book book) =>
        //    _book.ReplaceOne(book => book.Id == book.Id, book);

        //public void Delete(string id) =>
        //    _book.DeleteOne(book => book.Id == id);

    }
}
