using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco_Tag_Manager
{
    public class InstallHelper : MigrationBase
    {
        private readonly string[] _userGroups = { "admin", "editor", "writer" };

        public InstallHelper(IMigrationContext context) : base(context)
        {
        }

        // Use 'protected' here to match the base class
        protected override void Migrate()
        {
            var dbContext = Context.Database;
            if (TableExists("umbracoUserGroup2App"))
            {
                foreach (var groupAlias in _userGroups)
                {
                    var groupId =
                        dbContext.ExecuteScalar<int?>("select id from umbracoUserGroup where userGroupAlias = @0",
                            groupAlias);
                    if (groupId.HasValue && groupId != 0)
                    {
                        var rows = dbContext
                            .ExecuteScalar<int>(
                                "select count(*) from umbracoUserGroup2App where userGroupId = @0 and app = @1",
                                groupId.Value, ConstantValues.SectionAlias);
                        if (rows == 0)
                        {
                            dbContext.Execute("insert umbracoUserGroup2App values (@0, @1)", groupId,
                                ConstantValues.SectionAlias);
                        }
                    }
                }
            }
        }
    }
}
