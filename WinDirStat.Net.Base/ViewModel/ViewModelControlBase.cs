using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.ViewModel {
	/// <summary>An addition to the <see cref="ViewModelCommandBase"/> class with extra helper functions.</summary>
	public abstract class ViewModelControlBase : ViewModelCommandBase {

		#region Fields

		/// <summary>The control owning this view model.</summary>
		private IControl controlOwner;

		#endregion

		#region Properties

		/// <summary>Gets or sets the control owning this view model.</summary>
		public IControl WindowOwner {
			get => controlOwner;
			set => Set(ref controlOwner, value);
		}

		#endregion
	}
}
