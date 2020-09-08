using System.Runtime.Serialization;

namespace DevSpace.Common {
	[DataContract]
	public class SponsorLevel : ISponsorLevel {
		private SponsorLevel() {}

		[DataMember]public int Id { get; private set; }
		[DataMember]public int DisplayOrder { get; private set; }
		[DataMember]public string DisplayName { get; private set; }
		[DataMember]public bool DisplayInSidebar { get; private set; }
		[DataMember]public bool DisplayInEmails { get; private set; }
		[DataMember]public bool DisplayLink { get; private set; }
		[DataMember]public int TimeOnScreen { get; private set; }
	}
}
