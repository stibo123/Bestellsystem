namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsEnabledToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "IsEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "IsEnabled");
        }
    }
}
