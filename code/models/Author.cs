using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Models
{
    public class Author
    {
        public Author()
        {
            this.BookList = new List<Book>();
        }

        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        //public string Id { get; set; }

        //public string UserName { get; set; }

        [Required]
        //[RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Please enter a valid name")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        //[RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Please enter a valid name")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string ProfileUrl { get; set; }
        public string UrlIdentity { get; set; }

        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Please enter a valid email")]
        [Display(Name = "Email Address", Prompt = "sample@email.com *")]
        public string Email { get; set; } //consider making this field disabled so it cant be changed

        [Required]
        //[DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "Date Of Brith", Prompt = "YYYY-MM-DD")]
        public DateTime? DateOfBirth { get; set; }

        //[Required]
        [Display(Name = "Image of Yourself")]
        public string AuthorImageUrl { get; set; }

        [Display(Name = "Your Website", Prompt = "https://www.example.com")]
        [Url]
        public string WebsiteUrl { get; set; }

        [Required]
        //[RegularExpression(@"^(?!\s*$).+", ErrorMessage = "This field must not be blank")]
        [Display(Name = "Location", Prompt = "Ex. Toronto, Ontario *")]
        public string Location { get; set; }

        [Display(Name = "Pronouns")]
        public string Pronouns { get; set; }

        [Display(Name = "Biography", Prompt = "Write a little bit about yourself")] //Add number of words counter under this input field
        public string Biography { get; set; }


        // Social media accounts

        [Display(Name = "Facebook")]
        [Url]
        public string FacebookUrl { get; set; }

        [Display(Name = "Instagram")]
        [Url]
        public string InstagramUrl { get; set; }

        [Display(Name = "Twitter")]
        [Url]
        public string TwitterUrl { get; set; }

        [Display(Name = "Pinterest")]
        [Url]
        public string PinterestUrl { get; set; }

        //public IEnumerable<Book> BookList { get; set; }
        //public IEnumerable<VideoLink> VideoLinkList { get; set; }
        //public IEnumerable<UploadedFile> FilePathList { get; set; }
        public List<Book> BookList { get; set; }
        public List<VideoLink> VideoLinkList { get; set; }
        public List<UploadedFile> FilePathList { get; set; }
        //List<IFormFile> FileList { get; set; }
    }
}
