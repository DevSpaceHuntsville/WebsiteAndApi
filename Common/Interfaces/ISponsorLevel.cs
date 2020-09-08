namespace DevSpace.Common {
	public interface ISponsorLevel {
		int Id { get; }
		int DisplayOrder { get; }
		string DisplayName { get; }
		bool DisplayInSidebar { get; }
		bool DisplayInEmails { get; }
		bool DisplayLink { get; }
		int TimeOnScreen { get; }
	}
}
