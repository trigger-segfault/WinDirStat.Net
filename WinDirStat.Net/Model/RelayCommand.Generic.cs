using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinDirStat.Net.Model {
	public class RelayCommand<TArgs> : ObservableObject, ICommand {

		#region Fields

		private readonly Action<TArgs> execute;
		private readonly Predicate<TArgs> canExecute;

		#endregion

		#region Constructors

		public RelayCommand(Action<TArgs> execute) : this(execute, null) {
		}

		public RelayCommand(Action<TArgs> execute, Predicate<TArgs> canExecute) {
			this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.canExecute = canExecute;
		}

		#endregion

		#region ICommand Members

		public void Execute(object parameter) {
			execute((TArgs) parameter);
		}

		public bool CanExecute(object parameter) {
			return canExecute?.Invoke((TArgs) parameter) ?? true;
		}

		public event EventHandler CanExecuteChanged {
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		#endregion
	}
}
