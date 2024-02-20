using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco_Tag_Manager.Controllers
{
    public class SelectedTag
    {
        public int Id { get; set; }
        public IEnumerable<TagModel> Tags;
        public string Group { get; set; }
        public int PropertyId { get; set; }

        public string PropertyAlias { get; set; }
    }
}