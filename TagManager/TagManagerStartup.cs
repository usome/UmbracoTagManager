using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Services;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco_Tag_Manager
{
    public class TagManagerComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<TagManagerManifestFilter>();
            builder.Sections().Append<TagManagerSection>();
        }
    }

    public class TagManagerManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = "TagManager",
                Dashboards = new ManifestDashboard[]
                {
                    new ManifestDashboard()
                    {
                        Alias = "tagManager",
                        View = "/App_Plugins/TagManager/Dashboard.html",
                        Sections = new string[]
                        {
                            "TagManager"
                        }
                    }
                 },
                Scripts = new[]
                {
                    "/App_Plugins/TagManager/backoffice/TagManagerTree/TagManager.controller.js",
                    "/App_Plugins/TagManager/TagManager.resource.js"
                 },
                Stylesheets = new[]
                {
                    "/App_Plugins/TagManager/backoffice/TagManagerTree/TagManager.css"
                }
            });
        }
    }

    public class TagManagerComponentComposer : ComponentComposer<TagManagerStartup>
    {

    }

    public class TagManagerStartup : IComponent
    {
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;
        private readonly IKeyValueService _keyValueService;
        private readonly IRuntimeState _runtimeState;

        public TagManagerStartup(ICoreScopeProvider coreScopeProvider, IMigrationPlanExecutor migrationPlanExecutor, IKeyValueService keyValueService, IRuntimeState runtimeState)
        {
            _coreScopeProvider = coreScopeProvider;
            _migrationPlanExecutor = migrationPlanExecutor;
            _keyValueService = keyValueService;
            _runtimeState = runtimeState;
        }

        public void Initialize()
        {
            if (_runtimeState.Level < RuntimeLevel.Run)
            {
                return;
            }

            var migrationPlan = new MigrationPlan("UsomeTagManagerMigrationv1");
            migrationPlan.From(string.Empty).To<InstallHelper>("UsomeTagManagerMigrationv1-db");
            var upgrader = new Upgrader(migrationPlan);
            upgrader.Execute(_migrationPlanExecutor, _coreScopeProvider, _keyValueService);

        }

        public void Terminate()
        {

        }
    }
}