using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinDirStat.Net.Wpf.Commands {
	public class RelayInfoCommandBinding : InputBinding {

		public RelayInfoCommandBinding(IRelayInfoCommand command) : base(command, command.Info.InputGesture) {
			Gesture = command.Info.InputGesture;
			Command = command;
		}
	}
}
