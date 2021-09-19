using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OpenDota.Models
{
    public class ApiContext: DbContext
    {
        public ApiContext(): base("OpenDotaTestDb")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>().Property(x => x.Id).
                HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            modelBuilder.Entity<User>().Property(x => x.Id).
                HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);

        }

        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<User> Users => Set<User>();
    }
}