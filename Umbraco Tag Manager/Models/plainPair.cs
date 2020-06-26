using System.Runtime.Serialization;

namespace Umbraco_Tag_Manager.Models
{
    [DataContract(Name = "plainPair", Namespace = "")]
    public class PlainPair
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "tag")]
        public string Tag { get; set; }
    }
}