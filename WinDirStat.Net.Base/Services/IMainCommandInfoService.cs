using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Services {
	public interface IMainCommandInfoService {

		#region File Menu

		IRelayUICommandInfo Open { get; }
		IRelayUICommandInfo Save { get; }
		IRelayUICommandInfo Reload { get; }
		IRelayUICommandInfo Close { get; }
		IRelayUICommandInfo Cancel { get; }
		IRelayUICommandInfo Elevate { get; }
		IRelayUICommandInfo Exit { get; }

		#endregion

		#region Context Menu/ToolBar

		IRelayUICommandInfo Expand { get; }
		IRelayUICommandInfo Collapse { get; }
		IRelayUICommandInfo OpenItem { get; }
		IRelayUICommandInfo CopyPath { get; }
		IRelayUICommandInfo Explore { get; }
		IRelayUICommandInfo CommandPrompt { get; }
		IRelayUICommandInfo PowerShell { get; }
		IRelayUICommandInfo RefreshSelected { get; }
		IRelayUICommandInfo DeleteRecycle { get; }
		IRelayUICommandInfo DeletePermanently { get; }
		IRelayUICommandInfo Properties { get; }

		#endregion

		#region Options Menu

		IRelayUICommandInfo ShowFreeSpace { get; }
		IRelayUICommandInfo ShowUnknown { get; }
		IRelayUICommandInfo ShowTotalSpace { get; }
		IRelayUICommandInfo ShowFileTypes { get; }
		IRelayUICommandInfo ShowTreemap { get; }
		IRelayUICommandInfo ShowToolBar { get; }
		IRelayUICommandInfo ShowStatusBar { get; }
		IRelayUICommandInfo Configure { get; }

		#endregion

		#region Other

		IRelayUICommandInfo EmptyRecycleBin { get; }

		#endregion
	}
}
