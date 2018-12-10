namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUser : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Admins", newName: "Users");
            DropForeignKey("dbo.Orders", "Employee_Id", "dbo.Employees");
            DropForeignKey("dbo.Products", "Supplier_Id", "dbo.Suppliers");
            DropForeignKey("dbo.Suppliers", "Role_Id", "dbo.Roles");
            DropIndex("dbo.Employees", new[] { "Role_Id" });
            DropIndex("dbo.Products", new[] { "Supplier_Id" });
            DropIndex("dbo.Orders", new[] { "Employee_Id" });
            DropIndex("dbo.Suppliers", new[] { "Role_Id" });
            CreateTable(
                "dbo.SupplierProperties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompanyName = c.String(),
                        DeliveryTime = c.DateTime(nullable: false),
                        DeadLine = c.DateTime(nullable: false),
                        WantEmail = c.Boolean(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            AddColumn("dbo.Users", "Email", c => c.String(nullable: false));
            AddColumn("dbo.Products", "SupplierProperties_Id", c => c.Int());
            AddColumn("dbo.Orders", "User_Id", c => c.Int());
            CreateIndex("dbo.Products", "SupplierProperties_Id");
            CreateIndex("dbo.Orders", "User_Id");
            AddForeignKey("dbo.Orders", "User_Id", "dbo.Users", "Id");
            AddForeignKey("dbo.Products", "SupplierProperties_Id", "dbo.SupplierProperties", "Id");
            DropColumn("dbo.Products", "Supplier_Id");
            DropColumn("dbo.Orders", "Employee_Id");
            DropTable("dbo.Employees");
            DropTable("dbo.Suppliers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Suppliers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompanyName = c.String(),
                        Username = c.String(),
                        Password = c.String(),
                        Email = c.String(),
                        DeliveryTime = c.DateTime(nullable: false),
                        DeadLine = c.DateTime(nullable: false),
                        WantEmail = c.Boolean(nullable: false),
                        Role_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Firstname = c.String(nullable: false),
                        Lastname = c.String(nullable: false),
                        Username = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Password = c.String(nullable: false),
                        Identifier = c.Int(),
                        Role_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Orders", "Employee_Id", c => c.Int());
            AddColumn("dbo.Products", "Supplier_Id", c => c.Int());
            DropForeignKey("dbo.SupplierProperties", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Products", "SupplierProperties_Id", "dbo.SupplierProperties");
            DropForeignKey("dbo.Orders", "User_Id", "dbo.Users");
            DropIndex("dbo.SupplierProperties", new[] { "User_Id" });
            DropIndex("dbo.Orders", new[] { "User_Id" });
            DropIndex("dbo.Products", new[] { "SupplierProperties_Id" });
            DropColumn("dbo.Orders", "User_Id");
            DropColumn("dbo.Products", "SupplierProperties_Id");
            DropColumn("dbo.Users", "Email");
            DropTable("dbo.SupplierProperties");
            CreateIndex("dbo.Suppliers", "Role_Id");
            CreateIndex("dbo.Orders", "Employee_Id");
            CreateIndex("dbo.Products", "Supplier_Id");
            CreateIndex("dbo.Employees", "Role_Id");
            AddForeignKey("dbo.Suppliers", "Role_Id", "dbo.Roles", "Id");
            AddForeignKey("dbo.Products", "Supplier_Id", "dbo.Suppliers", "Id");
            AddForeignKey("dbo.Orders", "Employee_Id", "dbo.Employees", "Id");
            RenameTable(name: "dbo.Users", newName: "Admins");
        }
    }
}
