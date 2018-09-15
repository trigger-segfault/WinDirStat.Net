using System.Collections.Generic;
using System.Linq;
using WinDirStat.Net.Services;
using WinDirStat.Net.Structures;

namespace WinDirStat.Net.Rendering {
	/// <summary>An treemap item for previewing purposes only.</summary>
	public class PreviewTreemapItem : ITreemapItem {

		#region Static Fields

		/// <summary>Gets the default preview treemap.</summary>
		public static PreviewTreemapItem DefaultTreemap { get; } = Build(SettingsService.DefaultFilePalette);

		#endregion

		#region Fields

		/// <summary>The list of children in the treemap item.</summary>
		private readonly List<PreviewTreemapItem> children;
		/// <summary>Gets the color of this treemap item leaf.</summary
		public Rgb24Color Color { get; }
		/// <summary>Gets or sets the rectangular bounds of the treemap item in the treemap.</summary>
		private Rectangle2S rectangle;
		/// <summary>
		/// Gets the file size of the treemap item.<para/>
		/// The size of all children combined must match this value.
		/// </summary>
		public long Size { get; private set; }

		#endregion

		#region Constructors

		/// <summary>Constructs an empty <see cref="PreviewTreemapItem"/>.</summary>
		public PreviewTreemapItem() {
			children = new List<PreviewTreemapItem>();
			Size = 0;
		}

		/// <summary>Constructs a <see cref="PreviewTreemapItem"/> leaf.</summary>
		/// 
		/// <param name="size">The size of the treemap item.</param>
		/// <param name="color">The color of the treemap item.</param>
		public PreviewTreemapItem(long size, Rgb24Color color) {
			Size = size;
			Color = color;
		}

		/// <summary>Constructs a <see cref="PreviewTreemapItem"/> with the specified children.</summary>
		/// 
		/// <param name="children">The children to populate the treemap item with.</param>
		public PreviewTreemapItem(IEnumerable<PreviewTreemapItem> children) {
			this.children = children.ToList();
			Size = children.Sum(c => c.Size);
		}

		#endregion

		#region Sort

		/// <summary>Validates the treemap item.</summary>
		public void Validate() {
			children.Sort((a, b) => b.Size.CompareTo(a.Size));
			int count = ChildCount;
			for (int i = 0; i < count; i++) {
				PreviewTreemapItem child = children[i];
				if (child.ChildCount != 0)
					child.Validate();
			}
		}

		#endregion

		#region Add

		/// <summary>Adds a <see cref="PreviewTreemapItem"/> leaf to the children.</summary>
		/// 
		/// <param name="size">The size of the child treemap item.</param>
		/// <param name="color">The color of the child treemap item.</param>
		public void Add(long size, Rgb24Color color) {
			children.Add(new PreviewTreemapItem(size, color));
		}

		/// <summary>Adds a <see cref="PreviewTreemapItem"/> to the children.</summary>
		/// 
		/// <param name="child">The child treemap item to add.</param>
		public void Add(PreviewTreemapItem child) {
			children.Add(child);
		}

		/// <summary>Adds a collection of <see cref="PreviewTreemapItem"/>s to the children.</summary>
		/// 
		/// <param name="children">The children to add to the treemap item.</param>
		public void Add(IEnumerable<PreviewTreemapItem> children) {
			this.children.AddRange(children);
		}

		#endregion

		#region Properties

		/// <summary>Gets if this treemap item is a leaf that should be drawn.</summary>
		public bool IsLeaf => children == null;

		/// <summary>Gets or sets the rectangular bounds of the treemap item in the treemap.</summary>
		public Rectangle2I Rectangle {
			get => rectangle;
			set => rectangle = (Rectangle2S) value;
		}

		/// <summary>Gets the number of children in this treemap item.</summary>
		public int ChildCount => (children != null ? children.Count : 0);

		/// <summary>Gets the child at the specified index in the treemap item.</summary>
		public PreviewTreemapItem this[int index] => children[index];

		/// <summary>Gets the child at the specified index in the treemap item.</summary>
		ITreemapItem ITreemapItem.this[int index] => children[index];

		#endregion

		#region Build Preview

		/// <summary>Gets the next color in the palette.</summary>
		/// 
		/// <param name="index">The index to increment.</param>
		/// <param name="palette">The palette to get the color from.</param>
		/// <returns>A color from the palette.</returns>
		private static Rgb24Color NextColor(ref int index, IReadOnlyList<Rgb24Color> palette) {
			return palette[(index + 1 < palette.Count ? index++ : index)];
		}

		/// <summary>Builds a preview treemap for display purposes.</summary>
		/// 
		/// <param name="palette">The palette to use.</param>
		/// <returns>The root item of the preview treemap.</returns>
		public static PreviewTreemapItem Build(IReadOnlyList<Rgb24Color> palette) {
			int col = 0;
			Rgb24Color color;

			PreviewTreemapItem c4 = new PreviewTreemapItem();
			color = NextColor(ref col, palette);
			for (int i = 0; i < 30; i++)
				c4.Add(1 + 100 * i, color);

			PreviewTreemapItem c0 = new PreviewTreemapItem();
			for (int i = 0; i < 8; i++)
				c0.Add(500 + 600 * i, NextColor(ref col, palette));

			PreviewTreemapItem c1 = new PreviewTreemapItem();
			color = NextColor(ref col, palette);
			for (int i = 0; i < 10; i++)
				c1.Add(1 + 200 * i, color);
			c0.Add(c1);

			PreviewTreemapItem c2 = new PreviewTreemapItem();
			color = NextColor(ref col, palette);
			for (int i = 0; i < 160; i++)
				c2.Add(1 + i, color);

			PreviewTreemapItem c3 = new PreviewTreemapItem();
			c3.Add(10000, NextColor(ref col, palette));
			c3.Add(c4);
			c3.Add(c2);
			c3.Add(6000, NextColor(ref col, palette));
			c3.Add(1500, NextColor(ref col, palette));

			PreviewTreemapItem root = new PreviewTreemapItem();
			root.Add(c0);
			root.Add(c3);

			root.Validate();
			return root;
		}

		#endregion
	}
}
