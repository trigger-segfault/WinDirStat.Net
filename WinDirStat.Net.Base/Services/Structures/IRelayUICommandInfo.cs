using WinDirStat.Net.ViewModel;

namespace WinDirStat.Net.Services.Structures {
	/// <summary>An interface with construction information for a <see cref="IRelayUICommand"/>.</summary>
	public interface IRelayUICommandInfo : IRelayCommandInfo {

		/// <summary>Gets the text to display for the command.</summary>
		string Text { get; }
		/// <summary>Gets the icon to display for the command.</summary>
		IImage Icon { get; }
		/// <summary>Gets the shortcut for the command.</summary>
		IShortcut Shortcut { get; }
	}
}
