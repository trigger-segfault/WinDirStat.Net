using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinDirStat.Net.ViewModel.Commands {
	/// <summary>A base interface for working with any type of RelayCommand.</summary>
	public interface IRelayCommandBase : ICommand {
		
		/// <summary>Raises the <see cref="ICommand.CanExecuteChanged"/> event.</summary>
		void RaiseCanExecuteChanged();
	}
}
