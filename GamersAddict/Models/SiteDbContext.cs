using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace GamersAddict.Models
{
    public class SiteDbContext : DbContext
    {
        public SiteDbContext() : base("DefaultConnection")
        {   }

        //Tables
        public DbSet<Conf> Conf { get; set; }
        public DbSet<AspNetUserBan> AspNetUserBan { get; set; }
        public DbSet<NewsletterRegistration> NewsletterRegistration { get; set; }
        public DbSet<YouTubeAlertRegistration> YouTubeAlertRegistration { get; set; }
        public DbSet<ItemsToDoList> ItemsToDoList { get; set; }
        public DbSet<Articles> Articles { get; set; }
        public DbSet<Comments> Comments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }

    public class Conf
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class AspNetUserBan
    {
        [Key]
        public string UserId { get; set; }
    }
}