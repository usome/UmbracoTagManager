using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco_Tag_Manager.Models;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Core.Scoping;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco_Tag_Manager.Controllers
{
    [PluginController(ConstantValues.PluginAlias)]
    public class TagManagerApiController : UmbracoAuthorizedJsonController
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ILogger<TagManagerApiController> _logger;
        private readonly IMediaService _mediaService;
        private readonly IContentService _contentService;
        private readonly ITagService _tagService;

        public TagManagerApiController(IScopeProvider scopeProvider, ILogger<TagManagerApiController> logger, IMediaService mediaService, IContentService contentService, ITagService tagService)
        {
            _scopeProvider = scopeProvider;
            _logger = logger;
            _mediaService = mediaService;
            _contentService = contentService;
            _tagService = tagService;
        }


        public CmsTags GetTagById(int tagId)
        {
            var tag = new CmsTags();

            try
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    var query = new Sql().Select(
                        $"id, tag, [group], propertytypeid, count(tagId) as noTaggedNodes FROM cmsTags LEFT JOIN cmsTagRelationship ON cmsTags.id = cmsTagRelationship.tagId Where cmsTags.Id = {tagId} GROUP BY tag, id, [group], propertytypeid;");

                    tag = scope.Database.Fetch<CmsTags>(query).FirstOrDefault();

                    var taggedDocs = GetTaggedDocumentNodeIds(tagId);

                    if (tag != null)
                    {
                        tag.TaggedDocuments = taggedDocs;

                        var taggedMedia = GetTaggedMediaNodeIds(tagId);

                        tag.TaggedMedia = taggedMedia;

                        var tagsInGroup = GetAllTagsInGroup(tagId);

                        tag.TagsInGroup = tagsInGroup;
                    }

                    scope.Complete();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTagById:");
            }


            return tag;
        }

        public IEnumerable<TagGroup> GetTagGroups()
        {
            IEnumerable<TagGroup> tagGroups = null;
            try
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    var query = new Sql().Select("[group] from cmstags GROUP BY [group] ORDER BY [group];");
                    tagGroups = scope.Database.Fetch<TagGroup>(query);
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllTagsInGroup:");
            }

            return tagGroups;
        }

        public TagInGroup GetAllTagsInGroup(int tagId)
        {
            var tagsInGroup = new TagInGroup();

            try
            {
                var listOfTags = new List<PlainPair>();

                using (var scope = _scopeProvider.CreateScope())
                {
                    var groupNameQuery = new Sql().Select($"[group] FROM cmsTags WHERE id={tagId}");
                    var resultGroupName = scope.Database.Single<string>(groupNameQuery);

                    var query = new Sql().Select(
                        $"id, tag FROM cmsTags where [group] = '{resultGroupName}' ORDER BY tag");

                    var results = scope.Database.Fetch<PlainPair>(query);

                    foreach (var result in results)
                    {
                        var t = new PlainPair { Id = Convert.ToInt32(result.Id), Tag = result.Tag };

                        listOfTags.Add(t);

                        if (result.Id == tagId)
                        {
                            tagsInGroup.SelectedItem = t;
                        }
                    }

                    tagsInGroup.Options = listOfTags;
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Error in GetAllTagsInGroup:");
            }

            return tagsInGroup;
        }

        [HttpGet]
        public IEnumerable<CmsTags> GetAllTagsInGroup(string groupName)
        {
            IEnumerable<CmsTags> tags = null;
            try
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    var query = new Sql().Select(
                        $"id, tag, [group], count(tagId) as noTaggedNodes FROM cmstags LEFT JOIN cmsTagRelationship ON cmsTags.id = cmsTagRelationship.tagId WHERE [group] = '{groupName}' GROUP BY tag, id, [group] ORDER BY tag");

                    tags = scope.Database.Fetch<CmsTags>(query);
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllTagsInGroup:");
            }
            return tags;
        }

        public List<TaggedDocument> GetTaggedDocumentNodeIds(int tagId)
        {
            var docs = new List<TaggedDocument>();

            try
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    var query = new Sql().Select("nodeId as DocumentId").From("cmsTagRelationship").Where(
                        $"tagID = {tagId}");

                    var results = scope.Database.Fetch<TaggedDocument>(query);
                    foreach (var result in results)
                    {
                        var contentService = _contentService;
                        var content = contentService.GetById(result.DocumentId);

                        if (content != null)
                        {
                            if (!string.IsNullOrWhiteSpace(content.Name))
                            {
                                var document = new TaggedDocument
                                {
                                    DocumentId = result.DocumentId,
                                    DocumentName = content.Name,
                                    DocumentUrl = $"#/content/content/edit/{result.DocumentId.ToString()}"
                                };
                                docs.Add(document);
                            }
                        }
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTaggedDocumentNodeIds:");
            }
            return docs;
        }

        public List<TaggedMedia> GetTaggedMediaNodeIds(int tagId)
        {
            // This method is intentionally left empty and does not fetch or process any tagged media.
            // If you want to completely remove the "Tagged Media" functionality, this method can be kept empty.

            var medias = new List<TaggedMedia>();
            return medias;
        }

        public int MoveTaggedNodes(int currentTagId, int newTagId)
        {
            var success = 0;

            try
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    success = scope.Database.Execute("Update cmsTagRelationship SET tagID = @1 WHERE tagID = @0 AND nodeId NOT IN (SELECT nodeId FROM cmsTagRelationship WHERE tagId = @1);", currentTagId, newTagId);

                    if (success == 1)
                    {
                        success = scope.Database.Execute("DELETE FROM cmsTagRelationship WHERE tagId = @0;", currentTagId);
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MoveTaggedNodes:");
            }
            return success;
        }

        public int Save(CmsTags tag)
        {
            var success = 0;

            try
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    success = scope.Database.Execute("Update cmsTags set tag = @0 where id = @1", tag.Tag, tag.Id);

                    if (success == 1 && tag.Id != tag.TagsInGroup.SelectedItem.Id)
                    {
                        // Merge tags
                        var sqlQuery1 = string.Format("Update cmsTagRelationship SET tagID = {0} WHERE tagID = {1} AND nodeId NOT IN (SELECT nodeId FROM cmsTagRelationship WHERE tagId = {0});", tag.TagsInGroup.SelectedItem.Id, tag.Id);

                        success = scope.Database.Execute(sqlQuery1);

                        // Delete tag
                        var sqlQuery2 = $"DELETE FROM cmsTagRelationship WHERE tagId = {tag.Id};";
                        scope.Database.Execute(sqlQuery2);
                    }

                    UpdateDocuments(tag);
                    UpdateMedia(tag);

                    scope.Complete();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Save:");
            }

            return success;
        }


        private void UpdateDocuments(CmsTags tag)
        {
            try
            {
                if (tag.TaggedDocuments != null && tag.TaggedDocuments.Count > 0)
                {
                    var contentService = _contentService;
                    var tagService = _tagService;

                    foreach (var doc in tag.TaggedDocuments)
                    {
                        var content = contentService.GetById(doc.DocumentId);

                        foreach (var property in content.Properties)
                        {
                            var propertyAlias = property.Alias;
                            var tags = tagService.GetTagsForEntity(doc.DocumentId, tag.Group);
                            IEnumerable<string> tagList = tags.Select(x => x.Text).ToList();

                            // Assuming the AssignTags method takes 2 arguments
                            property.SetValue(tagList);
                        }

                        contentService.Save(content);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateDocuments:");
            }
        }

        private void UpdateMedia(CmsTags tag)
        {
            // This method is intentionally left empty and does not update or process any media related to the tag.
            // If you want to completely remove the "Tagged Media" functionality, this method can be kept empty.
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        [AcceptVerbs("POST", "GET")]
        public int DeleteTag(CmsTags tag)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var sqlQuery1 = $"DELETE FROM cmsTagRelationship WHERE tagId = {tag.Id};";
                scope.Database.Execute(sqlQuery1);
                scope.Complete();
            }

            var success = 0;
            using (var scope = _scopeProvider.CreateScope())
            {
                var sqlQuery2 = $"DELETE FROM cmsTags WHERE id = {tag.Id};";
                success = scope.Database.Execute(sqlQuery2);
                scope.Complete();
            }

            UpdateDocuments(tag);
            UpdateMedia(tag);

            return success;
        }
        public int CreateTag(CmsTags tag)
        {
            var success = 0;

            try
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    if (tag.Id == 0)
                    {
                        // Insert new tag with the specified group
                        var insertQuery = "INSERT INTO cmsTags (tag, [group]) VALUES (@0, @1)";
                        success = scope.Database.Execute(insertQuery, tag.Tag, "default");
                        tag.Id = GetLastInsertedId(scope);
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create:");
            }

            return tag.Id; 
        }

        public int GetLastInsertedId(IScope scope)
        {
            return scope.Database.ExecuteScalar<int>("SELECT CAST(SCOPE_IDENTITY() AS INT)");
        }
    }
}