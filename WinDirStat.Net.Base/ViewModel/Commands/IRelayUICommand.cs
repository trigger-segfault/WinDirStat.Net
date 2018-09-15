using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace WinDirStat.Net.ViewModel {
	/// <summary>An interface for working with generic and non-generic RelayUICommands.</summary>
	public interface IRelayUICommand : ICommand, INotifyPropertyChanged {
		string Text { get; set; }
		ImageSource Icon { get; set; }
		KeyGesture InputGesture { get; }
	}
}
