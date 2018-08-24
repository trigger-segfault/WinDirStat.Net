using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Settings.Geometry;

namespace WinDirStat.Net.Drawing {
	public interface ITreemapItem {
		bool IsLeaf { get; }

		Rectangle2I Rectangle { get; set; }
		Rgb24Color Color { get; }
		long Size { get; }

		int ChildCount { get; }
		ITreemapItem this[int index] { get; }
	}
}
