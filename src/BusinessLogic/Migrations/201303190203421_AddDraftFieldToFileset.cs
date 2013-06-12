namespace BusinessLogic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDraftFieldToFileset : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FileSet", "IsDraft", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FileSet", "IsDraft");
        }
    }
}
