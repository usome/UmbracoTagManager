using NPoco;

namespace Umbraco_Tag_Manager.Models
{
    [TableName("cmsTags")]
    public class TagGroup
    {
        public string Group { get; set; }
        public int GroupId { get; set; }
    }
}