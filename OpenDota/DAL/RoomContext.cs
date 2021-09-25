using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OpenDota.DAL
{
    public class RoomContext: DbContext
    {
        public RoomContext(): base("RoomResults")
        {

        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoomResult>().Property(x => x.RoomId).
                HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            //base.OnModelCreating(modelBuilder);
        }

        public DbSet<HeroResult> HeroResult => Set<HeroResult>();
        public DbSet<RoomResult> RoomResult => Set<RoomResult>();
    }
}