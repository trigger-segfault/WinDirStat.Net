﻿using Microsoft.Toolkit.Mvvm.Input;
using System;

namespace WinDirStat.Net.Services {
    /// <summary>A service for creating relay commands to be loaded by the view model.</summary>
    public abstract class RelayCommandFactory {

        #region Create

        /// <summary>
        /// Creates a new <see cref="IRelayCommand"/> with the specified info and functions.
        /// </summary>
        /// 
        /// <param name="info">The abstract information about the command.</param>
        /// <param name="execute">The execution action.</param>
        /// <param name="canExecute">The optional can execute function.</param>
        /// <returns>The constructed <see cref="IRelayCommand"/></returns>
        public IRelayCommand Create(Action execute) => Create(execute, null);

        /// <summary>
        /// Creates a new <see cref="IRelayCommand"/> with the specified info and functions.
        /// </summary>
        /// 
        /// <typeparam name="T">The parameter type of the command.</typeparam>
        /// <param name="info">The abstract information about the command.</param>
        /// <param name="execute">The execution action.</param>
        /// <param name="canExecute">The optional can execute function.</param>
        /// <returns>The constructed <see cref="IRelayCommand{T}"/></returns>
        public IRelayCommand<T> Create<T>(Action<T> execute) => Create(execute, null);

        #endregion

        #region Create Abstract

        /// <summary>
        /// Creates a new <see cref="IRelayCommand"/> with the specified info and functions.
        /// </summary>
        /// 
        /// <param name="info">The abstract information about the command.</param>
        /// <param name="execute">The execution action.</param>
        /// <param name="canExecute">The optional can execute function.</param>
        /// <returns>The constructed <see cref="IRelayCommand"/></returns>
        public abstract IRelayCommand Create(Action execute, Func<bool> canExecute);

		/// <summary>
		/// Creates a new <see cref="IRelayCommand"/> with the specified info and functions.
		/// </summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="info">The abstract information about the command.</param>
		/// <param name="execute">The execution action.</param>
		/// <param name="canExecute">The optional can execute function.</param>
		/// <returns>The constructed <see cref="IRelayCommand{T}"/></returns>
		public abstract IRelayCommand<T> Create<T>(Action<T> execute, Predicate<T> canExecute);

		#endregion
	}
}
