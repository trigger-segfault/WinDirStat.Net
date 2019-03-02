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
using WinDirStat.Net.Utils;
//using Bitmap = System.Drawing.Bitmap;
//using Graphics = System.Drawing.Graphics;
using Brush = System.Drawing.Brush;
using Pen = System.Drawing.Pen;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Structures;
using WinDirStat.Net.Rendering;
using WinDirStat.Net.Services;
using WinDirStat.Net.Wpf.ViewModel;
using WinDirStat.Net.ViewModel;
using WinDirStat.Net.Wpf.Utils;

namespace WinDirStat.Net.Wpf.Controls {
	public class GraphViewHoverEventArgs : RoutedEventArgs {

		public FileItemBase Hover;

		public GraphViewHoverEventArgs(RoutedEvent routedEvent, FileItemBase hover)
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
			DependencyProperty.Register("Root", typeof(FileItemBase), typeof(GraphView),
				new FrameworkPropertyMetadata(null, OnRootChanged));

		private static void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				FileItemBase root = graphView.Root;
				if (root == null) {
					graphView.AbortRender();
				}
				
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

		public FileItemBase Root {
			get => (FileItemBase) GetValue(RootProperty);
			set => SetValue(RootProperty, value);
		}

		private static readonly DependencyPropertyKey HoverPropertyKey =
			DependencyProperty.RegisterReadOnly("Hover", typeof(FileItemBase), typeof(GraphView),
				new FrameworkPropertyMetadata(null, OnHoverChanged));

		public static readonly DependencyProperty HoverProperty =
			HoverPropertyKey.DependencyProperty;

		private static void OnHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GraphView graphView) {
				FileItemBase hover = e.NewValue as FileItemBase;
				graphView.HasHover = hover != null;
				graphView.RaiseEvent(new GraphViewHoverEventArgs(HoverChangedEvent, hover));
			}
		}

		public FileItemBase Hover {
			get => (FileItemBase) GetValue(HoverProperty);
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
			IsDimmed = IsRenderingTreemap || resizing || !IsEnabled;
		}

		private static readonly DependencyPropertyKey IsDimmedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsDimmed", typeof(bool), typeof(GraphView),
				new FrameworkPropertyMetadata(false, OnIsDimmedChanged));

		private static void OnIsDimmedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			GraphView graphView = (GraphView) d;

			if (graphView.IsDimmed)
				graphView.Hover = null;
			else if (graphView.IsMouseOver)
				graphView.UpdateHover();
		}

		public static readonly DependencyProperty IsDimmedProperty =
			IsDimmedPropertyKey.DependencyProperty;

		public bool IsDimmed {
			get => (bool) GetValue(IsDimmedProperty);
			private set => SetValue(IsDimmedPropertyKey, value);
		}

		private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			GraphView graphView = (GraphView) d;
			MainViewModel oldViewModel = e.OldValue as MainViewModel;
			MainViewModel newViewModel = e.NewValue as MainViewModel;

			if (oldViewModel != null)
				oldViewModel.Settings.PropertyChanged -= graphView.OnSettingsChanged;
			if (graphView.IsLoaded && newViewModel != null)
				newViewModel.Settings.PropertyChanged += graphView.OnSettingsChanged;
			graphView.ViewModel = newViewModel;
		}

		private void OnSettingsChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			case nameof(SettingsService.TreemapOptions):
			case nameof(SettingsService.FilePalette):
				RenderAsync();
				break;
			case nameof(SettingsService.HighlightColor):
				if (highlightMode != HighlightMode.None)
					RenderHighlight(treemapSize);
				break;
			}
		}
		
		private Point2I treemapSize;
		private WriteableBitmap treemap;
		private Point2I highlightSize;
		private WriteableBitmap highlight;
		private Bitmap highlightGdi;
		private WriteableBitmap disabledTreemap;
		private WriteableBitmap disabledHighlight;
		private readonly DispatcherTimer resizeTimer;
		private Thread renderThread;
		private FileItemBase root;
		private bool resizing;
		/// <summary>Check if the running render thread has finished rendering the treemap.</summary>
		private volatile bool treemapRendered;
		private volatile bool fullRender;
		//private TreemapOptions options;

		private readonly object renderLock = new object();
		private readonly object drawBitmapLock = new object();
		private FileItemBase[] selection;
		private string extension;
		private HighlightMode highlightMode;

		static GraphView() {
			DataContextProperty.AddOwner(typeof(GraphView),
				new FrameworkPropertyMetadata(OnDataContextChanged));
			IsEnabledProperty.AddOwner(typeof(GraphView),
				new FrameworkPropertyMetadata(OnIsEnabledChanged));
		}

		public GraphView() {
			if (!this.DesignerInitializeComponent("Controls"))
				InitializeComponent();

			resizeTimer = new DispatcherTimer(
				TimeSpan.FromMilliseconds(50),
				DispatcherPriority.Normal,
				OnResizeTimerTick,
				Dispatcher);
			resizeTimer.Stop();
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
			if (IsRendering) {
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
			if (this.selection != null && this.selection.Intersect(selection.Cast<FileItemBase>()).Count() == this.selection.Length)
				return;

			highlightMode = HighlightMode.Selection;
			this.selection = selection.Cast<FileItemBase>().ToArray();
			extension = null;
			if (IsRendering) {
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

		private void OnLoaded(object sender, RoutedEventArgs e) {
			resizeTimer.Start();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e) {
			resizeTimer.Stop();
			AbortRender();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
			resizing = true;
			IsDimmed = true;
			// Restart the timer by stopping it first
			resizeTimer.Stop();
			resizeTimer.Start();
		}

		private void OnResizeTimerTick(object sender, EventArgs e) {
			resizing = false;
			UpdateDimmed();
			resizeTimer.Stop();
			RenderAsync();
		}



		/// <summary>Gets if anything is in the process of being rendered.</summary>
		private bool IsRendering => renderThread?.IsAlive ?? false;

		/// <summary>Gets if the treemap is in the process of being rendered.</summary>
		private bool IsRenderingTreemap => renderThread?.IsAlive ?? false && fullRender;

		/// <summary>A link to the view model that can be accessed outside of the UI thread.</summary>
		private MainViewModel ViewModel { get; set; }
		private SettingsService Settings => ViewModel.Settings;
		private TreemapRenderer Treemap => ViewModel.Treemap;

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
					Name = "GraphView Render",
				};
				renderThread.Start();
				UpdateDimmed();
			}
		}

		private void RenderThread(Point2I size) {
			try {
				if (size.X != 0 && size.Y != 0) {
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
			Treemap.DrawTreemap(treemap, new Rectangle2I(size), root);
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
				Treemap.HighlightExtensions(highlight, new Rectangle2I(size), root, Settings.HighlightColor, extension);
			}
			else if (highlightMode == HighlightMode.Selection) {
				Treemap.HighlightItems(highlight, new Rectangle2I(size), Settings.HighlightColor, selection);
			}
			Trace.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render highlight");
			Trace.WriteLine("");
		}

		private void OnMouseMove(object sender, MouseEventArgs e) {
			UpdateHover(e.GetPosition(this).ToPoint2I());
		}

		private void OnMouseLeave(object sender, MouseEventArgs e) {
			Hover = null;
		}

		private void UpdateHover() {
			UpdateHover(Mouse.GetPosition(this).ToPoint2I());
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
			Hover = (FileItemBase) TreemapRenderer.FindItemAtPoint(root, point);
		}
	}
}
