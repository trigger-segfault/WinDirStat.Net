using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services;
using WinDirStat.Net.ViewModel.Commands;
using WinDirStat.Net.Wpf.Commands;

namespace WinDirStat.Net.Wpf.Services {
	/// <summary>A service for creating relay commands to be loaded by the view model.</summary>
	public class RelayInfoCommandFactory : RelayCommandFactory {

		#region Create Abstract

		/// <summary>
		/// Creates a new <see cref="IRelayCommand"/> with the specified info and functions.
		/// </summary>
		/// 
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayCommand"/></returns>
		public override IRelayCommand Create(Action execute, Func<bool> canExecute,
			bool keepTargetAlive = false)
		{
			return new RelayInfoCommand(execute, canExecute, keepTargetAlive);
		}

		/// <summary>
		/// Creates a new <see cref="IRelayCommand"/> with the specified info and functions.
		/// </summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayCommand{T}"/></returns>
		public override IRelayCommand<T> Create<T>(Action<T> execute, Func<T, bool> canExecute,
			bool keepTargetAlive = false)
		{
			return new RelayInfoCommand<T>(execute, canExecute, keepTargetAlive);
		}

		#endregion
	}
}
