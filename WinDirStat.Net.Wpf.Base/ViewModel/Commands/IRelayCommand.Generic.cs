using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinDirStat.Net.ViewModel.Commands {
	/// <summary>An interface for working with any type of RelayCommand.</summary>
	public interface IRelayCommand<T> : IRelayCommandBase {
		
		/// <summary>Executes the method with a parameter.</summary>
		void Execute(T parameter);
	}
}
