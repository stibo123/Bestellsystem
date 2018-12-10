namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DbCreation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "Email", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "Email");
        }
    }
}
