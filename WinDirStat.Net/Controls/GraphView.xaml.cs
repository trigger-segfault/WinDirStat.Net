using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
				FileNode root = e.NewValue as FileNode;
				if (root == null) {
					graphView.AbortRender();
				}

				graphView.root = root;
				graphView.HighlightNone();
				if (root != null) {
					//lock (graphView.drawBitmapLock)
					graphView.Render();
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
					graphView.Render();
				}
			}
		}

		private void UpdateDimmed() {
			IsDimmed = IsActuallyRendering || resizing || ForceDimmed;
		}

		private static readonly DependencyProperty OptionsProperty =
			DependencyProperty.Register("Options", typeof(TreemapOptions), typeof(GraphView),
				new FrameworkPropertyMetadata(TreemapOptions.Default, OnOptionsChanged));

		private static void OnOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				graphView.options = graphView.Options;
				graphView.Render();
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
		private WriteableBitmap forceDimmedTreemap;
		private WriteableBitmap forceDimmedHighlight;
		private readonly DispatcherTimer resizeTimer;
		private Thread renderThread;
		private FileNode root;
		private bool resizing;
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
		}

		public void HighlightNone() {
			highlightMode = HighlightMode.None;
			extension = null;
			selection = null;
			imageHighlight.Source = null;
		}

		public void HighlightExtension(string extension) {
			highlightMode = HighlightMode.Extension;
			this.extension = extension;
			selection = null;
			if (!IsDimmed && Root != null) {
				RenderHighlight(treemapSize);
				imageHighlight.Source = highlight;
			}
			//Render(true);
		}

		public void HighlightSelection(IEnumerable selection) {
			highlightMode = HighlightMode.Selection;
			this.selection = selection.Cast<FileNode>().ToArray();
			extension = null;
			if (!IsDimmed && Root != null) {
				RenderHighlight(treemapSize);
				imageHighlight.Source = highlight;
			}
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
			Render();
		}

		

		private bool IsActuallyRendering {
			get => renderThread?.IsAlive ?? false;
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

		public void Render() {
			Render(false);
		}

		private void Render(bool justHighlight) {
			if (!ForceDimmed && Root != null) {
				AbortRender();
				IsDimmed = true;
				renderThread = new Thread(() => RenderThread(justHighlight, new Point2I((int) ActualWidth, (int) ActualHeight))) {
					Priority = RenderPriority,
				};
				renderThread.Start();
			}
		}

		private void RenderThread(bool justHighlight, Point2I size) {
			try {
				if (size.X != 0 && size.Y != 0) {
					Stopwatch sw = Stopwatch.StartNew();
					if (!justHighlight) {
						RenderTreemap(size);
						Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render treegraph");
					}
					sw.Restart();
					if (highlightMode != HighlightMode.None) {
						RenderHighlight(size);
						Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render highlight");
					}
				}
				Dispatcher.Invoke(() => {
					// Let the control know we're not rendering anymore
					renderThread = null;
					UpdateDimmed();
					imageTreemap.Source = treemap;
					if (highlightMode != HighlightMode.None)
						imageHighlight.Source = highlight;
				});
			}
			catch (ThreadAbortException) { }
			catch (Exception ex) {
				Dispatcher.Invoke(() => {
					Console.WriteLine(ex.ToString());
					renderThread = null;
					UpdateDimmed();
				});
			}
		}

		private void RenderTreemap(Point2I size) {
			if (treemap == null || treemapSize.X != size.X || treemapSize.Y != size.Y) {
				treemapSize = size;
				Application.Current.Dispatcher.Invoke(() => {
					treemap = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null);
				});
			}
			Treemap.DrawTreemap(treemap, new Rectangle2I(size), root, options);
		}
		private void RenderHighlight(Point2I size) {
			if (highlight == null || highlightSize.X != size.X || highlightSize.Y != size.Y) {
				highlightSize = size;
				Application.Current.Dispatcher.Invoke(() => {
					highlight = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null);
				});
			}
			if (highlightMode == HighlightMode.Extension) {
				Treemap.HighlightAll(highlight, new Rectangle2I(size), root, Rgba32Color.White, MatchExtension);
			}
			else if (highlightMode == HighlightMode.Selection) {
				var rectangles = selection.Select(f => ((ITreemapItem) f).Rectangle);
				Treemap.HighlightRectangles(highlight, new Rectangle2I(size), Rgba32Color.White, rectangles);
			}
		}

		private bool MatchExtension(ITreemapItem item) {
			FileNode file = (FileNode) item;
			return (file.Type == FileNodeType.File && file.Extension == extension);
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
	}
}
