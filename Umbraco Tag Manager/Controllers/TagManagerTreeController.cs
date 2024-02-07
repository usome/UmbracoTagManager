using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Umbraco_Tag_Manager.Controllers
{
    [PluginController(ConstantValues.PluginAlias)]
    [Tree(ConstantValues.SectionAlias, ConstantValues.TreeAlias, TreeGroup = ConstantValues.TreeGroup)]
    public class TagManagerTreeController : TreeController
    {
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly TagManagerApiController _tManagerController;

        public TagManagerTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IEventAggregator eventAggregator,
            IScopeProvider scopeProvider,
            ILogger<TagManagerApiController> logger,
            IMediaService mediaService,
            IContentService contentService,
            ITagService tagService)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory ?? throw new ArgumentNullException(nameof(menuItemCollectionFactory));
            _tManagerController = new TagManagerApiController(
                scopeProvider,
                logger,
                mediaService,
                contentService,
                tagService);
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            var tree = new TreeNodeCollection();

            if (id == global::Umbraco.Cms.Core.Constants.System.Root.ToString())
            {
                // Top level nodes - generate a list of tag groups that this user has access to.
                foreach (var tagGroup in _tManagerController.GetTagGroups())
                {
                    var item = CreateTreeNode("tagGroup-" + tagGroup.Group, id, null, tagGroup.Group, "icon-bulleted-list");
                    // Set the URL to navigate to the detail page
                    item.RoutePath = $"{ConstantValues.SectionAlias}/{ConstantValues.TreeAlias}/{ConstantValues.DetailAction}/{tagGroup.Group}";
                    tree.Add(item);
                }
            }

            return tree;
        }




        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();

            if (id.Contains("tagGroup-"))
            {
                // If the node is a tag group, add the "Create" option
                menu.Items.Add(new MenuItem("create", "Create")
                {
                    Icon = "add", // Set the icon to "add"
                    SeparatorBefore = true, // Add a separator before this menu item
                });
            }

            if (id.Contains("tag-"))
            {
                // If the node is a tag, add the "Delete" option
                menu.Items.Add(new MenuItem("delete", "Delete"));
            }

            return menu;
        }

        [HttpPost]
        public ActionResult<bool> PostMethod()
        {
            return true; 
        }

    }
}