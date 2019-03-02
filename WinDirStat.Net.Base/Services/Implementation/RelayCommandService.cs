using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.ViewModel;

namespace WinDirStat.Net.Services.Implementation {
	public abstract class RelayCommandService {

		/// <summary>
		/// Creates a new <see cref="IRelayCommand"/> with the specified info and actions.
		/// </summary>
		/// 
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayCommand"/></returns>
		public abstract IRelayCommand Create(IRelayCommandInfo info, Action execute,
			Func<bool> canExecute = null);

		/// <summary>
		/// Creates a new <see cref="IRelayCommand"/> with the specified info and actions.
		/// </summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayCommand"/></returns>
		public abstract IRelayCommand Create<T>(IRelayCommandInfo info, Action<T> execute,
			Func<T, bool> canExecute = null);

		/// <summary>
		/// Creates a new <see cref="IRelayUICommand"/> with the specified info and actions.
		/// </summary>
		/// 
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayUICommand"/></returns>
		public abstract IRelayUICommand Create(IRelayUICommandInfo info, Action execute,
			Func<bool> canExecute = null);

		/// <summary>
		/// Creates a new <see cref="IRelayUICommand"/> with the specified info and actions.
		/// </summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayUICommand"/></returns>
		public abstract IRelayUICommand Create<T>(IRelayUICommandInfo info, Action<T> execute,
			Func<T, bool> canExecute = null);

	}
}
