namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrationAddedRoles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Admins", "Role_Id", c => c.Int());
            AddColumn("dbo.Employees", "Role_Id", c => c.Int());
            AddColumn("dbo.Suppliers", "Role_Id", c => c.Int());
            CreateIndex("dbo.Admins", "Role_Id");
            CreateIndex("dbo.Employees", "Role_Id");
            CreateIndex("dbo.Suppliers", "Role_Id");
            AddForeignKey("dbo.Admins", "Role_Id", "dbo.Roles", "Id");
            AddForeignKey("dbo.Employees", "Role_Id", "dbo.Roles", "Id");
            AddForeignKey("dbo.Suppliers", "Role_Id", "dbo.Roles", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Suppliers", "Role_Id", "dbo.Roles");
            DropForeignKey("dbo.Employees", "Role_Id", "dbo.Roles");
            DropForeignKey("dbo.Admins", "Role_Id", "dbo.Roles");
            DropIndex("dbo.Suppliers", new[] { "Role_Id" });
            DropIndex("dbo.Employees", new[] { "Role_Id" });
            DropIndex("dbo.Admins", new[] { "Role_Id" });
            DropColumn("dbo.Suppliers", "Role_Id");
            DropColumn("dbo.Employees", "Role_Id");
            DropColumn("dbo.Admins", "Role_Id");
            DropTable("dbo.Roles");
        }
    }
}
