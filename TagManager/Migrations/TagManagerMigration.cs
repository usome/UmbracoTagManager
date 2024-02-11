using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Packaging;

namespace TagManager.Migrations
{
    public class TagManagerMigration : PackageMigrationBase
    {
        public TagManagerMigration(
            IPackagingService packagingService,
            IMediaService mediaService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IMigrationContext context,
            IOptions<PackageMigrationSettings> packageMigrationsSettings)
            : base(
                packagingService,
                mediaService,
                mediaFileManager,
                mediaUrlGenerators,
                shortStringHelper,
                contentTypeBaseServiceProvider,
                context,
                packageMigrationsSettings)
        {
        }

        protected override void Migrate()
        {
            // Your migration logic goes here
            // For example, you can import resources or update database schema
            // using Umbraco services and APIs.
        }
    }
}
