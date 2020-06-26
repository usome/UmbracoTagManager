using System.Collections.Generic;
using System.Runtime.Serialization;
using NPoco;

namespace Umbraco_Tag_Manager.Models
{
    [TableName("cmsTags")]
    [PrimaryKey("id")]
    [DataContract(Name = "cmsTags", Namespace = "")]
    public class CmsTags
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "tag")]
        public string Tag { get; set; }

        [DataMember(Name = "group")]
        public string Group { get; set; }

        [DataMember(Name = "propertytypeid")]
        public int PropertyTypeId { get; set; }

        [DataMember(Name = "noTaggedNodes")]
        public int NoTaggedNodes { get; set; }

        [DataMember(Name = "tagsInGroup")]
        public TagInGroup TagsInGroup { get; set; }

        [DataMember(Name = "taggedDocuments")]
        public List<TaggedDocument> TaggedDocuments { get; set; }

        [DataMember(Name = "taggedMedia")]
        public List<TaggedMedia> TaggedMedia { get; set; }


    }
}