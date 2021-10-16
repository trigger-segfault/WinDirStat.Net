using System.Windows;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.ViewModel {
    /// <summary>An addition to the <see cref="ViewModelBase"/> class with extra helper functions.</summary>
    public abstract class ViewModelWindow : ViewModelRelayCommand {

		#region Fields

		/// <summary>The window owning this view model.</summary>
		private Window windowOwner;

		#endregion

		#region Constructors

		public ViewModelWindow(RelayCommandFactory relayFactory) : base(relayFactory) { }

		#endregion

		#region Abstract Properties

		/// <summary>Gets the title to display for the window.</summary>
		public virtual string Title => "WinDirStat.Net";

		#endregion

		#region Properties

		/// <summary>Gets or sets the window owning this view model.</summary>
		public Window WindowOwner {
			get => windowOwner;
			set => SetProperty(ref windowOwner, value);
		}
		
		#endregion
	}
}
