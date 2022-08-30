using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Models
{
    public class UploadedFile
    {
        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public int Id { get; set; }

        public string Path { get; set; }

        [Display(Prompt = "Short Description")]
        public string Description { get; set; }

    }
}
