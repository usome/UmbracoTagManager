using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Umbraco_Tag_Manager
{
    public class TagManagerComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Sections().Append<TagManagerSection>();
        }
    }

    public class TagManagerComponentComposer : ComponentComposer<TagManagerStartup>
    {

    }

    public class TagManagerStartup : IComponent
    {
		private readonly IScopeProvider _scopeProvider;
        private readonly ILogger _logger;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly IKeyValueService _keyValueService;

        public TagManagerStartup(IScopeProvider scopeProvider, ILogger logger, IMigrationBuilder migrationBuilder,
            IKeyValueService keyValueService)
        {
            _scopeProvider = scopeProvider;
            _logger = logger;
            _migrationBuilder = migrationBuilder;
            _keyValueService = keyValueService;
        }

        public void Initialize()
        {
            var migrationPlan = new MigrationPlan("UsomeTagManagerMigrationv1");
            migrationPlan.From(string.Empty).To<InstallHelper>("UsomeTagManagerMigrationv1-db");
            var upGrader = new Upgrader(migrationPlan);
            upGrader.Execute(_scopeProvider, _migrationBuilder, _keyValueService, _logger);
        }

        public void Terminate()
        {

        }
	}
}