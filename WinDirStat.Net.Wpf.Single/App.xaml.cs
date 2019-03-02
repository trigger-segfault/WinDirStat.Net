using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WinDirStat.Net.Wpf.Windows;

namespace WinDirStat.Net.Wpf {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		public App() {
			ErrorMessageBox.GlobalHook(this);
		}
	}
}
