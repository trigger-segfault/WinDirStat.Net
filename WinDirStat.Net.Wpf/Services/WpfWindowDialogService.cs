using System.Media;
using System.Windows;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.Wpf.Windows;

namespace WinDirStat.Net.Wpf.Services {
	/// <summary>A service for launching dialogs and showing messages.</summary>
	public class WpfWindowDialogService : IWindowDialogService {

		#region Fields

		/// <summary>The service for performing UI actions such as dispatcher invoking.</summary>
		private readonly IUIService ui;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="WpfWindowDialogService"/>.</summary>
		public WpfWindowDialogService(IUIService ui) {
			this.ui = ui;
		}

		#endregion

		#region Dialogs

		/// <summary>Shows the dialog for selecting paths to scan.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog.</param>
		/// <returns>The selected root paths on success, otherwise null.</returns>
		public DriveSelectResult ShowDriveSelect(IWindow owner) {
			return ui.Invoke(() => {
				return DriveSelectDialog.ShowDialog((Window) owner.Window);
			});
		}

		/// <summary>Shows the folder browser dialog to select a folder.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog.</param>
		/// <param name="description">The description to display.</param>
		/// <param name="showNewFolder">True if the new folder button is present.</param>
		/// <param name="selectedPath">The currently selected path. Use an empty string for nothing.</param>
		/// <returns>The selected path on success, otherwise null.</returns>
		public string ShowFolderBrowser(IWindow owner, string description, bool showNewFolder, string selectedPath = "") {
			return ui.Invoke(() => {
				FolderBrowserDialog dialog = new FolderBrowserDialog() {
					Description = description,
					ShowNewFolderButton = showNewFolder,
					SelectedPath = selectedPath,
				};
				bool? result = dialog.ShowDialog((Window) owner.Window);
				if (result ?? false) {
					return dialog.SelectedPath;
				}
				return null;
			});
		}

		#endregion

		#region Messages

		/// <summary>Shows a message with no icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		public MessageResult ShowMessage(IWindow owner, string message, string title, MessageButton button = MessageButton.OK) {
			return ui.Invoke(() => {
				return (MessageResult) MessageBox.Show((Window) owner.Window, message, title, (MessageBoxButton) button);
			});
		}

		/// <summary>Shows a message with an information icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		public MessageResult ShowInformation(IWindow owner, string message, string title, MessageButton button = MessageButton.OK) {
			return ui.Invoke(() => {
				SystemSounds.Asterisk.Play();
				return (MessageResult) MessageBox.Show((Window) owner.Window, message, title, (MessageBoxButton) button, MessageBoxImage.Information);
			});
		}

		/// <summary>Shows a message with a question icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		public MessageResult ShowQuestion(IWindow owner, string message, string title, MessageButton button = MessageButton.OK) {
			return ui.Invoke(() => {
				SystemSounds.Asterisk.Play();
				return (MessageResult) MessageBox.Show((Window) owner.Window, message, title, (MessageBoxButton) button, MessageBoxImage.Question);
			});
		}

		/// <summary>Shows a message with a warning icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		public MessageResult ShowWarning(IWindow owner, string message, string title, MessageButton button = MessageButton.OK) {
			return ui.Invoke(() => {
				SystemSounds.Exclamation.Play();
				return (MessageResult) MessageBox.Show((Window) owner.Window, message, title, (MessageBoxButton) button, MessageBoxImage.Warning);
			});
		}

		/// <summary>Shows a message with an error icon.</summary>
		/// 
		/// <param name="owner">The owner window for this dialog message.</param>
		/// <param name="message">The text message.</param>
		/// <param name="title">The message window title.</param>
		/// <param name="button">The message buttons to display.</param>
		public MessageResult ShowError(IWindow owner, string message, string title, MessageButton button = MessageButton.OK) {
			return ui.Invoke(() => {
				SystemSounds.Hand.Play();
				return (MessageResult) MessageBox.Show((Window) owner.Window, message, title, (MessageBoxButton) button, MessageBoxImage.Error);
			});
		}

		#endregion
	}
}
