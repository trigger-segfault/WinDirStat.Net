using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data.Nodes {
	[Serializable]
	public enum FileNodeType : byte {
		Root,
		Directory,
		File,
		FileCollection,
		FreeSpace,
	}
}
