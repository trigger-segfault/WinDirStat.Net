using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Rendering;
using WinDirStat.Net.Services;
using WinDirStat.Net.Structures;

namespace WinDirStat.Net.ViewModel.Graph {
	/// <summary>The view model for a file treemap display.</summary>
	public partial class FileGraphViewModel : ViewModelBaseEx {

		#region Private Enums

		/// <summary>The modes for highlighting the graph view.</summary>
		protected enum HighlightMode {
			None,
			Extension,
			Selection,
		}

		#endregion

		#region Fields

		public SettingsService Settings { get; }
		public TreemapRenderer Treemap { get; }
		public UIService UI { get; }


		private Point2I treemapSize;
		private WriteableBitmap treemap;
		private Point2I highlightSize;
		private WriteableBitmap highlight;
		private WriteableBitmap disabledTreemap;
		private WriteableBitmap disabledHighlight;

		/// <summary>True if the graph is currently dimmed.</summary>
		private volatile bool isDimmed;
		/// <summary>True if the running render thread has finished rendering the treemap.</summary>
		private volatile bool treemapRendered;
		/// <summary>True if the running render thread is rendering the treemap.</summary>
		private volatile bool fullRender;

		/// <summary>The root treemap item.</summary>
		private FileItemBase rootItem;
		/// <summary>The hovered treemap item.</summary>
		private FileItemBase hoverItem;

		/// <summary>The selected file items to highlight.</summary>
		private FileItemBase[] selection;
		/// <summary>The extension to highlight.</summary>
		private string extension;
		/// <summary>The current mode for highlighting.</summary>
		private HighlightMode highlightMode;

		/// <summary>True if rendering should be done on a separate thread.</summary>
		private bool renderAsynchronously;
		
		/// <summary>True if resizing is in progress.</summary>
		private bool resizing;
		/// <summary>The timer to delay rendering while resizing.</summary>
		private readonly UITimer resizeTimer;

		/// <summary>The thread where the asynchronous rendering is done.</summary>
		private Thread renderThread;

		/// <summary>The current mouse position of inside the graph.</summary>
		private Point2I mousePosition;
		/// <summary>True if the mouse is inside the graph.</summary>
		private bool isMouseOver;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="FileGraphViewModel"/>.</summary>
		public FileGraphViewModel(SettingsService settings,
								  TreemapRendererFactory treemapFactory,
								  UIService ui)
		{
			Settings = settings;
			Treemap = treemapFactory.Create();
			UI = ui;

			Settings.PropertyChanged += OnSettingsPropertyChanged;

			resizeTimer = UI.CreateTimer(TimeSpan.FromSeconds(0.05), true, OnResizeTick);
		}

		#endregion

		#region Events Handlers
		
		private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			case nameof(SettingsService.TreemapOptions):
			case nameof(SettingsService.FilePalette):
				RenderAsync();
				break;
			case nameof(SettingsService.HighlightColor):
				if (highlightMode != HighlightMode.None)
					RenderHighlight(treemapSize);
				break;
			case nameof(SettingsService.RenderPriority):
				if (renderThread != null && renderThread.IsAlive)
					renderThread.Priority = Settings.RenderPriority;
				break;
			}
		}

		private void OnResizeTick() {

		}

		#endregion

		#region Private Properties

		/// <summary>Gets if anything is in the process of being rendered.</summary>
		private bool IsRendering => renderThread?.IsAlive ?? false;
		/// <summary>Gets if the treemap is in the process of being rendered.</summary>
		private bool IsRenderingTreemap => renderThread?.IsAlive ?? false && fullRender;

		/// <summary>Gets the treemap highlight color.</summary>
		private Rgba32Color Options => Settings.HighlightColor;

		#endregion

		#region Properties

		#endregion

		#region Rendering
		
		private void UpdateDimmed() {
			IsDimmed = IsRenderingTreemap || resizing || !IsEnabled;
		}

		/// <summary>Clears the current render.</summary>
		private void Clear() {
			AbortRender();
			treemap = null;
			highlight = null;
			treemapSize = Point2I.Zero;
			highlightSize = Point2I.Zero;
		}

		/// <summary>Aborts the current render in progress.</summary>
		public void AbortRender(bool waitForExit = true) {
			if (renderThread != null) {
				renderThread.Abort();
				renderThread = null;
				UpdateDimmed();
			}
		}

		private void RenderHighlightAsync() {
			fullRender = !treemapRendered;
			RenderAsyncImpl();
		}

		private void RenderAsync() {
			fullRender = true;
			RenderAsyncImpl();
		}

		private void RenderAsyncImpl() {
			if (IsEnabled && rootItem != null) {
				AbortRender();
				treemapRendered = false;
				//IsDimmed = true;
				Point2I size = new Point2I((int) ActualWidth, (int) ActualHeight);
				renderThread = new Thread(() => RenderThread(size)) {
					Priority = Settings.RenderPriority,
					Name = "File GraphView Render",
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
				UI.Invoke(() => {
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
				UI.Invoke(() => {
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
				UI.Invoke(() => {
					treemap = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null);
				});
			}
			Treemap.DrawTreemap(treemap, new Rectangle2I(size), rootItem);
			//Treemap.DrawTreemap(treemap, new Rectangle2I(size), fileRoot, options);
			Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render treemap");
		}
		private void RenderHighlight(Point2I size) {
			Stopwatch sw = Stopwatch.StartNew();
			if (highlight == null || highlightSize.X != size.X || highlightSize.Y != size.Y) {
				highlightSize = size;
				UI.Invoke(() => {
					highlight = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null);
				});
				Trace.WriteLine($"Took {sw.ElapsedMilliseconds}ms to setup highlight bitmap");
			}
			sw.Restart();
			if (highlightMode == HighlightMode.Extension) {
				Treemap.HighlightExtensions(highlight, new Rectangle2I(size), rootItem, Settings.HighlightColor, extension);
			}
			else if (highlightMode == HighlightMode.Selection) {
				Treemap.HighlightItems(highlight, new Rectangle2I(size), Settings.HighlightColor, selection);
			}
			Trace.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render highlight");
			Trace.WriteLine("");
		}

		#endregion
	}
}
