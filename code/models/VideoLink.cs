using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Models
{
    public class VideoLink
    {
        //public int Id { get; set; }
        [Url]
        [Display(Prompt = "https://www.example.com/videoName")]
        public string Link { get; set; }

        [Display(Prompt = "Short Description")]
        public string Description { get; set; }
    }
}
