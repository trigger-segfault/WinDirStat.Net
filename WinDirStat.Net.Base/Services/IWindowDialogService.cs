using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Services {
	/// <summary>A service for launching dialogs and showing messages.</summary>
	public interface IWindowDialogService {

		/// <summary>Shows the dialog for selecting paths to scan.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog.</param>
		/// <returns>The selected root paths on success, otherwise null.</returns>
		DriveSelectResult ShowDriveSelect(IWindow owner);

		/// <summary>Shows the folder browser dialog to select a folder.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog.</param>
		/// <param name="description">The description to display.</param>
		/// <param name="showNewFolder">True if the new folder button is present.</param>
		/// <param name="selectedPath">The currently selected path. Use an empty string for nothing.</param>
		/// <returns>The selected path on success, otherwise null.</returns>
		string ShowFolderBrowser(IWindow owner, string description, bool showNewFolder, string selectedPath = "");

		/// <summary>Shows a message with no icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		MessageResult ShowMessage(IWindow owner, string message, string title, MessageButton button = MessageButton.OK);

		/// <summary>Shows a message with an information icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		MessageResult ShowInformation(IWindow owner, string message, string title, MessageButton button = MessageButton.OK);

		/// <summary>Shows a message with a question icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		MessageResult ShowQuestion(IWindow owner, string message, string title, MessageButton button = MessageButton.OK);

		/// <summary>Shows a message with a warning icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		MessageResult ShowWarning(IWindow owner, string message, string title, MessageButton button = MessageButton.OK);

		/// <summary>Shows a message with an error icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		MessageResult ShowError(IWindow owner, string message, string title, MessageButton button = MessageButton.OK);
	}
}
