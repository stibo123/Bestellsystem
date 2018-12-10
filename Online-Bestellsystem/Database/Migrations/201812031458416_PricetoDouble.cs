namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PricetoDouble : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Products", "Price", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "Price", c => c.Int(nullable: false));
        }
    }
}
