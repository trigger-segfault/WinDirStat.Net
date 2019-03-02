using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.ViewModel {
	/// <summary>An addition to the <see cref="ViewModelCommandBase"/> class with extra helper functions.</summary>
	public abstract class ViewModelWindowBase : ViewModelCommandBase {

		#region Fields

		/// <summary>The window owning this view model.</summary>
		private IWindow windowOwner;

		#endregion

		#region Virtual Properties

		/// <summary>Gets the title to display for the window.</summary>
		public virtual string Title => "WinDirStat.Net";

		#endregion

		#region Properties

		/// <summary>Gets or sets the window owning this view model.</summary>
		public IWindow WindowOwner {
			get => windowOwner;
			set => Set(ref windowOwner, value);
		}

		#endregion
	}
}
