using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data {
	public enum FileSortMethod {
		None,
		Name,
		Subtree,
		Percent,
		Size,
		Items,
		Files,
		Subdirs,
		LastChange,
		LastAccess,
		Creation,
		Attributes,
		[Browsable(false)]
		Count,
	}

	public enum ExtensionSortMethod {
		None,
		Extension,
		Color,
		Description,
		Bytes,
		Percent,
		Files,
		[Browsable(false)]
		Count,
	}
}
