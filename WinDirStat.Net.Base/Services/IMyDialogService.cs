using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinDirStat.Net.Model.Drives;

namespace WinDirStat.Net.Services {
	/// <summary>A service for launching dialogs and showing messages.</summary>
	public interface IMyDialogService {

		/// <summary>Shows the dialog for selecting paths to scan.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog.</param>
		/// <returns>The selected root paths on success, otherwise null.</returns>
		DriveSelectResult ShowDriveSelect(Window owner);

		/// <summary>Shows the folder browser dialog to select a folder.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog.</param>
		/// <param name="description">The description to display.</param>
		/// <param name="showNewFolder">True if the new folder button is present.</param>
		/// <param name="selectedPath">The currently selected path. Use an empty string for nothing.</param>
		/// <returns>The selected path on success, otherwise null.</returns>
		string ShowFolderBrowser(Window owner, string description, bool showNewFolder, string selectedPath = "");

		/// <summary>Shows a message with no icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		MessageBoxResult ShowMessage(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.OK);

		/// <summary>Shows a message with an information icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		MessageBoxResult ShowInformation(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.OK);

		/// <summary>Shows a message with a question icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		MessageBoxResult ShowQuestion(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.OK);

		/// <summary>Shows a message with a warning icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		MessageBoxResult ShowWarning(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.OK);

		/// <summary>Shows a message with an error icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		MessageBoxResult ShowError(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.OK);
	}
}
