using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Code.Models;

namespace Code.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Code.Models.User> User { get; set; }
        public DbSet<Code.Models.Author> Author { get; set; }
        public DbSet<Code.Models.Book> Book { get; set; }
    }
}
