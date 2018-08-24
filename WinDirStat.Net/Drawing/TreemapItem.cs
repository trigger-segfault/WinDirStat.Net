using System.IO;
using System.Collections.Generic;
using WinDirStat.Net.Settings.Geometry;
using WinDirStat.Net.Model.Data.Nodes;

namespace WinDirStat.Net.Drawing {
	public class TreemapItem : ITreemapItem {
		private List<TreemapItem> children;
		private long size;
		private Rectangle2S rectangle;
		private string extension;
		private string name;
		private TreemapItem parent;
		private Rgb24Color color;
		private FileNodeType type;

		public TreemapItem(FileNodeBase file) : this(file, null) {
		}

		private TreemapItem(FileNodeBase file, TreemapItem parent) {
			this.parent = parent;
			int count = file.ChildCount;
			children = new List<TreemapItem>(count);
			for (int i = 0; i < count; i++)
			children.Add(new TreemapItem(file[i], this));
			children.Sort(CompareReverse);
			children.Sort(Compare);
			size = file.Size;
			extension = file.Extension + "";
			name = file.Name + "";
			type = file.Type;
			//color = file.Color;
		}

		public TreemapItem(TreemapItem file) : this(file, null) {
		}

		private TreemapItem(TreemapItem file, TreemapItem parent) {
			this.parent = parent;
			int count = file.ChildCount;
			children = new List<TreemapItem>(count);
			for (int i = 0; i < count; i++)
				children.Add(new TreemapItem(file[i], this));
			children.Sort(CompareReverse);
			children.Sort(Compare);
			size = file.Size;
			extension = file.Extension + "";
			name = file.name + "";
			type = file.type;
			color = file.Color;
		}

		public void AddChild(FileNodeBase file) {
			children.Add(new TreemapItem(file, this));
		}

		public void AddChild(TreemapItem item) {
			children.Add(item);
			item.parent = this;
		}

		/*public void Validate(WinDirDocument document) {
			size = 0;
			color = document.Extensions[extension].Color;
			for (int i = 0; i < children.Count; i++) {
				TreemapItem child = children[i];
				if (child.children.Count > 0)
					child.Validate(document);
				//else
				//	child.color = document.Extensions[extension].Color;
				size += child.size;
			}
			children.Sort(Compare);
		}*/

		private int Compare(TreemapItem a, TreemapItem b) {
			int diff = b.Size.CompareTo(a.Size);
			if (diff == 0)
				diff = string.Compare(b.name, a.name, true);
			return diff;
		}

		private int CompareReverse(TreemapItem b, TreemapItem a) {
			int diff = b.Size.CompareTo(a.Size);
			if (diff == 0)
				diff = string.Compare(b.name, a.name, true);
			return diff;
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
