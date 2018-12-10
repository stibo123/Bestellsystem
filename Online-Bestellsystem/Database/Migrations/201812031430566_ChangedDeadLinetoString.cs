namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedDeadLinetoString : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SupplierProperties", "DeadLine", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SupplierProperties", "DeadLine", c => c.DateTime(nullable: false));
        }
    }
}
