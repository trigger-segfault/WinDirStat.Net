using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Rendering;
using WinDirStat.Net.Services;
using WinDirStat.Net.Structures;

namespace WinDirStat.Net.ViewModel.Graph {
	/// <summary>The base view model for a treemap display.</summary>
	public partial class GraphViewModel : ViewModelBase {

		#region Private Enums

		/// <summary>The modes for highlighting the graph view.</summary>
		protected enum HighlightMode {
			None,
			Extension,
			Selection,
		}

		#endregion

		#region Fields

		private readonly object volatileLock = new object();

		private readonly SettingsService settings;
		private readonly TreemapRenderer treemap;
		private readonly UIService ui;


		private Point2I treemapSize;
		private Point2I highlightSize;

		private WriteableBitmap treemapImage;
		private WriteableBitmap highlightImage;
		private WriteableBitmap disabledTreemapImage;
		private WriteableBitmap disabledHighlightImage;

		private ImageSource treemapDisplayImage;
		private ImageSource highlightDisplayImage;

		private Point2I dimensions;


		/// <summary>The root treemap item.</summary>
		private volatile ITreemapItem rootItem;

		// Hover
		/// <summary>The hovered treemap item.</summary>
		private ITreemapItem hoverItem;
		/// <summary>The current mouse position of inside the graph.</summary>
		private Point2I mousePosition;
		/// <summary>True if the mouse is inside the graph.</summary>
		private bool isMouseOver;

		// Highlight
		/// <summary>The selected treemap items to highlight.</summary>
		private ITreemapItem[] selection;
		/// <summary>The extension to highlight.</summary>
		private string extension;
		/// <summary>The current mode for highlighting.</summary>
		private HighlightMode highlightMode;

		// Display
		/// <summary>True if the graph is currently dimmed.</summary>
		private volatile bool isDimmed;
		/// <summary>True if the graph is enabled.</summary>
		private volatile bool isEnabled;

		// Rendering
		/// <summary>True if rendering should be done on a separate thread.</summary>
		private bool renderAsynchronously;
		/// <summary>True if the running render thread has finished rendering the treemap.</summary>
		private volatile bool treemapRendered;
		/// <summary>True if the running render thread is rendering the treemap.</summary>
		private volatile bool fullRender;
		/// <summary>True if resizing is in progress.</summary>
		private volatile bool resizing;
		/// <summary>The timer to delay rendering while resizing.</summary>
		private readonly UITimer resizeTimer;
		/// <summary>The thread where the asynchronous rendering is done.</summary>
		private Thread renderThread;

		private Rgb24Color highlightColor;
		
		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="GraphViewModel"/>.</summary>
		public GraphViewModel(SettingsService settings,
							  TreemapRendererFactory treemapFactory,
							  UIService ui)
		{
			this.settings = settings;
			treemap = treemapFactory.Create();
			this.ui = ui;

			this.settings.PropertyChanged += OnSettingsPropertyChanged;

			resizeTimer = this.ui.CreateTimer(TimeSpan.FromSeconds(0.05), true, OnResizeTick);
			isEnabled = true;
		}

		#endregion

		#region Events Handlers

		private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			/*case nameof(SettingsService.TreemapOptions):
			case nameof(SettingsService.FilePalette):
				Render();
				break;
			case nameof(SettingsService.HighlightColor):
				if (highlightMode != HighlightMode.None)
					RenderHighlight(treemapSize);
				break;*/
			case nameof(SettingsService.RenderPriority):
				ui.Invoke(() => {
					if (renderThread != null && renderThread.IsAlive)
						renderThread.Priority = settings.RenderPriority;
				});
				break;
			}
		}

		private void OnResizeTick() {

		}

		#endregion

		#region Private Properties

		/// <summary>Gets if anything is in the process of being rendered.</summary>
		private bool IsRenderingAsync => renderThread?.IsAlive ?? false;
		/// <summary>Gets if the treemap is in the process of being rendered.</summary>
		private bool IsRenderingTreemap => renderThread?.IsAlive ?? false && fullRender;

		/// <summary>Gets or sets the treemap highlight color.</summary>
		public Rgba32Color HighlightColor {
			get => highlightColor;
			set => SetProperty(ref highlightColor, value);
		}

		#endregion

		#region Properties

		/// <summary>Gets if the graph view should be dimmed due to rendering or being disabled.</summary>
		public bool IsDimmed {
			get => isDimmed;
			protected set {
				if (isDimmed != value) {
					isDimmed = value;

					if (isDimmed)
						HoverItem = null;
					else if (isMouseOver)
						UpdateHover();

					OnPropertyChanged();
				}
			}
		}

		/// <summary>Gets if the graph view is visually enabled.</summary>
		public bool IsEnabled {
			get => isEnabled;
			private set {
				if (isEnabled != value) {
					isEnabled = value;
					UpdateDimmed();
					if (!isEnabled) {
						AbortRender();
						disabledTreemapImage = treemapImage;
						disabledHighlightImage = highlightImage;
						TreemapImage = disabledTreemapImage;
						HighlightImage = disabledHighlightImage;
					}
					else {
						disabledTreemapImage = null;
						disabledHighlightImage = null;
						OnPropertyChanged(nameof(TreemapImage));
						// GraphView will render if (!IsEnabled and RootItem != null)
						Render();
					}
					OnPropertyChanged();
				}
			}
		}
		
		/// <summary>Gets or sets the dimensions of the graph view.</summary>
		public Point2I Dimensions {
			get => dimensions;
			set => SetProperty(ref dimensions, value);
		}

		/// <summary>Gets or sets the current mouse position inside the graph.</summary>
		public Point2I MousePosition {
			get => mousePosition;
			set => SetProperty(ref mousePosition, value);
		}

		/// <summary>ets or sets if the mouse is inside the graph.</summary>
		public bool IsMouseOver {
			get => isMouseOver;
			set => SetProperty(ref isMouseOver, value);
		}

		/// <summary>Gets the image to display the treemap.</summary>
		public ImageSource TreemapImage {
			get => treemapDisplayImage;
			private set => SetProperty(ref treemapDisplayImage, value);
		}

		/// <summary>Gets the image to display the highlight.</summary>
		public ImageSource HighlightImage {
			get => highlightDisplayImage;
			private set => SetProperty(ref highlightDisplayImage, value);
		}

		public ITreemapItem HoverItem {
			get => hoverItem;
			set => SetProperty(ref hoverItem, value);
		}

		public ITreemapItem RootItem {
			get => rootItem;
			set {
				if (rootItem != value) {
					rootItem = value;

					if (rootItem == null) {
						AbortRender();
					}
					
					HighlightNone();
					HoverItem = null;
					if (rootItem != null)
						Render();
					else
						Clear();

					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Rendering

		public void Refresh() {
			Render();
		}

		private void Render() {
			if (renderAsynchronously)
				RenderAsyncImpl();
			else
				RenderImpl();
		}

		private void RenderHighlight() {
			fullRender = !treemapRendered;
			if (IsRenderingAsync) {
				RenderAsyncImpl();
			}
			else if (!isDimmed && rootItem != null) {
				RenderImpl();
			}
		}

		/// <summary>Updates the dimmed settings.</summary>
		private void UpdateDimmed() {
			IsDimmed = IsRenderingTreemap || resizing || !IsEnabled;
		}

		/// <summary>Clears the current render.</summary>
		private void Clear() {
			AbortRender();
			treemapImage = null;
			highlightImage = null;
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

		private void RenderImpl() {
			if (IsEnabled && rootItem != null) {
				AbortRender();
				treemapRendered = false;
				Point2I size = dimensions;
				renderThread = null;
				RenderThread(size);
			}
		}

		private void RenderAsyncImpl() {
			if (IsEnabled && rootItem != null) {
				AbortRender();
				treemapRendered = false;
				//IsDimmed = true;
				Point2I size = dimensions;
				renderThread = new Thread(() => RenderThread(size)) {
					Priority = settings.RenderPriority,
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
				ui.Invoke(() => {
					// Let the control know we're not rendering anymore
					renderThread = null;
					TreemapImage = treemapImage;
					if (highlightMode != HighlightMode.None)
						HighlightImage = highlightImage;
					UpdateDimmed();
				});
			}
			catch (ThreadAbortException) { }
			catch (Exception ex) {
				Stopwatch sw = Stopwatch.StartNew();
				ui.Invoke(() => {
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
			if (treemapImage == null || treemapSize.X != size.X || treemapSize.Y != size.Y) {
				treemapSize = size;
				ui.Invoke(() => {
					treemapImage = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null);
				});
			}
			treemap.DrawTreemap(treemapImage, new Rectangle2I(size), rootItem);
			//Treemap.DrawTreemap(treemap, new Rectangle2I(size), fileRoot, options);
			Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render treemap");
		}
		private void RenderHighlight(Point2I size) {
			Stopwatch sw = Stopwatch.StartNew();
			if (highlightImage == null || highlightSize.X != size.X || highlightSize.Y != size.Y) {
				highlightSize = size;
				ui.Invoke(() => {
					highlightImage = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null);
				});
				Trace.WriteLine($"Took {sw.ElapsedMilliseconds}ms to setup highlight bitmap");
			}
			sw.Restart();
			if (highlightMode == HighlightMode.Extension) {
				treemap.HighlightExtensions(highlightImage, new Rectangle2I(size), (FileItemBase) rootItem, settings.HighlightColor, extension);
			}
			else if (highlightMode == HighlightMode.Selection) {
				treemap.HighlightItems(highlightImage, new Rectangle2I(size), settings.HighlightColor, selection);
			}
			Trace.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render highlight");
			Trace.WriteLine("");
		}

		#endregion

		#region Hover

		private void UpdateHover() {
			if (rootItem == null) {
				HoverItem = null;
				return;
			}
			if (hoverItem != null) {
				Rectangle2I rc = HoverItem.Rectangle;
				if (rc.Contains(mousePosition))
					return; // Hover is the same
			}
			HoverItem = TreemapRenderer.FindItemAtPoint(rootItem, mousePosition);
		}

		#endregion
	}
}
