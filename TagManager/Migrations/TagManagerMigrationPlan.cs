using System;
using Umbraco.Cms.Core.Packaging;

namespace TagManager.Migrations
{
    public class TagManagerMigrationPlan : PackageMigrationPlan
    {
        public TagManagerMigrationPlan() : base("TagManager")
        {
        }

        protected override void DefinePlan()
        {
            // Use a unique GUID for each migration step
            To<TagManagerMigration>(new Guid("12345678-1234-5678-1234-567812345678"));
            // Add more migration steps as needed
        }
    }
}
