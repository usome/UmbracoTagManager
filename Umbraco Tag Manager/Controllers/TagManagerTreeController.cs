using System.Net.Http.Formatting;
using Umbraco.Core.Scoping;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace Umbraco_Tag_Manager.Controllers
{
    [PluginController(ConstantValues.PluginAlias)]
    [Tree(ConstantValues.SectionAlias, ConstantValues.TreeAlias, TreeGroup = ConstantValues.TreeGroup)]
    public class TagManagerTreeController : TreeController
    {
        private readonly TagManagerApiController _tManagerController;

        public TagManagerTreeController(IScopeProvider scopeProvider)
        {
            _tManagerController = new TagManagerApiController(scopeProvider);
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            if (id == global::Umbraco.Core.Constants.System.Root.ToString())
            {
                //top level nodes - generate list of tag groups that this user has access to.       
                var tree = new TreeNodeCollection();
                foreach (var tagGroup in _tManagerController.GetTagGroups())
                {
                    var item = CreateTreeNode("tagGroup-" + tagGroup.Group, id, null, tagGroup.Group, "icon-bulleted-list", true, queryStrings.GetValue<string>("application"));
                    tree.Add(item);
                }

                return tree;
            }
            else
            {
                //List all tags under group

                //Get tag groupname
                var groupName = id.Substring(id.IndexOf('-') + 1);

                var tree = new TreeNodeCollection();

                var cmsTags = _tManagerController.GetAllTagsInGroup(groupName);

                foreach (var tag in cmsTags)
                {
                    var item = CreateTreeNode(tag.Id.ToString(), groupName, queryStrings,
                        $"{tag.Tag} ({tag.NoTaggedNodes.ToString()})", "icon-bulleted-list", false);
                    tree.Add(item);
                }

                return tree;
            }
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id.Contains("tag-"))
            {
                menu.Items.Add(new MenuItem("delete", "Delete"));
            }

            return menu;
        }
    }
}