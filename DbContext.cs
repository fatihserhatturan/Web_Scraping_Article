using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using scrapingWeb.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scrapingWeb
{
    public class ScrapDbContext  : DbContext
    {

        public DbSet<Article> Articles { get; set; }
        public ScrapDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) 
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Article>(b =>
            {
                b.ToCollection("articles");
                b.Property(c => c.Title).HasElementName("Title");
            });
        }
    }
}
