namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeletedDeliveryTime : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SupplierProperties", "DeliveryTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SupplierProperties", "DeliveryTime", c => c.DateTime(nullable: false));
        }
    }
}
