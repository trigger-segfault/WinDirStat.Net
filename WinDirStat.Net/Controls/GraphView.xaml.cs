using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using WinDirStat.Net.Drawing;
using WinDirStat.Net.Settings;
using WinDirStat.Net.Settings.Geometry;
using WinDirStat.Net.Utils;
//using Bitmap = System.Drawing.Bitmap;
//using Graphics = System.Drawing.Graphics;
using Brush = System.Drawing.Brush;
using Pen = System.Drawing.Pen;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using WinDirStat.Net.Model.Data.Nodes;
using WinDirStat.Net.Model.View;
using WinDirStat.Net.Model.Settings;

namespace WinDirStat.Net.Controls {
	public class GraphViewHoverEventArgs : RoutedEventArgs {

		public FileNodeBase Hover;

		public GraphViewHoverEventArgs(RoutedEvent routedEvent, FileNodeBase hover)
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
			DependencyProperty.Register("Root", typeof(FileNodeBase), typeof(GraphView),
				new FrameworkPropertyMetadata(null, OnRootChanged));

		private static void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				FileNodeBase root = graphView.Root;
				if (root == null) {
					graphView.AbortRender();
				}

				graphView.fileRoot = null;

				graphView.root = root;
				graphView.HighlightNone();
				graphView.Hover = null;
				if (root != null) {
					//lock (graphView.drawBitmapLock)
					graphView.RenderAsync();
				}
				else {
					graphView.Clear();
				}

			}
		}

		public FileNodeBase Root {
			get => (FileNodeBase) GetValue(RootProperty);
			set => SetValue(RootProperty, value);
		}

		private static readonly DependencyPropertyKey HoverPropertyKey =
			DependencyProperty.RegisterReadOnly("Hover", typeof(FileNodeBase), typeof(GraphView),
				new FrameworkPropertyMetadata(null, OnHoverChanged));

		public static readonly DependencyProperty HoverProperty =
			HoverPropertyKey.DependencyProperty;

		private static void OnHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				FileNodeBase hover = e.NewValue as FileNodeBase;
				graphView.HasHover = hover != null;
				graphView.RaiseEvent(new GraphViewHoverEventArgs(HoverChangedEvent, hover));
			}
		}

		public FileNodeBase Hover {
			get => (FileNodeBase) GetValue(HoverProperty);
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

		/*private static readonly DependencyProperty ForceDimmedProperty =
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
					graphView.disabledTreemap = graphView.treemap;
					graphView.disabledHighlight = graphView.highlight;
					graphView.imageTreemap.Source = graphView.disabledTreemap;
					graphView.imageHighlight.Source = graphView.disabledHighlight;
				}
				else {
					graphView.disabledTreemap = null;
					// GraphView will render if (!ForceDimmed and Root != null)
					graphView.RenderAsync();
				}
			}
		}*/

		private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				graphView.UpdateDimmed();
				if (!graphView.IsEnabled) {
					graphView.AbortRender();
					graphView.disabledTreemap = graphView.treemap;
					graphView.disabledHighlight = graphView.highlight;
					graphView.imageTreemap.Source = graphView.disabledTreemap;
					graphView.imageHighlight.Source = graphView.disabledHighlight;
				}
				else {
					graphView.disabledTreemap = null;
					graphView.disabledHighlight = null;
					// GraphView will render if (!IsEnabled and Root != null)
					graphView.RenderAsync();
				}
			}
		}

		private void UpdateDimmed() {
			IsDimmed = IsActuallyRenderingFull || resizing || !IsEnabled;
		}

		/*private static readonly DependencyProperty OptionsProperty =
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
		}*/

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

		private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				WinDirStatViewModel oldViewModel = e.OldValue as WinDirStatViewModel;
				WinDirStatViewModel newViewModel = e.NewValue as WinDirStatViewModel;
				if (oldViewModel != null)
					oldViewModel.Settings.PropertyChanged -= graphView.OnSettingsChanged;
				if (newViewModel != null)
					newViewModel.Settings.PropertyChanged += graphView.OnSettingsChanged;
				graphView.viewModel = newViewModel;
			}
		}

		private void OnSettingsChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			case nameof(WinDirStatSettings.TreemapOptions):
			case nameof(WinDirStatSettings.FilePalette):
				RenderAsync();
				break;
			case nameof(WinDirStatSettings.HighlightColor):
				if (highlightMode != HighlightMode.None)
					RenderHighlight(treemapSize);
				break;
			}
		}

		public static readonly WinDirStatSettings DefaultSettings = new WinDirStatSettings(null);

		private Point2I treemapSize;
		private WriteableBitmap treemap;
		private Point2I highlightSize;
		private WriteableBitmap highlight;
		private Bitmap highlightGdi;
		private WriteableBitmap disabledTreemap;
		private WriteableBitmap disabledHighlight;
		private readonly DispatcherTimer resizeTimer;
		private Thread renderThread;
		private FileNodeBase root;
		private FileNodeBase fileRoot;
		private bool resizing;
		/// <summary>Check if the running render thread has finished rendering the treemap.</summary>
		private volatile bool treemapRendered;
		private volatile bool fullRender;
		//private TreemapOptions options;
		private WinDirStatViewModel viewModel;

		private readonly object renderLock = new object();
		private readonly object drawBitmapLock = new object();
		private FileNodeBase[] selection;
		private string extension;
		private HighlightMode highlightMode;

		static GraphView() {
			DataContextProperty.AddOwner(typeof(GraphView),
				new FrameworkPropertyMetadata(OnDataContextChanged));
			IsEnabledProperty.AddOwner(typeof(GraphView),
				new FrameworkPropertyMetadata(OnIsEnabledChanged));
		}

		public GraphView() {
			InitializeComponent();
			resizeTimer = new DispatcherTimer(
				TimeSpan.FromMilliseconds(50),
				DispatcherPriority.Normal,
				OnResizeTimerTick,
				Dispatcher);
			treemapSize = Point2I.Zero;
			highlightSize = Point2I.Zero;
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
			if (this.selection != null && this.selection.Intersect(selection.Cast<FileNodeBase>()).Count() == this.selection.Length)
				return;

			highlightMode = HighlightMode.Selection;
			this.selection = selection.Cast<FileNodeBase>().ToArray();
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
		private WinDirStatSettings Settings {
			get => viewModel?.Settings ?? DefaultSettings;
		}
		private TreemapOptions Options {
			get => Settings.TreemapOptions;
		}
		private Rgba32Color HighlightColor {
			get => Settings.HighlightColor;
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
			if (IsEnabled && Root != null) {
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
					if (fileRoot == null) {
						//RootNode fileRoot = new RootNode((RootNode) root);
						//fileRoot.FinalValidate(fileRoot);
						//this.fileRoot = fileRoot;
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
					UpdateHover();
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
			Treemap.DrawTreemap(treemap, new Rectangle2I(size), root, Options);
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
			sw.Restart();
			if (highlightMode == HighlightMode.Extension) {
				//HighlightAll(highlight, highlightGdi, new Rectangle2I(size), fileRoot);
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
			Trace.WriteLine("");
		}

		private bool MatchExtension(ITreemapItem item) {
#if PREVIEW_GRAPH_VIEW
			//return (((PreviewTreemapItem) item).Extension == extension);
			return ((item as PreviewTreemapItem).Extension == extension);
#else
			//return (((FileNode) item).Extension == extension);
			return ((item as FileNodeBase).Extension == extension);
#endif
		}
		private bool MatchExtensionFile(ITreemapItem item) {
			//return (((FileNode) item).Extension == extension);
			return ((item as FileNodeBase).Extension == extension);
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
			if (root == null) {
				Hover = null;
				return;
			}
			if (Hover != null) {
				Rectangle2I rc = ((ITreemapItem) Hover).Rectangle;
				if (rc.Contains(point))
					return; // Hover is the same
			}
			Hover = (FileNodeBase) Treemap.FindItemAtPoint(root, point);
		}

		private void HighlightAll(WriteableBitmap bitmap, Bitmap gdiBitmap, Rectangle2I rc, FileNodeBase root) {
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			using (Graphics g = Graphics.FromImage(gdiBitmap))
			using (Brush brush = new SolidBrush((Color) HighlightColor))
			using (Pen pen = new Pen(brush)) {
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
				//using (Brush brush = new SolidBrush(C)) {
				g.Clear(Color.Transparent);

				RecurseHighlightAll(g, brush, pen, root);

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

		private void RecurseHighlightAll(Graphics g, Brush brush, Pen pen, FileNodeBase parent) {
			List<FileNodeBase> children = parent.VirtualChildren;
			int count = children.Count;
			for (int i = 0; i < count; i++) {
				FileNodeBase child = children[i];
				Rectangle2S rc = child.Rectangle;
				if (rc.Width > 0 && rc.Height > 0) {
					if (child.IsLeaf) {
						if (child.Extension == extension)
							HighlightRectangle(g, brush, pen, (Rectangle) rc);
					}
					else {
						RecurseHighlightAll(g, brush, pen, child);
					}
				}
			}
		}

		private void HighlightAll(WriteableBitmap bitmap, Bitmap gdiBitmap, Rectangle2I rc, TreemapItem root) {
			if (rc.Width <= 0 || rc.Height <= 0)
				return;

			using (Graphics g = Graphics.FromImage(gdiBitmap))
			using (Brush brush = new SolidBrush((Color) HighlightColor))
			using (Pen pen = new Pen(brush)) {
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
				//using (Brush brush = new SolidBrush(C)) {
				g.Clear(Color.Transparent);

				RecurseHighlightAll(g, brush, pen, root);

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

		private void RecurseHighlightAll(Graphics g, Brush brush, Pen pen, TreemapItem parent) {
			int count = parent.ChildCount;
			for (int i = 0; i < count; i++) {
				TreemapItem child = parent[i];
				Rectangle2S rc = child.Rectangle;
				if (rc.Width > 0 && rc.Height > 0) {
					if (child.IsLeaf) {
						if (child.Extension == extension)
							HighlightRectangle(g, brush, pen, (Rectangle) rc);
					}
					else {
						RecurseHighlightAll(g, brush, pen, child);
					}
				}
			}
		}

		private void HighlightRectangle(Graphics g, Brush brush, Pen pen, Rectangle rc) {
			//Brush brush = System.Drawing.Brushes.White;
			if (rc.Width >= 7 && rc.Height >= 7) {
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
				g.FillRectangle(brush, rc);
				//g.FillRectangle(brush, rc);
			}
		}
	}
}
