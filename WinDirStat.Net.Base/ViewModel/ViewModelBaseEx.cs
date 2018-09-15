using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
#if WPF
using GalaSoft.MvvmLight.CommandWpf;
#else
using GalaSoft.MvvmLight.Command;
#endif

namespace WinDirStat.Net.ViewModel {
	/// <summary>An addition to the <see cref="ViewModelBase"/> class with extra helper functions.</summary>
	public abstract class ViewModelBaseEx : ViewModelBase {

		#region Fields

		/// <summary>The window owning this view model.</summary>
		private Window windowOwner;
		/// <summary>The list of loaded commands.</summary>
		private readonly Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

		#endregion

		#region Abstract Properties

		/// <summary>Gets the title to display for the window.</summary>
		public virtual string Title => "WinDirStat.Net";

		#endregion

		#region Properties

		/// <summary>Gets or sets the window owning this view model.</summary>
		public Window WindowOwner {
			get => windowOwner;
			set => Set(ref windowOwner, value);
		}

		/// <summary>Gets all commands created with <see cref="GetCommand"/>.</summary>
		public IEnumerable<ICommand> Commands => commands.Values;

		#endregion

		#region GetCommand (Basic)

		/// <summary>
		/// Gets the commnd if one exists, or assigns the passed command.
		/// </summary>
		/// 
		/// <param name="newCommand">The new command to assign if one does not exist.</param>
		/// <param name="commandName">The name of the command.</param>
		/// <returns>The command from the command list.</returns>
		protected TCommand GetCommand<TCommand>(TCommand newCommand,
			[CallerMemberName] string commandName = null)
			where TCommand : ICommand
		{
			if (!commands.TryGetValue(commandName, out ICommand command)) {
				command = newCommand;
				commands.Add(commandName, command);
			}
			return (TCommand) command;
		}

		/// <summary>
		/// Gets the commnd if one exists, or assigns calls the specified create command function.
		/// </summary>
		/// 
		/// <param name="createCommand">The function to create the new command if one does not exist.</param>
		/// <param name="commandName">The name of the command.</param>
		/// <returns>The command from the command list.</returns>
		protected TCommand GetCommand<TCommand>(Func<TCommand> createCommand,
			[CallerMemberName] string commandName = null)
			where TCommand : ICommand
		{
			if (!commands.TryGetValue(commandName, out ICommand command)) {
				command = createCommand();
				commands.Add(commandName, command);
			}
			return (TCommand) command;
		}

		#endregion

		#region GetCommand (RelayCommand)

		/// <summary>Gets or creates a new command with the specified parameters.</summary>
		/// 
		/// <param name="execute">The execute method for the command.</param>
		/// <param name="canExecute">The optional canExecute method for the command.</param>
		/// <param name="commandName">The name of the command to get or set.</param>
		/// <returns>The existing or created command.</returns>
		protected RelayCommand GetCommand(Action execute, Func<bool> canExecute = null,
			[CallerMemberName] string commandName = null)
		{
			return GetCommand(() => new RelayCommand(execute, canExecute), commandName);
		}

		#endregion

		#region GetCommand (RelayCommand<T>)

		/// <summary>Gets or creates a new command with the specified parameters.</summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="execute">The execute method for the command.</param>
		/// <param name="canExecute">The optional canExecute method for the command.</param>
		/// <param name="commandName">The name of the command to get or set.</param>
		/// <returns>The existing or created command.</returns>
		protected RelayCommand<T> GetCommand<T>(Action<T> execute, Func<T, bool> canExecute = null,
			[CallerMemberName] string commandName = null)
		{
			return GetCommand(() => new RelayCommand<T>(execute, canExecute), commandName);
		}

		#endregion

		#region GetCommand (RelayUICommand)

		/// <summary>Gets or creates a new command with the specified parameters.</summary>
		/// 
		/// <param name="text">The display text for the command.</param>
		/// <param name="execute">The execute method for the command.</param>
		/// <param name="canExecute">The optional canExecute method for the command.</param>
		/// <param name="commandName">The name of the command to get or set.</param>
		/// <returns>The existing or created command.</returns>
		protected RelayUICommand GetCommand(string text, Action execute, Func<bool> canExecute = null,
			[CallerMemberName] string commandName = null)
		{
			return GetCommand(text, null, null, execute, canExecute, commandName);
		}

		/// <summary>Gets or creates a new command with the specified parameters.</summary>
		/// 
		/// <param name="text">The display text for the command.</param>
		/// <param name="icon">The display icon for the command.</param>
		/// <param name="execute">The execute method for the command.</param>
		/// <param name="canExecute">The optional canExecute method for the command.</param>
		/// <param name="commandName">The name of the command to get or set.</param>
		/// <returns>The existing or created command.</returns>
		protected RelayUICommand GetCommand(string text, ImageSource icon, Action execute,
			Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
		{
			return GetCommand(text, icon, null, execute, canExecute, commandName);
		}

		/// <summary>Gets or creates a new command with the specified parameters.</summary>
		/// 
		/// <param name="text">The display text for the command.</param>
		/// <param name="icon">The display icon for the command.</param>
		/// <param name="keyGesture">The keybind for the command.</param>
		/// <param name="execute">The execute method for the command.</param>
		/// <param name="canExecute">The optional canExecute method for the command.</param>
		/// <param name="commandName">The name of the command to get or set.</param>
		/// <returns>The existing or created command.</returns>
		protected RelayUICommand GetCommand(string text, KeyGesture keyGesture, Action execute,
			Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
		{
			return GetCommand(text, null, keyGesture, execute, canExecute, commandName);
		}

		/// <summary>Gets or creates a new command with the specified parameters.</summary>
		/// 
		/// <param name="text">The display text for the command.</param>
		/// <param name="icon">The display icon for the command.</param>
		/// <param name="keyGesture">The keybind for the command.</param>
		/// <param name="execute">The execute method for the command.</param>
		/// <param name="canExecute">The optional canExecute method for the command.</param>
		/// <param name="commandName">The name of the command to get or set.</param>
		/// <returns>The existing or created command.</returns>
		protected RelayUICommand GetCommand(string text, ImageSource icon, KeyGesture keyGesture,
			Action execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
		{
			return GetCommand(() => new RelayUICommand(text, icon, keyGesture, execute, canExecute), commandName);
		}

		#endregion

		#region GetCommand (RelayUICommand<T>)

		/// <summary>Gets or creates a new command with the specified parameters.</summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="text">The display text for the command.</param>
		/// <param name="execute">The execute method for the command.</param>
		/// <param name="canExecute">The optional canExecute method for the command.</param>
		/// <param name="commandName">The name of the command to get or set.</param>
		/// <returns>The existing or created command.</returns>
		protected RelayUICommand<T> GetCommand<T>(string text, Action<T> execute,
			Func<T, bool> canExecute = null, [CallerMemberName] string commandName = null)
		{
			return GetCommand(text, null, null, execute, canExecute, commandName);
		}

		/// <summary>Gets or creates a new command with the specified parameters.</summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="text">The display text for the command.</param>
		/// <param name="icon">The display icon for the command.</param>
		/// <param name="execute">The execute method for the command.</param>
		/// <param name="canExecute">The optional canExecute method for the command.</param>
		/// <param name="commandName">The name of the command to get or set.</param>
		/// <returns>The existing or created command.</returns>
		protected RelayUICommand<T> GetCommand<T>(string text, ImageSource icon, Action<T> execute,
			Func<T, bool> canExecute = null, [CallerMemberName] string commandName = null)
		{
			return GetCommand(text, icon, null, execute, canExecute, commandName);
		}

		/// <summary>Gets or creates a new command with the specified parameters.</summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="text">The display text for the command.</param>
		/// <param name="icon">The display icon for the command.</param>
		/// <param name="keyGesture">The keybind for the command.</param>
		/// <param name="execute">The execute method for the command.</param>
		/// <param name="canExecute">The optional canExecute method for the command.</param>
		/// <param name="commandName">The name of the command to get or set.</param>
		/// <returns>The existing or created command.</returns>
		protected RelayUICommand<T> GetCommand<T>(string text, KeyGesture keyGesture, Action<T> execute,
			Func<T, bool> canExecute = null, [CallerMemberName] string commandName = null)
		{
			return GetCommand(text, null, keyGesture, execute, canExecute, commandName);
		}

		/// <summary>Gets or creates a new command with the specified parameters.</summary>
		/// 
		/// <typeparam name="T">The parameter type of the command.</typeparam>
		/// <param name="text">The display text for the command.</param>
		/// <param name="icon">The display icon for the command.</param>
		/// <param name="keyGesture">The keybind for the command.</param>
		/// <param name="execute">The execute method for the command.</param>
		/// <param name="canExecute">The optional canExecute method for the command.</param>
		/// <param name="commandName">The name of the command to get or set.</param>
		/// <returns>The existing or created command.</returns>
		protected RelayUICommand<T> GetCommand<T>(string text, ImageSource icon, KeyGesture keyGesture,
			Action<T> execute, Func<T, bool> canExecute = null, [CallerMemberName] string commandName = null)
		{
			return GetCommand(() => new RelayUICommand<T>(text, icon, keyGesture, execute, canExecute), commandName);
		}

		#endregion
	}
}
