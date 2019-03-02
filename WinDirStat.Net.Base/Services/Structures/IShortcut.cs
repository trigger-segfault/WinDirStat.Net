
namespace WinDirStat.Net.Services.Structures {
	/// <summary>An interface for a shortcut implementation.</summary>
	public interface IShortcut {
		
		/// <summary>The actual shortcut object.</summary>
		object Shortcut { get; }

		/// <summar>Gets the display text for the shortcut.</summar>
		string DisplayText { get; }

		/// <summar>Gets the display image for the shortcut.</summar>
		IImage DisplayImage { get; }
	}
}
