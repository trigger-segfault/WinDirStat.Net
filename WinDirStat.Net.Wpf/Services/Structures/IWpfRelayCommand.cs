using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using WinDirStat.Net.ViewModel;

namespace WinDirStat.Net.Wpf.Services.Structures {
	public interface IWpfRelayCommand : IRelayUICommand {

		KeyGesture InputGesture { get; }

		ImageSource ImageSource { get; }

	}
}
