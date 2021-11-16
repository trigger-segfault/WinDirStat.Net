using Ookii.Dialogs.Wpf;
using System.Media;
using System.Windows;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Services;
using WinDirStat.Net.Wpf.Windows;

namespace WinDirStat.Net.Wpf.Services {
    /// <summary>A service for launching dialogs and showing messages.</summary>
    public class DialogService : IMyDialogService {

		/// <summary>Shows the dialog for selecting paths to scan.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog.</param>
		/// <returns>The selected root paths on success, otherwise null.</returns>
		public DriveSelectResult ShowDriveSelect(Window owner) {
			return DriveSelectDialog.ShowDialog(owner);
		}

		/// <summary>Shows the folder browser dialog to select a folder.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog.</param>
		/// <param name="description">The description to display.</param>
		/// <param name="showNewFolder">True if the new folder button is present.</param>
		/// <param name="selectedPath">The currently selected path. Use an empty string for nothing.</param>
		/// <returns>The selected path on success, otherwise null.</returns>
		public string ShowFolderBrowser(Window owner, string description, bool showNewFolder, string selectedPath = "") {
			var dialog = new VistaFolderBrowserDialog() {
				Description = description,
				ShowNewFolderButton = showNewFolder,
				SelectedPath = selectedPath,
			};

            return (dialog.ShowDialog() ?? false) ? dialog.SelectedPath : null;
        }

        /// <summary>Shows a message with no icon.</summary>
        /// 
        /// <param name="owner">The owner window for this dialog message.</param>
        /// <param name="message">The text message.</param>
        /// <param name="title">The message window title.</param>
        /// <param name="button">The message buttons to display.</param>
        public MessageBoxResult ShowMessage(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.OK) {
			return MessageBox.Show(owner, message, title, button);
		}

		/// <summary>Shows a message with an information icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		public MessageBoxResult ShowInformation(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.OK) {
			SystemSounds.Asterisk.Play();
			return MessageBox.Show(owner, message, title, button, MessageBoxImage.Information);
		}

		/// <summary>Shows a message with a question icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		public MessageBoxResult ShowQuestion(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.OK) {
			SystemSounds.Asterisk.Play();
			return MessageBox.Show(owner, message, title, button, MessageBoxImage.Question);
		}

		/// <summary>Shows a message with a warning icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		public MessageBoxResult ShowWarning(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.OK) {
			SystemSounds.Exclamation.Play();
			return MessageBox.Show(owner, message, title, button, MessageBoxImage.Warning);
		}

		/// <summary>Shows a message with an error icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		public MessageBoxResult ShowError(Window owner, string message, string title, MessageBoxButton button = MessageBoxButton.OK) {
			SystemSounds.Hand.Play();
			return MessageBox.Show(owner, message, title, button, MessageBoxImage.Error);
		}
	}
}
