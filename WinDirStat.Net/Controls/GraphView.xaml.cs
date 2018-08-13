using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WinDirStat.Net.Data.Nodes;
using WinDirStat.Net.Drawing;
using WinDirStat.Net.Settings;
using WinDirStat.Net.Settings.Geometry;
using WinDirStat.Net.Utils;
//using Bitmap = System.Drawing.Bitmap;
//using Graphics = System.Drawing.Graphics;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace WinDirStat.Net.Controls {
	public class GraphViewHoverEventArgs : RoutedEventArgs {

		public FileNode Hover;

		public GraphViewHoverEventArgs(RoutedEvent routedEvent, FileNode hover)
			: base(routedEvent)
		{
			Hover = hover;
		}
	}

	public delegate void GraphViewHoverEventHandler(object sender, GraphViewHoverEventArgs e);


	/// <summary>
	/// Interaction logic for WpfGraphView.xaml
	/// </summary>
	public partial class GraphView : UserControl {

		public enum HighlightMode {
			None,
			Extension,
			Selection,
		}

		public static readonly DependencyProperty RootProperty =
			DependencyProperty.Register("Root", typeof(FileNode), typeof(GraphView),
				new FrameworkPropertyMetadata(null, OnRootChanged));

		private static void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				FileNode root = graphView.Root;
				if (root == null) {
					graphView.AbortRender();
				}
				graphView.itemRoot = null;
				graphView.fileRoot = null;

				graphView.root = root;
				graphView.HighlightNone();
				if (root != null) {
					//lock (graphView.drawBitmapLock)
					graphView.RenderAsync();
				}
				else {
					graphView.Clear();
				}

			}
		}

		public FileNode Root {
			get => (FileNode) GetValue(RootProperty);
			set => SetValue(RootProperty, value);
		}

		public static readonly DependencyProperty FileRootProperty =
			DependencyProperty.Register("FileRoot", typeof(RootNode), typeof(GraphView),
				new FrameworkPropertyMetadata(null, OnFileRootChanged));

		private static void OnFileRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				

			}
		}

		public RootNode FileRoot {
			get => (RootNode) GetValue(FileRootProperty);
			set => SetValue(FileRootProperty, value);
		}

		private static readonly DependencyPropertyKey HoverPropertyKey =
			DependencyProperty.RegisterReadOnly("Hover", typeof(FileNode), typeof(GraphView),
				new FrameworkPropertyMetadata(null, OnHoverChanged));

		public static readonly DependencyProperty HoverProperty =
			HoverPropertyKey.DependencyProperty;

		private static void OnHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				FileNode hover = e.NewValue as FileNode;
				graphView.HasHover = hover != null;
				graphView.RaiseEvent(new GraphViewHoverEventArgs(HoverChangedEvent, hover));
			}
		}

		public FileNode Hover {
			get => (FileNode) GetValue(HoverProperty);
			private set => SetValue(HoverPropertyKey, value);
		}

		private static readonly DependencyPropertyKey HasHoverPropertyKey =
			DependencyProperty.RegisterReadOnly("HasHover", typeof(bool), typeof(GraphView),
				new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty HasHoverProperty =
			HasHoverPropertyKey.DependencyProperty;

		public bool HasHover {
			get => (bool) GetValue(HasHoverProperty);
			private set => SetValue(HasHoverPropertyKey, value);
		}

		public static readonly RoutedEvent HoverChangedEvent =
			EventManager.RegisterRoutedEvent("HoverChanged", RoutingStrategy.Bubble,
				typeof(GraphViewHoverEventHandler), typeof(GraphView));

		public event GraphViewHoverEventHandler HoverChanged {
			add => AddHandler(HoverChangedEvent, value);
			remove => RemoveHandler(HoverChangedEvent, value);
		}

		private static readonly DependencyProperty RenderPriorityProperty =
			DependencyProperty.Register("RenderPriority", typeof(ThreadPriority), typeof(GraphView),
				new FrameworkPropertyMetadata(ThreadPriority.BelowNormal));

		public ThreadPriority RenderPriority {
			get => (ThreadPriority) GetValue(RenderPriorityProperty);
			set => SetValue(RenderPriorityProperty, value);
		}

		private static readonly DependencyProperty ForceDimmedProperty =
			DependencyProperty.Register("ForceDimmed", typeof(bool), typeof(GraphView),
				new FrameworkPropertyMetadata(false, OnForceDimmedChanged));

		public bool ForceDimmed {
			get => (bool) GetValue(ForceDimmedProperty);
			set => SetValue(ForceDimmedProperty, value);
		}

		private static void OnForceDimmedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				graphView.UpdateDimmed();
				if (graphView.ForceDimmed) {
					graphView.AbortRender();
					graphView.forceDimmedTreemap = graphView.treemap;
					graphView.forceDimmedHighlight = graphView.highlight;
					graphView.imageTreemap.Source = graphView.forceDimmedTreemap;
					graphView.imageHighlight.Source = graphView.forceDimmedHighlight;
				}
				else {
					graphView.forceDimmedTreemap = null;
					// GraphView will render if (!ForceDimmed and Root != null)
					graphView.RenderAsync();
				}
			}
		}

		private void UpdateDimmed() {
			IsDimmed = IsActuallyRenderingFull || resizing || ForceDimmed;
		}

		private static readonly DependencyProperty OptionsProperty =
			DependencyProperty.Register("Options", typeof(TreemapOptions), typeof(GraphView),
				new FrameworkPropertyMetadata(TreemapOptions.Default, OnOptionsChanged));

		private static void OnOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				graphView.options = graphView.Options;
				graphView.RenderAsync();
			}
		}

		public TreemapOptions Options {
			get => (TreemapOptions) GetValue(OptionsProperty);
			set => SetValue(OptionsProperty, value);
		}

		/*private static readonly DependencyPropertyKey IsRenderingPropertyKey =
			DependencyProperty.RegisterReadOnly("IsRendering", typeof(bool), typeof(WpfGraphView),
				new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsRenderingProperty =
			IsRenderingPropertyKey.DependencyProperty;

		public bool IsRendering {
			get => (bool) GetValue(IsRenderingProperty);
			private set => SetValue(IsRenderingPropertyKey, value);
		}*/

		private static readonly DependencyPropertyKey IsDimmedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsDimmed", typeof(bool), typeof(GraphView),
				new FrameworkPropertyMetadata(false, OnIsDimmedChanged));

		private static void OnIsDimmedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				bool isDimmed = (bool) e.NewValue;
				if (isDimmed)
					graphView.Hover = null;
				else if (graphView.IsMouseOver)
					graphView.UpdateHover();
			}
		}

		public static readonly DependencyProperty IsDimmedProperty =
			IsDimmedPropertyKey.DependencyProperty;

		public bool IsDimmed {
			get => (bool) GetValue(IsDimmedProperty);
			private set => SetValue(IsDimmedPropertyKey, value);
		}

		private Point2I treemapSize;
		private WriteableBitmap treemap;
		private Point2I highlightSize;
		private WriteableBitmap highlight;
		private Bitmap highlightGdi;
		private WriteableBitmap forceDimmedTreemap;
		private WriteableBitmap forceDimmedHighlight;
		private readonly DispatcherTimer resizeTimer;
		private Thread renderThread;
		private FileNode root;
		private FileNode fileRoot;
		private TreemapItem itemRoot;
		private bool resizing;
		/// <summary>Check if the running render thread has finished rendering the treemap.</summary>
		private volatile bool treemapRendered;
		private volatile bool fullRender;
		private TreemapOptions options;

		private readonly object renderLock = new object();
		private readonly object drawBitmapLock = new object();
		private FileNode[] selection;
		private string extension;
		private HighlightMode highlightMode;

		public GraphView() {
			InitializeComponent();
			resizeTimer = new DispatcherTimer(
				TimeSpan.FromMilliseconds(50),
				DispatcherPriority.Normal,
				OnResizeTimerTick,
				Dispatcher);
			treemapSize = Point2I.Zero;
			highlightSize = Point2I.Zero;
			options = Options;
			highlightMode = HighlightMode.None;
			treemapRendered = true;
		}

		public void HighlightNone() {
			highlightMode = HighlightMode.None;
			extension = null;
			selection = null;
			imageHighlight.Source = null;
		}

		public void HighlightExtension(string extension) {
			if (this.extension == extension)
				return;

			highlightMode = HighlightMode.Extension;
			this.extension = extension;
			selection = null;
			if (IsActuallyRendering) {
				RenderHighlightAsync();
			}
			else if (!IsDimmed && Root != null) {
				RenderHighlight(treemapSize);
				if (imageHighlight.Source != highlight)
					imageHighlight.Source = highlight;
			}
			//RenderHighlightAsync(ThreadPriority.Normal);
			//Render(true);
		}

		public void HighlightSelection(IEnumerable selection) {
			if (this.selection != null && this.selection.Intersect(selection.Cast<FileNode>()).Count() == this.selection.Length)
				return;

			highlightMode = HighlightMode.Selection;
			this.selection = selection.Cast<FileNode>().ToArray();
			extension = null;
			if (IsActuallyRendering) {
				RenderHighlightAsync();
			}
			else if (!IsDimmed && Root != null) {
				RenderHighlight(treemapSize);
				if (imageHighlight.Source != highlight)
					imageHighlight.Source = highlight;
			}
			//RenderHighlightAsync(ThreadPriority.Normal);
			//Render(true);
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
			resizing = true;
			IsDimmed = true;
			// Reset the timer by stopping it first
			resizeTimer.Stop();
			resizeTimer.Start();
		}

		private void OnResizeTimerTick(object sender, EventArgs e) {
			resizing = false;
			UpdateDimmed();
			resizeTimer.Stop();
			RenderAsync();
		}



		private bool IsActuallyRendering {
			get => renderThread?.IsAlive ?? false;
		}
		private bool IsActuallyRenderingFull {
			get => renderThread?.IsAlive ?? false && fullRender;
		}

		private void Clear() {
			AbortRender();
			treemap = null;
			highlight = null;
			treemapSize = Point2I.Zero;
			highlightSize = Point2I.Zero;
		}

		public void AbortRender(bool waitForExit = true) {
			if (renderThread != null) {
				renderThread.Abort();
				renderThread = null;
				UpdateDimmed();
			}
		}

		public void RenderHighlightAsync(ThreadPriority? priority = null) {
			fullRender = !treemapRendered;
			RenderAsyncFinal(priority);
		}

		public void RenderAsync(ThreadPriority? priority = null) {
			fullRender = true;
			RenderAsyncFinal(priority);
		}

		private void RenderAsyncFinal(ThreadPriority? priority = null) {
			if (!ForceDimmed && Root != null) {
				AbortRender();
				treemapRendered = false;
				//IsDimmed = true;
				Point2I size = new Point2I((int) ActualWidth, (int) ActualHeight);
				renderThread = new Thread(() => RenderThread(size)) {
					Priority = priority ?? RenderPriority,
				};
				renderThread.Start();
				UpdateDimmed();
			}
		}

		private void RenderThread(Point2I size) {
			try {
				if (size.X != 0 && size.Y != 0) {
					if (itemRoot == null) {
						TreemapItem itemRoot = new TreemapItem(root);
						RootNode fileRoot = new RootNode((RootNode) root);
						this.itemRoot = itemRoot;
						this.fileRoot = fileRoot;
						Dispatcher.Invoke(() => {
							FileRoot = fileRoot;
						});
					}
					if (fullRender) {
						RenderTreemap(size);
					}
					treemapRendered = true;
					if (highlightMode != HighlightMode.None) {
						RenderHighlight(size);
					}
				}
				Dispatcher.Invoke(() => {
					// Let the control know we're not rendering anymore
					renderThread = null;
					imageTreemap.Source = treemap;
					if (highlightMode != HighlightMode.None)
						imageHighlight.Source = highlight;
					UpdateDimmed();
				});
			}
			catch (ThreadAbortException) { }
			catch (Exception ex) {
				Stopwatch sw = Stopwatch.StartNew();
				Dispatcher.Invoke(() => {
					Console.WriteLine(ex.ToString());
					renderThread = null;
					UpdateDimmed();
				});
				Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to invoke EXCEPTION Dispatcher");
			}
		}

		private void RenderTreemap(Point2I size) {
			Stopwatch sw = Stopwatch.StartNew();
			if (treemap == null || treemapSize.X != size.X || treemapSize.Y != size.Y) {
				treemapSize = size;
				Application.Current.Dispatcher.Invoke(() => {
					treemap = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null);
				});
			}
			Treemap.DrawTreemap(treemap, new Rectangle2I(size), root, options);
			Treemap.DrawTreemap(treemap, new Rectangle2I(size), itemRoot, options);
			Treemap.DrawTreemap(treemap, new Rectangle2I(size), fileRoot, options);
			//Treemap.DrawTreemap(treemap, new Rectangle2I(size), fileRoot, options);
			Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render treemap");
		}
		private void RenderHighlight(Point2I size) {
			Stopwatch sw = Stopwatch.StartNew();
			if (highlight == null || highlightSize.X != size.X || highlightSize.Y != size.Y) {
				highlightSize = size;
				Application.Current.Dispatcher.Invoke(() => {
					highlight = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null);
				});
				highlightGdi?.Dispose();
				highlightGdi = new Bitmap(size.X, size.Y, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				Trace.WriteLine($"Took {sw.ElapsedMilliseconds}ms to setup highlight bitmap");
			}
			if (highlightMode == HighlightMode.Extension) {
				HighlightAll(highlight, highlightGdi, new Rectangle2I(size), fileRoot);
			}
			Trace.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render file highlight");
			sw.Restart();
			if (highlightMode == HighlightMode.Extension) {
				HighlightAll(highlight, highlightGdi, new Rectangle2I(size), root);
				//Treemap.HighlightAll(highlight, highlightGdi, new Rectangle2I(size), root, Rgba32Color.White, MatchExtension);
			}
			else if (highlightMode == HighlightMode.Selection) {
				var rectangles = selection.Select(f => (Rectangle2I) f.Rectangle);
				Treemap.HighlightRectangles(highlight, highlightGdi, new Rectangle2I(size), Rgba32Color.White, rectangles);
			}
			Trace.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render ui highlight");
			sw.Restart();
			if (highlightMode == HighlightMode.Extension) {
				HighlightAll(highlight, highlightGdi, new Rectangle2I(size), itemRoot);
			}
			Trace.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render item highlight");
			Trace.WriteLine("");
			//sw.Restart();
			//if (highlightMode == HighlightMode.Extension) {
			//Treemap.HighlightAll(highlight, highlightGdi, new Rectangle2I(size), fileRoot, Rgba32Color.White, MatchExtensionFile);
			//}
			//Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render file highlight");
		}

		private bool MatchExtension(ITreemapItem item) {
#if PREVIEW_GRAPH_VIEW
			//return (((PreviewTreemapItem) item).Extension == extension);
			return ((item as PreviewTreemapItem).Extension == extension);
#else
			//return (((FileNode) item).Extension == extension);
			return ((item as FileNode).Extension == extension);
#endif
		}
		private bool MatchExtensionFile(ITreemapItem item) {
			//return (((FileNode) item).Extension == extension);
			return ((item as FileNode).Extension == extension);
		}

		private void OnMouseMove(object sender, MouseEventArgs e) {
			UpdateHover((Point2I) e.GetPosition(this));
		}

		private void OnMouseLeave(object sender, MouseEventArgs e) {
			Hover = null;
		}

		private void UpdateHover() {
			UpdateHover((Point2I) Mouse.GetPosition(this));
		}

		private void UpdateHover(Point2I point) {
			if (Hover != null) {
				Rectangle2I rc = ((ITreemapItem) Hover).Rectangle;
				if (rc.Contains(point))
					return; // Hover is the same
			}
			Hover = (FileNode) Treemap.FindItemAtPoint(root, point);
		}

		private void HighlightAll(WriteableBitmap bitmap, Bitmap gdiBitmap, Rectangle2I rc, FileNode root) {
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			using (Graphics g = Graphics.FromImage(gdiBitmap)) {
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
				//using (Brush brush = new SolidBrush(C)) {
				g.Clear(Color.Transparent);

				RecurseHighlightAll(g, root);

				BitmapData data = gdiBitmap.LockBits((Rectangle) rc, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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
		}

		private void RecurseHighlightAll(Graphics g, FileNode parent) {
			int count = parent.ChildCount;
			//List<FileNode> chilren = parent.virtualChildren;
			for (int i = 0; i < count; i++) {
				FileNode child = parent[i];
				Rectangle2S rc = child.Rectangle;
				if (rc.Width > 0 && rc.Height > 0) {
					if (child.IsLeaf) {
						if (child.Extension == extension)
							HighlightRectangle(g, (Rectangle) rc);
					}
					else {
						RecurseHighlightAll(g, child);
					}
				}
			}
		}

		private void HighlightAll(WriteableBitmap bitmap, Bitmap gdiBitmap, Rectangle2I rc, TreemapItem root) {
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			using (Graphics g = Graphics.FromImage(gdiBitmap)) {
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
				//using (Brush brush = new SolidBrush(C)) {
				g.Clear(Color.Transparent);

				RecurseHighlightAll(g, root);

				BitmapData data = gdiBitmap.LockBits((Rectangle) rc, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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
		}

		private void RecurseHighlightAll(Graphics g, TreemapItem parent) {
			int count = parent.ChildCount;
			for (int i = 0; i < count; i++) {
				TreemapItem child = parent[i];
				Rectangle2S rc = child.Rectangle;
				if (rc.Width > 0 && rc.Height > 0) {
					if (child.IsLeaf) {
						if (child.Extension == extension)
							HighlightRectangle(g, (Rectangle) rc);
					}
					else {
						RecurseHighlightAll(g, child);
					}
				}
			}
		}

		private void HighlightRectangle(Graphics g, Rectangle rc) {
			//Brush brush = System.Drawing.Brushes.White;
			if (rc.Width >= 7 && rc.Height >= 7) {
				System.Drawing.Pen pen = System.Drawing.Pens.White;
				g.DrawRectangle(pen, rc);
				rc.X++; rc.Y++; rc.Width -= 2; rc.Height -= 2;
				g.DrawRectangle(pen, rc);
				rc.X++; rc.Y++; rc.Width -= 2; rc.Height -= 2;
				g.DrawRectangle(pen, rc);
				/*g.FillRectangle(brush, Rectangle.FromLTRB(rc.Left, rc.Top, rc.Right, rc.Top + 3));
				g.FillRectangle(brush, Rectangle.FromLTRB(rc.Left, rc.Bottom - 3, rc.Right, rc.Bottom));
				g.FillRectangle(brush, Rectangle.FromLTRB(rc.Left, rc.Top + 3, rc.Left + 3, rc.Bottom - 3));
				g.FillRectangle(brush, Rectangle.FromLTRB(rc.Right - 3, rc.Top + 3, rc.Right, rc.Bottom - 3));*/
			}
			else if (rc.Width > 0 && rc.Height > 0) {
				g.FillRectangle(System.Drawing.Brushes.White, rc);
				//g.FillRectangle(brush, rc);
			}
		}
	}
}
