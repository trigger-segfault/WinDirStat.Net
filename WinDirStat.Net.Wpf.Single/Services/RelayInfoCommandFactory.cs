using Microsoft.Toolkit.Mvvm.Input;
using System;
using WinDirStat.Net.Services;
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
        public override IRelayCommand Create(Action execute, Func<bool> canExecute) => new RelayInfoCommand(execute, canExecute);

        /// <summary>
        /// Creates a new <see cref="IRelayCommand"/> with the specified info and functions.
        /// </summary>
        /// 
        /// <typeparam name="T">The parameter type of the command.</typeparam>
        /// <param name="info">The abstract information about the command.</param>
        /// <param name="execute">The execution action.</param>
        /// <param name="canExecute">The optional can execute function.</param>
        /// <returns>The constructed <see cref="IRelayCommand{T}"/></returns>
        public override IRelayCommand<T> Create<T>(Action<T> execute, Predicate<T> canExecute) => new RelayInfoCommand<T>(execute, canExecute);

        #endregion
    }
}
