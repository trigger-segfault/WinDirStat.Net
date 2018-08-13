using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Data;
using WinDirStat.Net.Data.Nodes;
using WinDirStat.Net.Settings.Geometry;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Drawing {
	public class PreviewTreemapItem : ITreemapItem {
		private List<PreviewTreemapItem> children;
		private long size;
		private Rgb24Color color;
		private Rectangle2S rectangle;

		public PreviewTreemapItem() {
			children = new List<PreviewTreemapItem>();
			size = 0;
		}

		public PreviewTreemapItem(long size, Rgb24Color color) {
			this.size = size;
			this.color = color;
		}

		public PreviewTreemapItem(IEnumerable<PreviewTreemapItem> children) {
			this.children = children.ToList();
			size = children.Sum(c => c.size);
			Sort(false);
		}

		public void Sort(bool recursive) {
			children.Sort((a, b) => b.size.CompareTo(a.size));
			if (recursive) {
				foreach (PreviewTreemapItem child in children) {
					child.Sort(true);
				}
			}
		}

		public void Add(long size, Color color) {
			children.Add(new PreviewTreemapItem(size, color));
		}

		public void Add(PreviewTreemapItem child) {
			children.Add(child);
		}

		public void Add(IEnumerable<PreviewTreemapItem> children) {
			this.children.AddRange(children);
		}

		public bool IsLeaf {
			get => children.Count == 0;
		}

		public Rectangle2I Rectangle {
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
			get => (children != null ? children.Count : 0);
		}

		public PreviewTreemapItem this[int index] {
			get => children[index];
		}

		ITreemapItem ITreemapItem.this[int index] {
			get => children[index];
		}

		private static Color NextColor(ref int index, Color[] palette) {
			return palette[(index + 1 < palette.Length ? index++ : index)];
		}

		public static PreviewTreemapItem Build(Color[] palette) {
			int col = 0;
			Color color;

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

			root.Sort(true);
			return root;
		}
	}
}
