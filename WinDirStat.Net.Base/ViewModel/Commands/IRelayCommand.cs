using System;
using System.ComponentModel;
using System.Windows.Input;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.ViewModel {
	/// <summary>An interface for working with any type of RelayCommand.</summary>
	public interface IRelayCommand : ICommand {

		#region CanExecuteChanged

		/// <summary>Raises the <see cref="ICommand.CanExecuteChanged"/> event.</summary>
		void RaiseCanExecuteChanged();

		#endregion

		#region Execute (No Parameter)

		/// <summary>Executes the method without a parameter.</summary>
		void Execute();

		#endregion
	}
}
