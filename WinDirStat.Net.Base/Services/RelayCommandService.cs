using System;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.ViewModel;

namespace WinDirStat.Net.Services {
	/// <summary>A service for creating relay commands to be loaded by the view model.</summary>
	public class RelayCommandService {

		#region Abstract Create

		/// <summary>
		/// Creates a new <see cref="IRelayCommand"/> with the specified info and functions.
		/// </summary>
		/// 
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayCommand"/></returns>
		IRelayCommand CreateCommand(Action execute, Func<bool> canExecute = null,
			bool keepTargetAlive = false);

		/// <summary>
		/// Creates a new <see cref="IRelayCommand"/> with the specified info and functions.
		/// </summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayCommand"/></returns>
		IRelayCommand CreateCommand<T>(Action<T> execute, Func<T, bool> canExecute = null,
			bool keepTargetAlive = false);

		/// <summary>
		/// Creates a new <see cref="IRelayUICommand"/> with the specified info and functions.
		/// </summary>
		/// 
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayUICommand"/></returns>
		IRelayUICommand CreateCommand(IRelayUICommandInfo info, Action execute, Func<bool> canExecute = null,
			bool keepTargetAlive = false);

		/// <summary>
		/// Creates a new <see cref="IRelayUICommand"/> with the specified info and functions.
		/// </summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayUICommand"/></returns>
		IRelayUICommand CreateCommand<T>(IRelayUICommandInfo info, Action<T> execute,
			Func<T, bool> canExecute = null, bool keepTargetAlive = false);

		#endregion
	}
}
