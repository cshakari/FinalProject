using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Models
{
    public class User
    {
        const int PASSWORD_MIN_LENGTH = 8;
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Please enter a valid email")]
        [Required]
        [Display(Name = "Username", Prompt = "Username(Email) *")]
        public string UserName { get; set; }

        [RegularExpression(@"^(.{0,7}|(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*#?&]*)[a-zA-Z\d@$!%*#?&]{8,})$", ErrorMessage = "Password must contain at least one number, one lowercase and one uppercase letter.")]
        [MinLength(PASSWORD_MIN_LENGTH, ErrorMessage = "Password must be at least 8 characters.")]
        [Required]
        [Display(Prompt = "Password *")]
        public string Password { get; set; }
        [Display(Name = "Role")]
        public string UserRole { get; set; }

        [BsonIgnore]
        [Required]
        [Display(Name = "Password Confirmation", Prompt = "Password Confirmation *")]
        [Compare(nameof(Password))]
        public string PasswordConfirmation { get; set; }
        [Required]
        [Display(Name = "Full Name", Prompt = "Full Name *")]
        public string Name { get; set; }
        [Display(Name = "Email Activated")]
        public bool IsActive { get; set; }        

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Date Activated")]
        public DateTime ActivatedAt { get; set; }
        [Display(Name = "Date Updated")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        [Display(Name = "Profile Visibility")]
        public bool Visibility { get; set; }
        public Author Author { get; set; }
    }
}
