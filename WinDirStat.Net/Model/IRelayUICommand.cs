using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace WinDirStat.Net.Model {
	public interface IRelayUICommand : ICommand {
		string Text { get; set; }
		ImageSource Icon { get; set; }
		KeyGesture InputGesture { get; }
	}
}
