using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinDirStat.Net.Model {
	public class RelayCommand : ObservableObject, ICommand {

		#region Fields

		private readonly Action<object> execute;
		private readonly Predicate<object> canExecute;

		#endregion

		#region Constructors

		public RelayCommand(Action execute) : this(execute, null) {
		}
		public RelayCommand(Action<object> execute) : this(execute, null) {
		}

		public RelayCommand(Action execute, Func<bool> canExecute) {
			if (execute == null)
				throw new ArgumentNullException(nameof(execute));
			this.execute = (o) => execute();
			this.canExecute = (o) => canExecute();
		}
		public RelayCommand(Action<object> execute, Predicate<object> canExecute) {
			this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.canExecute = canExecute;
		}

		#endregion

		#region ICommand Members

		public void Execute(object parameter) {
			execute(parameter);
		}

		public bool CanExecute(object parameter) {
			return canExecute?.Invoke(parameter) ?? true;
		}

		public event EventHandler CanExecuteChanged {
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		#endregion
	}
}
