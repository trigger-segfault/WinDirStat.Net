using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Int32Rect = System.Windows.Int32Rect;
using SystemColors = System.Drawing.SystemColors;
using WinDirStat.Net.Settings.Geometry;
using WinDirStat.Net.Settings;
using System.Threading;
using System.Drawing;

namespace WinDirStat.Net.Drawing {
	public partial class Treemap {

		private static readonly Treemap Default = new Treemap(null);
		
		internal Treemap(TreemapOptions? options) {
			this.options = options ?? TreemapOptions.Default;

			float lx = this.options.LightSourceX;
			float ly = this.options.LightSourceY;
			const float lz = 10f;

			float lenght = (float) Math.Sqrt(lx*lx + ly*ly + lz*lz);
			this.lx = lx / lenght;
			this.ly = ly / lenght;
			this.lz = lz / lenght;
		}

		private TreemapOptions options;
		private float lx;
		private float ly;
		private float lz;
		private Rectangle2I renderArea;

		public TreemapOptions Options {
			get => options;
			set {
				options = value;

				float lx = options.LightSourceX;
				float ly = options.LightSourceY;
				const float lz = 10f;

				float lenght = (float) Math.Sqrt(lx*lx + ly*ly + lz*lz);
				this.lx = lx / lenght;
				this.ly = ly / lenght;
				this.lz = lz / lenght;
			}
		}

		[Conditional("DEBUG")]
		private void RecurseCheckTree(ITreemapItem item) {
			if (item.IsLeaf) {
				Debug.Assert(item.ChildCount == 0);
			}
			else {
				// TODO: check that children are sorted by size.
				long sum = 0;
				for (int i = 0; i < item.ChildCount; i++) {
					ITreemapItem child = item[i];
					sum += child.Size;
					RecurseCheckTree(child);
				}
				Debug.Assert(sum == item.Size);
			}
		}

		public static void HighlightRectangles(WriteableBitmap bitmap, Bitmap gdiBitmap, Rectangle2I rc, Rgba32Color color, IEnumerable<Rectangle2I> items) {
			Default.HighlightRectanglesInternal(bitmap, gdiBitmap, rc, color, items);
		}

		private void HighlightRectanglesInternal(WriteableBitmap bitmap, Bitmap gdiBitmap, Rectangle2I rc, Rgba32Color color, IEnumerable<Rectangle2I> items) {
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			renderArea = rc;
			
			using (Graphics g = Graphics.FromImage(gdiBitmap))
			using (Brush brush = new SolidBrush((Color) color)) {
				g.Clear(Color.Transparent);

				foreach (Rectangle2I rect in items)
					HighlightRectangle(g, rect, brush);

				BitmapData data = gdiBitmap.LockBits((Rectangle) rc, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				try {
					if (Application.Current.Dispatcher.Thread == Thread.CurrentThread) {
						Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
						bitmap.WritePixels(int32Rect, data.Scan0, data.Stride * rc.Height, data.Stride);
					}
					else {
						Application.Current.Dispatcher.Invoke(() => {
							Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
							bitmap.WritePixels(int32Rect, data.Scan0, data.Stride * rc.Height, data.Stride);
						});
					}
				}
				finally {
					gdiBitmap.UnlockBits(data);
				}
			}

			// That bitmap in turn will be created from this array
			/*Rgba32Color[] bitmapBits = new Rgba32Color[rc.Width * rc.Height];


			Memset(bitmapBits, Rgba32Color.Transparent);

			fixed (Rgba32Color* pBitmapBits = bitmapBits) {

				foreach (Rectangle2I rect in items)
					HighlightRectangle(pBitmapBits, rect, color);

				IntPtr bitmapBitsPtr = (IntPtr) pBitmapBits;

				Application.Current.Dispatcher.Invoke(() => {
					Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
					bitmap.WritePixels(int32Rect, bitmapBitsPtr, rc.Width * rc.Height * 4, bitmap.BackBufferStride);
				});
			}*/
		}

		public static void HighlightRectangles(WriteableBitmap bitmap, Rectangle2I rc, Rgba32Color color, IEnumerable<Rectangle2I> items) {
			Default.HighlightRectanglesInternal(bitmap, rc, color, items);
		}

		private unsafe void HighlightRectanglesInternal(WriteableBitmap bitmap, Rectangle2I rc, Rgba32Color color, IEnumerable<Rectangle2I> items) {
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			renderArea = rc;

			using (Bitmap gdiBitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb))
			using (Graphics g = Graphics.FromImage(gdiBitmap))
			using (Brush brush = new SolidBrush((Color) color)) {
				g.Clear(Color.Transparent);

				foreach (Rectangle2I rect in items)
					HighlightRectangle(g, rect, brush);

				//BitmapData data = gdiBitmap.LockBits((Rectangle) rc, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				try {
					/*if (Application.Current.Dispatcher.Thread == Thread.CurrentThread) {
						Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
						bitmap.WritePixels(int32Rect, data.Scan0, data.Stride * rc.Height, data.Stride);
					}
					else {
						Application.Current.Dispatcher.Invoke(() => {
							Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
							bitmap.WritePixels(int32Rect, data.Scan0, data.Stride * rc.Height, data.Stride);
						});
					}*/
				}
				finally {
					//gdiBitmap.UnlockBits(data);
				}
			}

			// That bitmap in turn will be created from this array
			/*Rgba32Color[] bitmapBits = new Rgba32Color[rc.Width * rc.Height];


			Memset(bitmapBits, Rgba32Color.Transparent);

			fixed (Rgba32Color* pBitmapBits = bitmapBits) {

				foreach (Rectangle2I rect in items)
					HighlightRectangle(pBitmapBits, rect, color);

				IntPtr bitmapBitsPtr = (IntPtr) pBitmapBits;

				Application.Current.Dispatcher.Invoke(() => {
					Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
					bitmap.WritePixels(int32Rect, bitmapBitsPtr, rc.Width * rc.Height * 4, bitmap.BackBufferStride);
				});
			}*/
		}

		public static void HighlightAll(WriteableBitmap bitmap, Bitmap gdiBitmap, Rectangle2I rc, ITreemapItem root, Rgba32Color color, Predicate<ITreemapItem> match) {
			Default.HighlightAllInternal(bitmap, gdiBitmap, rc, root, color, match);
		}

		private void HighlightAllInternal(WriteableBitmap bitmap, Bitmap gdiBitmap, Rectangle2I rc, ITreemapItem root, Rgba32Color color, Predicate<ITreemapItem> match) {
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			renderArea = rc;

			using (Graphics g = Graphics.FromImage(gdiBitmap))
			using (Brush brush = new SolidBrush((Color) color)) {
				g.Clear(Color.Transparent);

				Stopwatch sw1 = Stopwatch.StartNew();
				LoopHighlightAll(g, root, brush, match);
				Console.WriteLine($"Took {sw1.ElapsedMilliseconds:n}ms to LoopHighlightAll");

				sw1.Restart();
				BitmapData data = gdiBitmap.LockBits((Rectangle) rc, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				Console.WriteLine($"Took {sw1.ElapsedMilliseconds:n}ms to LockBits");

				try {
					Stopwatch sw = Stopwatch.StartNew();
					if (Application.Current.Dispatcher.Thread == Thread.CurrentThread) {
						Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
						bitmap.WritePixels(int32Rect, data.Scan0, data.Stride * rc.Height, data.Stride);
					}
					else {
						Application.Current.Dispatcher.Invoke(() => {
							Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
							bitmap.WritePixels(int32Rect, data.Scan0, data.Stride * rc.Height, data.Stride);
						});
					}
					Console.WriteLine($"Took {sw.ElapsedMilliseconds:n}ms to WritePixels");
				}
				finally {
					Stopwatch sw = Stopwatch.StartNew();
					gdiBitmap.UnlockBits(data);
					Console.WriteLine($"Took {sw.ElapsedMilliseconds:n}ms to UnlockBits");
				}
			}

			// That bitmap in turn will be created from this array
			/*Rgba32Color[] bitmapBits = new Rgba32Color[rc.Width * rc.Height];


			Memset(bitmapBits, Rgba32Color.Transparent);

			if (root.IsLeaf && match(root))
				HighlightRectangle(bitmapBits, root.Rectangle, color);
			//RecurseHighlightAll(pBitmapBits, root, color, match);
			LoopHighlightAll(bitmapBits, root, color, match);

			fixed (Rgba32Color* pBitmapBits = bitmapBits) {

				if (root.IsLeaf && match(root))
					HighlightRectangle(pBitmapBits, root.Rectangle, color);
				//RecurseHighlightAll(pBitmapBits, root, color, match);
				LoopHighlightAll(pBitmapBits, root, color, match);

				IntPtr bitmapBitsPtr = (IntPtr) pBitmapBits;

				if (Application.Current.Dispatcher.Thread == Thread.CurrentThread) {
					Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
					bitmap.WritePixels(int32Rect, bitmapBitsPtr, rc.Width * rc.Height * 4, bitmap.BackBufferStride);
				}
				else {
					Application.Current.Dispatcher.Invoke(() => {
						Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
						bitmap.WritePixels(int32Rect, bitmapBitsPtr, rc.Width * rc.Height * 4, bitmap.BackBufferStride);
					});
				}
			}*/
		}

		public static void HighlightAll(WriteableBitmap bitmap, Rectangle2I rc, ITreemapItem root, Rgba32Color color, Predicate<ITreemapItem> match) {
			Default.HighlightAllInternal(bitmap, rc, root, color, match);
		}

		private unsafe void HighlightAllInternal(WriteableBitmap bitmap, Rectangle2I rc, ITreemapItem root, Rgba32Color color, Predicate<ITreemapItem> match) {
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			renderArea = rc;


			using (Bitmap gdiBitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb))
			using (Graphics g = Graphics.FromImage(gdiBitmap))
			using (Brush brush = new SolidBrush((Color) color)) {
				g.Clear(Color.Transparent);

				LoopHighlightAll(g, root, brush, match);

				BitmapData data = gdiBitmap.LockBits((Rectangle) rc, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				try {
					if (Application.Current.Dispatcher.Thread == Thread.CurrentThread) {
						Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
						bitmap.WritePixels(int32Rect, data.Scan0, data.Stride * rc.Height, data.Stride);
					}
					else {
						Application.Current.Dispatcher.Invoke(() => {
							Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
							bitmap.WritePixels(int32Rect, data.Scan0, data.Stride * rc.Height, data.Stride);
						});
					}
				}
				finally {
					gdiBitmap.UnlockBits(data);
				}
			}

			// That bitmap in turn will be created from this array
			/*Rgba32Color[] bitmapBits = new Rgba32Color[rc.Width * rc.Height];


			Memset(bitmapBits, Rgba32Color.Transparent);

			if (root.IsLeaf && match(root))
				HighlightRectangle(bitmapBits, root.Rectangle, color);
			//RecurseHighlightAll(pBitmapBits, root, color, match);
			LoopHighlightAll(bitmapBits, root, color, match);

			fixed (Rgba32Color* pBitmapBits = bitmapBits) {

				if (root.IsLeaf && match(root))
					HighlightRectangle(pBitmapBits, root.Rectangle, color);
				//RecurseHighlightAll(pBitmapBits, root, color, match);
				LoopHighlightAll(pBitmapBits, root, color, match);

				IntPtr bitmapBitsPtr = (IntPtr) pBitmapBits;

				if (Application.Current.Dispatcher.Thread == Thread.CurrentThread) {
					Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
					bitmap.WritePixels(int32Rect, bitmapBitsPtr, rc.Width * rc.Height * 4, bitmap.BackBufferStride);
				}
				else {
					Application.Current.Dispatcher.Invoke(() => {
						Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
						bitmap.WritePixels(int32Rect, bitmapBitsPtr, rc.Width * rc.Height * 4, bitmap.BackBufferStride);
					});
				}
			}*/
		}

		private void LoopHighlightAll(Graphics g, ITreemapItem root, Brush brush, Predicate<ITreemapItem> match) {
			Queue<ITreemapItem> parents = new Queue<ITreemapItem>();
			parents.Enqueue(root);

			while (parents.Count != 0) {
				ITreemapItem parent = parents.Dequeue();
				for (int i = 0; i < parent.ChildCount; i++) {
					ITreemapItem child = parent[i];
					if (child.Rectangle.Width > 0 && child.Rectangle.Height > 0) {
						if (child.IsLeaf) {
							if (match(child))
								HighlightRectangle(g, child.Rectangle, brush);
						}
						else {
							parents.Enqueue(child);
						}
					}
				}
			}
		}

		private unsafe void LoopHighlightAll(Rgba32Color* bitmap, ITreemapItem root, Rgba32Color color, Predicate<ITreemapItem> match) {
			Queue<ITreemapItem> parents = new Queue<ITreemapItem>();
			parents.Enqueue(root);

			while (parents.Count != 0) {
				ITreemapItem parent = parents.Dequeue();
				for (int i = 0; i < parent.ChildCount; i++) {
					ITreemapItem child = parent[i];
					if (child.Rectangle.Width > 0 && child.Rectangle.Height > 0) {
						if (child.IsLeaf) {
							if (match(child))
								HighlightRectangle(bitmap, child.Rectangle, color);
						}
						else {
							parents.Enqueue(child);
						}
					}
				}
			}
		}

		private unsafe void LoopHighlightAll(Rgba32Color[] bitmap, ITreemapItem root, Rgba32Color color, Predicate<ITreemapItem> match) {
			Queue<ITreemapItem> parents = new Queue<ITreemapItem>();
			parents.Enqueue(root);

			while (parents.Count != 0) {
				ITreemapItem parent = parents.Dequeue();
				for (int i = 0; i < parent.ChildCount; i++) {
					ITreemapItem child = parent[i];
					if (child.Rectangle.Width > 0 && child.Rectangle.Height > 0) {
						if (child.IsLeaf) {
							if (match(child))
								HighlightRectangle(bitmap, child.Rectangle, color);
						}
						else {
							parents.Enqueue(child);
						}
					}
				}
			}
		}

		private unsafe void RecurseHighlightAll(Rgba32Color* bitmap, ITreemapItem parent, Rgba32Color color, Predicate<ITreemapItem> match) {
			Queue<ITreemapItem> parents = new Queue<ITreemapItem>();
			parents.Enqueue(parent);
			
			for (int i = 0; i < parent.ChildCount; i++) {
				ITreemapItem child = parent[i];
				if (child.Rectangle.Width > 0 && child.Rectangle.Height > 0) {
					if (child.IsLeaf) {
						if (match(child))
							HighlightRectangle(bitmap, child.Rectangle, color);
					}
					else {
						RecurseHighlightAll(bitmap, child, color, match);
					}
				}
			}
		}

		private void HighlightRectangle(Graphics g, Rectangle2I rc, Brush brush) {
			if (rc.Width >= 7 && rc.Height >= 7) {
				g.FillRectangle(brush, (Rectangle) Rectangle2I.FromLTRB(rc.Left, rc.Top, rc.Right, rc.Top + 3));
				g.FillRectangle(brush, (Rectangle) Rectangle2I.FromLTRB(rc.Left, rc.Bottom - 3, rc.Right, rc.Bottom));
				g.FillRectangle(brush, (Rectangle) Rectangle2I.FromLTRB(rc.Left, rc.Top + 3, rc.Left + 3, rc.Bottom - 3));
				g.FillRectangle(brush, (Rectangle) Rectangle2I.FromLTRB(rc.Right - 3, rc.Top + 3, rc.Right, rc.Bottom - 3));
			}
			else if (rc.Width > 0 && rc.Height > 0) {
				g.FillRectangle(brush, (Rectangle) rc);
			}
		}

		private unsafe void HighlightRectangle(Rgba32Color* bitmap, Rectangle2I rc, Rgba32Color color) {
			if (rc.Width >= 7 && rc.Height >= 7) {
				FillRect(bitmap, Rectangle2I.FromLTRB(rc.Left, rc.Top, rc.Right, rc.Top + 3), color);
				FillRect(bitmap, Rectangle2I.FromLTRB(rc.Left, rc.Bottom - 3, rc.Right, rc.Bottom), color);
				FillRect(bitmap, Rectangle2I.FromLTRB(rc.Left, rc.Top + 3, rc.Left + 3, rc.Bottom - 3), color);
				FillRect(bitmap, Rectangle2I.FromLTRB(rc.Right - 3, rc.Top + 3, rc.Right, rc.Bottom - 3), color);
			}
			else if (rc.Width > 0 && rc.Height > 0) {
				FillRect(bitmap, rc, color);
			}
		}

		private unsafe void HighlightRectangle(Rgba32Color[] bitmap, Rectangle2I rc, Rgba32Color color) {
			if (rc.Width >= 7 && rc.Height >= 7) {
				FillRect(bitmap, Rectangle2I.FromLTRB(rc.Left, rc.Top, rc.Right, rc.Top + 3), color);
				FillRect(bitmap, Rectangle2I.FromLTRB(rc.Left, rc.Bottom - 3, rc.Right, rc.Bottom), color);
				FillRect(bitmap, Rectangle2I.FromLTRB(rc.Left, rc.Top + 3, rc.Left + 3, rc.Bottom - 3), color);
				FillRect(bitmap, Rectangle2I.FromLTRB(rc.Right - 3, rc.Top + 3, rc.Right, rc.Bottom - 3), color);
			}
			else if (rc.Width > 0 && rc.Height > 0) {
				FillRect(bitmap, rc, color);
			}
		}

		private unsafe void FillRect(Rgba32Color* bitmap, Rectangle2I rc, Rgba32Color color) {
			for (int iy = rc.Top; iy < rc.Bottom; iy++) {
				for (int ix = rc.Left; ix < rc.Right; ix++) {
					bitmap[ix + iy * renderArea.Width] = color;
				}
			}
		}

		private unsafe void FillRect(Rgba32Color[] bitmap, Rectangle2I rc, Rgba32Color color) {
			for (int iy = rc.Top; iy < rc.Bottom; iy++) {
				for (int ix = rc.Left; ix < rc.Right; ix++) {
					bitmap[ix + iy * renderArea.Width] = color;
				}
			}
		}

		public static void DrawColorPreview(WriteableBitmap bitmap, Rectangle2I rc, Rgb24Color color, TreemapOptions ? options = null) {
			Treemap treemap = new Treemap(options);
			treemap.DrawColorPreview(bitmap, rc, color);
		}

		private unsafe void DrawColorPreview(WriteableBitmap bitmap, Rectangle2I rc, Rgb24Color color) {
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			renderArea = rc;

			// That bitmap in turn will be created from this array
			Rgba32Color[] bitmapBits = new Rgba32Color[rc.Width * rc.Height];

			fixed (Rgba32Color* pBitmapBits = bitmapBits) {
				
				float[] surface = { 0f, 0f, 0f, 0f };

				AddRidge(rc, surface, options.Height * options.ScaleFactor);
				RenderRectangle(pBitmapBits, rc, surface, color);

				IntPtr bitmapBitsPtr = (IntPtr) pBitmapBits;

				Application.Current.Dispatcher.Invoke(() => {
					Int32Rect int32Rect = new Int32Rect(rc.X, rc.Y, rc.Width, rc.Height);
					bitmap.WritePixels(int32Rect, bitmapBitsPtr, rc.Width * rc.Height * 4, bitmap.BackBufferStride);
				});
			}
		}

		public static void DrawTreemap(WriteableBitmap bitmap, Rectangle2I rc, ITreemapItem root, TreemapOptions? options = null) {
			Treemap treemap = new Treemap(options);
			treemap.DrawTreemap(bitmap, rc, root);
		}

		private unsafe void DrawTreemap(WriteableBitmap bitmap, Rectangle2I rc, ITreemapItem root) {
			RecurseCheckTree(root);

			Rectangle2I fullRc = rc;
			
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			if (options.Grid) {
				/*using (Brush brush = new SolidBrush(options.GridColor))
					g.FillRectangle(brush, rc);*/
			}
			else {
				// We shrink the rectangle here, too.
				// If we didn't do this, the layout of the treemap would
				// change, when grid is switched on and off.
				/*using (Pen pen = new Pen(SystemColors.ButtonShadow, 1)) {
					g.DrawLine(pen, rc.Right - 1, rc.Top, rc.Right - 1, rc.Bottom);
					g.DrawLine(pen, rc.Left, rc.Bottom - 1, rc.Right, rc.Bottom - 1);
				}*/
			}

			rc.Width--;
			rc.Height--;

			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			renderArea = fullRc;
			
			// That bitmap in turn will be created from this array
			Rgba32Color[] bitmapBits = new Rgba32Color[fullRc.Width * fullRc.Height];
			if (root.Size == 0)
				Memset(bitmapBits, Rgba32Color.Black);
			else if (options.Grid)
				Memset(bitmapBits, options.GridColor);
			else
				Memset(bitmapBits, SystemColors.ButtonShadow);

			fixed (Rgba32Color* pBitmapBits = bitmapBits) {

				// Recursively draw the tree graph
				if (root.Size > 0) {
					float[] surface = { 0f, 0f, 0f, 0f };
					RecurseDrawGraph(pBitmapBits, root, rc, true, surface, options.Height, 0);
				}

				IntPtr bitmapBitsPtr = (IntPtr) pBitmapBits;

				Application.Current.Dispatcher.Invoke(() => {
					Int32Rect int32Rect = new Int32Rect(fullRc.X, fullRc.Y, fullRc.Width, fullRc.Height);
					bitmap.WritePixels(int32Rect, bitmapBitsPtr, fullRc.Width * fullRc.Height * 4, bitmap.BackBufferStride);
				});
			}
		}

		/// <summary>Populates the entire array with the specified value.</summary>
		/// <typeparam name="T">The element type of the array.</typeparam>
		/// <param name="array">The array to fill.</param>
		/// <param name="value">The value to populate the array with.</param>
		/// <returns>Returns the same array that was populated.</returns>
		/// 
		/// <remarks>
		/// Modified version of Memset created by TowerOfBricks: <a href=
		/// "https://stackoverflow.com/a/13806014/7517185">Source</a>
		/// </remarks>
		private static void Memset<T>(T[] array, T value) {
			Memset(array, value, 0, array.Length);
		}

		/// <summary>
		/// Populates the specified section of the array with the specified value.
		/// </summary>
		/// 
		/// <typeparam name="T">The element type of the array.</typeparam>
		/// <param name="array">The array to fill.</param>
		/// <param name="value">The value to populate the array with.</param>
		/// <param name="startIndex">The index to start populating the array at.</param>
		/// <param name="length">The number of elements to populate the array with.</param>
		/// <returns>Returns the same array that was populated.</returns>
		/// 
		/// <remarks>
		/// Modified version of Memset created by TowerOfBricks: <a href=
		/// "https://stackoverflow.com/a/13806014/7517185">Source</a>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> or <paramref name="length"/> is less than zero.
		/// </exception>
		private static void Memset<T>(T[] array, T value, int startIndex,
			int length) {
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length),
					$"{nameof(length)} '{length}' is less than zero!");

			int block = Math.Min(32, length);
			int index = startIndex;
			int end = startIndex + block;

			while (index < end)
				array[index++] = value;

			end = startIndex + length;
			while (index < end) {
				Array.Copy(array, startIndex, array, index, Math.Min(block, end - index));
				index += block;
				block *= 2;
			}
		}

		private unsafe void RecurseDrawGraph(Rgba32Color* bitmap, ITreemapItem item, Rectangle2I rc, bool asroot, float[] pSurface, float h, uint flags) {
			Debug.Assert(rc.Width >= 0);
			Debug.Assert(rc.Height >= 0);
			Debug.Assert(item.Size > 0);

			item.Rectangle = rc;

			int gridWidth = options.Grid ? 1 : 0;

			if (rc.Width <= gridWidth || rc.Height <= gridWidth)
				return;

			float[] surface = new float[] { 0f, 0f, 0f, 0f };
			if (IsCushionShading) {
				Array.Copy(pSurface, surface, pSurface.Length);

				if (!asroot)
					AddRidge(rc, surface, h);
			}

			if (item.IsLeaf) {
				RenderLeaf(bitmap, item, surface);
			}
			else {
				Debug.Assert(item.ChildCount > 0);
				Debug.Assert(item.Size > 0);

				DrawChildren(bitmap, item, surface, h, flags);
			}
		}

		private unsafe void DrawChildren(Rgba32Color* bitmap, ITreemapItem parent, float[] surface, float h, uint flags) {
			switch (options.Style) {
			case TreemapStyle.KDirStatStyle:
				KDirStat_DrawChildren(bitmap, parent, surface, h, flags);
				break;
			case TreemapStyle.SequoiaViewStyle:
				throw new NotImplementedException("SequoiaViewStyle");
				//SequoiaView_DrawwChildren(bitmap, parent, surface, h, flags);
				//break;
			case TreemapStyle.SimpleStyle:
				throw new NotImplementedException("SimpleStyle");
				//Simple_DrawwChildren(bitmap, parent, surface, h, flags);
				//break;
			}
		}

		private unsafe void KDirStat_DrawChildren(Rgba32Color* bitmap, ITreemapItem parent, float[] surface, float h, uint flags) {
			Debug.Assert(parent.ChildCount > 0);

			Rectangle2I rc = parent.Rectangle;
			List<float> rows = new List<float>();
			List<int> childrenPerRow = new List<int>();

			float[] childWidth = new float[parent.ChildCount];

			bool horizontalRows = KDirStat_ArrangeChildren(parent, childWidth, rows, childrenPerRow);

			int width = horizontalRows ? rc.Width : rc.Height;
			int height = horizontalRows ? rc.Height : rc.Width;
			Debug.Assert(width >= 0);
			Debug.Assert(height >= 0);

			int c = 0;
			float top = horizontalRows ? rc.Top : rc.Left;
			for (int row = 0; row < rows.Count; row++) {
				float fBottom = top + rows[row] * height;
				int bottom = (int) fBottom;
				if (row == rows.Count - 1) {
					bottom = horizontalRows ? rc.Bottom : rc.Right;
				}
				float left = horizontalRows ? rc.Left : rc.Top;
				for (int i = 0; i < childrenPerRow[row]; i++, c++) {
					ITreemapItem child = parent[c];
					Debug.Assert(childWidth[c] >= 0);
					float fRight = left + childWidth[c] * width;
					int right = (int) fRight;

					bool lastChild = (i == childrenPerRow[row] - 1 || childWidth[c + 1] == 0);

					if (lastChild)
						right = horizontalRows ? rc.Right : rc.Bottom;

					Rectangle2I rcChild;
					if (horizontalRows) {
						rcChild = Rectangle2I.FromLTRB((int) left, (int) top, right, bottom);
					}
					else {
						rcChild = Rectangle2I.FromLTRB((int) top, (int) left, bottom, right);
					}

#if DEBUG
					if (rcChild.Width > 0 && rcChild.Height > 0) {
						//Rectangle2I test;
						//test.IntersectRect(parent->TmiGetRectangle(), rcChild);
						//Debug.Assert(test == rcChild);
					}
#endif
					RecurseDrawGraph(bitmap, child, rcChild, false, surface, h * options.ScaleFactor, 0);

					if (lastChild) {
						i++; c++;

						if (i < childrenPerRow[row]) {
							parent[c].Rectangle = Rectangle2I.FromLTRB(-1, -1, -1, -1);
						}

						c += childrenPerRow[row] - i;
						break;
					}

					left = fRight;
				}
				top = fBottom;
			}
		}

		private bool KDirStat_ArrangeChildren(ITreemapItem parent, float[] childWidth, List<float> rows, List<int> childrenPerRow) {
			Debug.Assert(!parent.IsLeaf);
			Debug.Assert(parent.ChildCount > 0);

			if (parent.Size == 0) {
				rows.Add(1f);
				childrenPerRow.Add(parent.ChildCount);
				for (int i = 0; i < parent.ChildCount; i++) {
					childWidth[i] = 1f / parent.ChildCount;
				}
				return true;
			}

			bool horizontalRows = parent.Rectangle.Width >= parent.Rectangle.Height;

			float width = 1f;
			if (horizontalRows) {
				if (parent.Rectangle.Height > 0)
					width = (float) parent.Rectangle.Width / parent.Rectangle.Height;
			}
			else {
				if (parent.Rectangle.Width > 0)
					width = (float) parent.Rectangle.Height / parent.Rectangle.Width;
			}

			int nextChild = 0;
			while (nextChild < parent.ChildCount) {
				rows.Add(KDirStat_CalculateNextRow(parent, nextChild, width, out int childrenUsed, childWidth));
				childrenPerRow.Add(childrenUsed);
				nextChild += childrenUsed;
			}

			return horizontalRows;
		}

		private float KDirStat_CalculateNextRow(ITreemapItem parent, int nextChild, float width, out int childrenUsed, float[] childWidth) {
			int i = 0;
			const float minProportion = 0.4f;
			Debug.Assert(minProportion < 1f);

			Debug.Assert(nextChild < parent.ChildCount);
			Debug.Assert(width >= 1f);

			float mySize = (float) parent.Size;
			Debug.Assert(mySize > 0f);
			long sizeUsed = 0;
			float rowHeight = 0;

			for (i = nextChild; i < parent.ChildCount; i++) {
				long childSize = parent[i].Size;
				if (childSize == 0) {
					Debug.Assert(i > nextChild);
					break;
				}

				sizeUsed += childSize;
				float virtualRowHeight = sizeUsed / mySize;
				Debug.Assert(virtualRowHeight > 0f);
				Debug.Assert(virtualRowHeight <= 1f);

				// Rectangle2I(mySize)    = width * 1.0
				// Rectangle2I(childSize) = childWidth * virtualRowHeight
				// Rectangle2I(childSize) = childSize / mySize * width

				float childWidth_ = childSize / mySize * width / virtualRowHeight;

				if (childWidth_ / virtualRowHeight < minProportion) {
					Debug.Assert(i > nextChild); // because width >= 1 and _minProportion < 1.
					// For the first child we have:
					// childWidth / rowHeight
					// = childSize / mySize * width / rowHeight / rowHeight
					// = childSize * width / sizeUsed / sizeUsed * mySize
					// > childSize * mySize / sizeUsed / sizeUsed
					// > childSize * childSize / childSize / childSize
					// = 1 > _minProportion.
					break;
				}
				rowHeight = virtualRowHeight;
			}
			Debug.Assert(i > nextChild);

			while (i < parent.ChildCount && parent[i].Size == 0) {
				i++;
			}

			childrenUsed = i - nextChild;

			for (i = 0; i < childrenUsed; i++) {
				// Rectangle2I(1f * 1f) = mySize
				float rowSize = mySize * rowHeight;
				float childSize = (float) parent[nextChild + i].Size;
				float cw = childSize / rowSize;
				Debug.Assert(cw >= 0);
				childWidth[nextChild + i] = cw;
			}

			return rowHeight;
		}

		private bool IsCushionShading {
			get => options.AmbientLight < 1f && options.Height > 0f && options.ScaleFactor > 0f;
		}

		private unsafe void RenderLeaf(Rgba32Color* bitmap, ITreemapItem item, float[] surface) {
			Rectangle2I rc = item.Rectangle;

			if (options.Grid) {
				rc.Y++;
				rc.X++;
				rc.Width--;
				rc.Height--;
				if (rc.Width <= 0 || rc.Height <= 0)
					return;
			}

			RenderRectangle(bitmap, rc, surface, item.Color);
		}

		private unsafe void RenderRectangle(Rgba32Color* bitmap, Rectangle2I rc, float[] surface, Rgba32Color color) {
			float brightness = options.Brightness;

			//color = ColorSpace.SetBrightness(color, PaletteBrightness);
			//brightness *= 0.66f;

			if (IsCushionShading) {
				DrawCushion(bitmap, rc, surface, color, brightness);
			}
			else {
				DrawSolidRect(bitmap, rc, color, brightness);
			}
		}

		private unsafe void DrawSolidRect(Rgba32Color* bitmap, Rectangle2I rc, Rgba32Color color, float brightness) {
			float factor = brightness / WinDirSettings.PaletteBrightness;

			int red = (int) (color.R * factor);
			int green = (int) (color.G * factor);
			int blue = (int) (color.B * factor);
			color = new Rgba32Color(red, green, blue);

			ColorSpace.NormalizeColor(ref red, ref green, ref blue);

			for (int iy = rc.Top; iy < rc.Bottom; iy++) {
				for (int ix = rc.Left; ix < rc.Right; ix++) {
					bitmap[ix + iy * renderArea.Width] = color;
				}
			}
		}
		private unsafe void DrawCushion(Rgba32Color* bitmap, Rectangle2I rc, float[] surface, Rgba32Color color, float brightness) {
			float Ia = options.AmbientLight;

			float Is = 1f - Ia;

			float r = color.R;
			float g = color.G;
			float b = color.B;

			for (int iy = rc.Top; iy < rc.Bottom; iy++) {
				for (int ix = rc.Left; ix < rc.Right; ix++) {
					float nx = (float) -(2d * surface[0] * (ix + 0.5f) + surface[2]);
					float ny = (float) -(2d * surface[1] * (iy + 0.5f) + surface[3]);
					float cosa = Math.Min(1f, (float) (((double) nx*lx + ny*ly + lz) / Math.Sqrt((double) nx*nx + ny*ny + 1f)));

					float pixel = Math.Max(0f, Is * cosa);

					pixel += Ia;
					Debug.Assert(pixel <= 1f);

					// Now, pixel is the brightness of the pixel, 0...1.0.

					// Apply contrast.
					// Not implemented.
					// Costs performance and nearly the same effect can be
					// made width the m_options->ambientLight parameter.
					// pixel= pow(pixel, m_options->contrast);

					// Apply "brightness"
					pixel *= brightness / WinDirSettings.PaletteBrightness;

					int red = (int) (r * pixel);
					int green = (int) (g * pixel);
					int blue = (int) (b * pixel);

					ColorSpace.NormalizeColor(ref red, ref green, ref blue);

					bitmap[ix + iy * renderArea.Width] = new Rgba32Color(red, green, blue);
				}
			}
		}

		private void AddRidge(Rectangle2I rc, float[] surface, float h) {
			/*
			Unoptimized:

			if(rc.Width() > 0)
			{
				surface[2]+= 4 * h * (rc.right + rc.left) / (rc.right - rc.left);
				surface[0]-= 4 * h / (rc.right - rc.left);
			}

			if(rc.Height() > 0)
			{
				surface[3]+= 4 * h * (rc.bottom + rc.top) / (rc.bottom - rc.top);
				surface[1]-= 4 * h / (rc.bottom - rc.top);
			}
			*/

			// Optimized (gained 15 ms of 1030):

			int width = rc.Width;
			int height = rc.Height;

			Debug.Assert(width > 0 && height > 0);

			float h4 = 4 * h;

			float wf = h4 / width;
			surface[2] += wf * (rc.Right + rc.Left);
			surface[0] -= wf;

			float hf = h4 / height;
			surface[3] += hf * (rc.Bottom + rc.Top);
			surface[1] -= hf;
		}

		public static ITreemapItem FindItemAtPoint(ITreemapItem item, Point2I p) {
			if (item.IsLeaf) {
				return item;
			}
			else {
				for (int i = 0; i < item.ChildCount; i++) {
					ITreemapItem child = item[i];
					Rectangle2I rcChild = child.Rectangle;

					if (rcChild.Contains(p))
						return FindItemAtPoint(child, p);
				}
			}

			return null;
		}
	}
}
