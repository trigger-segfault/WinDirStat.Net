using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Data.Extensions {
	public enum ExtensionRecordAction {
		/// <summary>Requests the view watching this extension to show itself.</summary>
		GetView,
	}

	public class ExtensionRecordEventArgs {
		public ExtensionRecordAction Action { get; }
		public object View { get; set; }

		public ExtensionRecordEventArgs(ExtensionRecordAction action) {
			Action = action;
		}
	}

	public delegate void ExtensionRecordEventHandler(ExtensionRecord sender, ExtensionRecordEventArgs e);
}
