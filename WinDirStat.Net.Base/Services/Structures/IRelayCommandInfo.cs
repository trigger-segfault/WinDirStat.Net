using System;
using System.Windows.Input;
using WinDirStat.Net.ViewModel;

namespace WinDirStat.Net.Services.Structures {
	/// <summary>An interface with construction information for a <see cref="IRelayCommand"/>.</summary>
	public interface IRelayCommandInfo {
		
		/*#region Properties

		/// <summary>Gets an optional action to call before <see cref="ICommand.Execute"/>.</summary>
		Action ExecuteBefore { get; }
		/// <summary>Gets an optional action to call after <see cref="ICommand.Execute"/>.</summary>
		Action ExecuteAfter { get; }

		/// <summary>Gets an optional function to call before <see cref="ICommand.CanExecute"/>.</summary>
		Func<bool> CanExecuteBefore { get; }
		/// <summary>Gets an optional function to call after <see cref="ICommand.CanExecute"/>.</summary>
		Func<bool> CanExecuteAfter { get; }

		#endregion*/
	}
}
