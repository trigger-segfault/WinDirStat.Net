using System.IO;
using System.Collections.Generic;
using WinDirStat.Net.Data.Nodes;
using WinDirStat.Net.Settings.Geometry;

namespace WinDirStat.Net.Drawing {
	public class TreemapItem : ITreemapItem {
		private List<TreemapItem> children;
		private readonly long size;
		private Rectangle2S rectangle;
		private readonly string extension;
		private readonly TreemapItem parent;
		private Rgb24Color color;

		public TreemapItem(FileNode file) : this(file, null) {
		}

		private TreemapItem(FileNode file, TreemapItem parent) {
			this.parent = parent;
			int count = file.ChildCount;
			children = new List<TreemapItem>();
			for (int i = 0; i < count; i++)
				children.Add(new TreemapItem(file[i]));
			size = file.Size;
			extension = file.Extension + "";
			color = file.Color;
		}

		public string Extension => extension;

		public bool IsLeaf => children.Count == 0;

		public Rectangle2S Rectangle {
			get => rectangle;
			set => rectangle = value;
		}

		public long Size => size;

		public int ChildCount => children.Count;

		public TreemapItem this[int index] => children[index];

		ITreemapItem ITreemapItem.this[int index] => children[index];


		//private static readonly TreemapItem[] Empty = new TreemapItem[0];
		/*public TreemapItem(FileNode file) {
			int count = file.ChildCount;
			if (count > 0) {
				children = new TreemapItem[count];
				for (int i = 0; i < count; i++)
					children[i] = new TreemapItem(file[i]);
			}
			else {
				children = Empty;
			}
			size = file.Size;
			color = file.Color;
			extension = file.Extension;
		}*/
		//public Rgb24Color Color => Rgb24Color.White;
		public Rgb24Color Color => color;
		/*public string Extension {
			get => extension;
		}

		public bool IsLeaf {
			get => children.Length == 0;
		}

		public Rectangle2S Rectangle {
			get => rectangle;
			set => rectangle = value;
		}

		public Rgb24Color Color {
			get => color;
		}

		public long Size {
			get => size;
		}

		public int ChildCount {
			get => children.Length;
		}

		public TreemapItem this[int index] {
			get => children[index];
		}

		/*public FileNode File {
			get => file;
		}*/

		Rectangle2I ITreemapItem.Rectangle {
			get => rectangle;
			set => rectangle = value;
		}

		/*ITreemapItem ITreemapItem.this[int index] {
			get => children[index];
		}*/
	}
}
