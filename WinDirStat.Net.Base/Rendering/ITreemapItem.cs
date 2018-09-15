using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Structures;

namespace WinDirStat.Net.Rendering {
	/// <summary>An interface for items to use in treemap rendering.</summary>
	public interface ITreemapItem {
		/// <summary>Gets if this treemap item is a leaf that should be drawn.</summary>
		bool IsLeaf { get; }

		/// <summary>Gets the color of this treemap item leaf.</summary
		Rgb24Color Color { get; }

		/// <summary>Gets or sets the rectangular bounds of the treemap item in the treemap.</summary>
		Rectangle2I Rectangle { get; set; }

		/// <summary>
		/// Gets the file size of the treemap item.<para/>
		/// The size of all children combined must match this value.
		/// </summary>
		long Size { get; }

		/// <summary>Gets the number of children in this treemap item.</summary>
		int ChildCount { get; }
		/// <summary>Gets the child at the specified index in the treemap item.</summary>
		ITreemapItem this[int index] { get; }
	}
}
