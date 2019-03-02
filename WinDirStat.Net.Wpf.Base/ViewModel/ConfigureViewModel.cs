using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.ViewModel {
	/// <summary>The view model for the configure dialog.</summary>
	public partial class ConfigureViewModel : ViewModelWindow {

		#region Fields

		// Services
		/// <summary>Gets the program settings service.</summary>
		public SettingsService Settings { get; }
		/// <summary>Gets the UI service.</summary>
		public UIService UI { get; }
		/// <summary>Gets the dialog service.</summary>
		public IMyDialogService Dialogs { get; }

		#endregion

		#region Constructors

		/// <summary>Constructrs the <see cref="DriveSelectViewModel"/>.</summary>
		public ConfigureViewModel(SettingsService settings,
								  UIService ui,
								  IMyDialogService dialog,
								  RelayCommandFactory relayFactory)
			: base(relayFactory)
		{
			Settings = settings;
			UI = ui;
			Dialogs = dialog;
		}

		#endregion

		#region Override Properties

		/// <summary>Gets the title to display for the window.</summary>
		public override string Title => "WinDirStat.Net - Settings";

		#endregion
	}
}
