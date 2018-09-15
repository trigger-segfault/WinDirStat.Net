using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinDirStat.Net.Utils {
	/// <summary>Extensions for <see cref="ICommand"/>s.</summary>
	public static class CommandExtensions {

		/// <summary>Executes the method without a parameter.</summary>
		/// 
		/// <param name="command">The command to execute.</param>
		public static void Execute(this ICommand command) {
			command.Execute(null);
		}

	}
}
