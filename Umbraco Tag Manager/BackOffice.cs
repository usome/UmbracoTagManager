using Umbraco.Core.Models.Sections;

namespace Umbraco_Tag_Manager
{
	public class TagManagerSection : ISection
	{
		public string Alias => ConstantValues.SectionAlias;

		public string Name => ConstantValues.SectionName;
	}
}