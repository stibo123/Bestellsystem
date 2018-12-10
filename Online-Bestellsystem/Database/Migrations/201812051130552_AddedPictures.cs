namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPictures : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Products", "ProductImage_Id", c => c.Int());
            CreateIndex("dbo.Products", "ProductImage_Id");
            AddForeignKey("dbo.Products", "ProductImage_Id", "dbo.ProductImages", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Products", "ProductImage_Id", "dbo.ProductImages");
            DropIndex("dbo.Products", new[] { "ProductImage_Id" });
            DropColumn("dbo.Products", "ProductImage_Id");
            DropTable("dbo.ProductImages");
        }
    }
}
