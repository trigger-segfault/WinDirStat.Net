using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.ViewModel;

namespace WinDirStat.Net.Wpf.Services.Structures {
	public class WpfRelayUICommand : WpfRelayCommand, IWpfRelayCommand {

		#region Fields

		public string Text { get; }
		public WpfImage Icon { get; }
		public WpfShortcut Shortcut { get; }

		#endregion

		#region Constructors
		
		public WpfRelayUICommand(IRelayUICommandInfo info, Action execute, bool keepTargetAlive = false)
			: this(info, execute, null, keepTargetAlive)
		{
		}
		public WpfRelayUICommand(IRelayUICommandInfo info, Action execute, Func<bool> canExecute,
			bool keepTargetAlive = false)
			: base(execute, canExecute, keepTargetAlive)
		{
			Text = info.Text;
			Icon = (WpfImage) info.Icon;
			Shortcut = (WpfShortcut) info.Shortcut;
		}

		#endregion

		#region Properties

		public KeyGesture InputGesture => Shortcut.Shortcut;
		public ImageSource ImageSource => Icon.Source;

		IImage IRelayUICommand.Icon => Icon;
		IShortcut IRelayUICommand.Shortcut => Shortcut;

		#endregion
	}
}
