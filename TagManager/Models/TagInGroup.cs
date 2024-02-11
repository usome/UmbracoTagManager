using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco_Tag_Manager.Models
{
    [DataContract(Name = "tagInGroup", Namespace = "")]
    public class TagInGroup
    {
        [DataMember(Name = "selectedItem")]
        public PlainPair SelectedItem { get; set; }

        [DataMember(Name = "options")]
        public List<PlainPair> Options { get; set; }
    }
}