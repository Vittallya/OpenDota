namespace OpenDota.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HeroResults",
                c => new
                    {
                        HeroId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ChoosenByCreator = c.Boolean(nullable: false),
                        RoomResultId = c.Int(nullable: false),
                        RoomResult_RoomId = c.Int(),
                        RoomResult_RoomId1 = c.Int(),
                    })
                .PrimaryKey(t => t.HeroId)
                .ForeignKey("dbo.RoomResults", t => t.RoomResult_RoomId)
                .ForeignKey("dbo.RoomResults", t => t.RoomResult_RoomId1)
                .Index(t => t.RoomResult_RoomId)
                .Index(t => t.RoomResult_RoomId1);
            
            CreateTable(
                "dbo.RoomResults",
                c => new
                    {
                        RoomId = c.Int(nullable: false),
                        Comment = c.String(),
                        Result = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RoomId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HeroResults", "RoomResult_RoomId1", "dbo.RoomResults");
            DropForeignKey("dbo.HeroResults", "RoomResult_RoomId", "dbo.RoomResults");
            DropIndex("dbo.HeroResults", new[] { "RoomResult_RoomId1" });
            DropIndex("dbo.HeroResults", new[] { "RoomResult_RoomId" });
            DropTable("dbo.RoomResults");
            DropTable("dbo.HeroResults");
        }
    }
}
