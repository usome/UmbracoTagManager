using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Services;
using static Umbraco.Cms.Core.Constants;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco_Tag_Manager
{
    public class InstallHelper : MigrationBase
    {
        private readonly IUserService _userService;
        private readonly IScopeProvider _scopeProvider;
        private readonly IUmbracoContextFactory _umbracoContext;

        public InstallHelper(IUserService userService,
            IScopeProvider scopeProvider,
            IUmbracoContextFactory umbracoContext,
            IMigrationContext context) : base(context)
        {
            _userService = userService;
            _scopeProvider = scopeProvider;
            _umbracoContext = umbracoContext;
        }

        // Use 'protected' here to match the base class
        protected override void Migrate()
        {
            using (UmbracoContextReference umbracoContextReference = _umbracoContext.EnsureUmbracoContext())
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    var adminGroup = _userService.GetUserGroupByAlias(Security.AdminGroupAlias);
                    adminGroup.AddAllowedSection("tagManager");

                    _userService.Save(adminGroup);

                    scope.Complete();
                }
            }
        }
    }
}
