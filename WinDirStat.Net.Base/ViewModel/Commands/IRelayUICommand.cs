using System.ComponentModel;
using System.Windows.Input;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.ViewModel {
	/// <summary>An interface for working with generic and non-generic RelayUICommands.</summary>
	public interface IRelayUICommand : IRelayCommand {
		string Text { get; }
		IImage Icon { get; }
		IShortcut Shortcut { get; }
	}
}
