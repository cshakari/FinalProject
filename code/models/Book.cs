using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Models
{
    public class Book
    {
        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public int? Id { get; set; }

        //public string AuthorId { get; set; }

        //[Required]
        [Display(Name = "Title of Book")]
        public string TitleOfBook { get; set; }

        //[Required]
        public decimal Price { get; set; }

        //[DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd}")]
        //[Required]
        [Display(Name = "Published Date", Prompt = "YYYY-MM-DD")]
        public DateTime? PublishedDate { get; set; }

        //[Required]
        public string Publisher { get; set; }

        //[Required]
        [Display(Prompt = "#############")]
        //[ValidateISBN("Please enter a valid ISBN.")]
        [RegularExpression(@"^(?:ISBN(?:-1[03])?:?●)?(?=[0-9X]{10}$|(?=(?:[0-9]+[-●]){3})[-●0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[-●]){4})[-●0-9]{17}$)(?:97[89][-●]?)?[0-9]{1,5}[-●]?[0-9]+[-●]?[0-9]+[-●]?[0-9X]$", ErrorMessage = "Please enter a valid ISBN")]
        public string ISBN { get; set; }

        //[Required]
        [Display(Name = "Book Cover")]
        public string BookCoverUrl { get; set; }

        [Display(Name = "Book Description", Prompt = "Write a little bit about your book")]
        public string BookDescription { get; set; }

        //[Required]
        [Display(Prompt = "EX. French")]
        public string Language { get; set; }

        //[Required]
        [Display(Name = "Age Group", Prompt = "EX. Kids 5 - 10")]
        public string AgeGroup { get; set; }

        [Display(Name = "Book's Shopify link")]
        public string shopifyUrl { get; set; }
    }
}
