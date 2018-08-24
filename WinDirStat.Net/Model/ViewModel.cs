using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinDirStat.Net.Model {
	public class ViewModel : ObservableObject {
		private readonly Dictionary<string, ICommand> cmds = new Dictionary<string, ICommand>();

		protected ICommand GetCommand(ICommand newCommand, [CallerMemberName] string name = null) {
			if (!cmds.TryGetValue(name, out ICommand command)) {
				command = newCommand;
				cmds.Add(name, command);
			}
			return command;
		}
	}
}
