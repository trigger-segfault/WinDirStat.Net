using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Rendering;
using WinDirStat.Net.Services;
using WinDirStat.Net.Structures;
using WinDirStat.Net.ViewModel;
using WinDirStat.Net.Wpf.Utils;

namespace WinDirStat.Net.Wpf.Controls {
    public class GraphViewHoverEventArgs : RoutedEventArgs {

        public FileItemBase Hover;

        public GraphViewHoverEventArgs(RoutedEvent routedEvent, FileItemBase hover)
            : base(routedEvent) {
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

        private static async void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not GraphView graphView)
                return;

            FileItemBase root = graphView.Root;
            //if (root == null) {
            //    await graphView.AbortRenderAsync();
            //}

            graphView.root = root;
            graphView.HighlightNone();
            graphView.Hover = null;
            if (root != null) {
                await graphView.RenderAsync();
            }
            else {
                var render = graphView.renderTask;
                if (render != null) {
                    await render;
                }
                graphView.Clear();
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

        private static async void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is not GraphView graphView)
                return;

            graphView.UpdateDimmed();
            if (graphView.IsEnabled) {
                // GraphView will render if (!IsEnabled and Root != null)
                await graphView.RenderAsync();
            }
        }

        private void UpdateDimmed() => IsDimmed = IsRenderingTreemap || resizing || !IsEnabled;

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

        private async void OnSettingsChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SettingsService.TreemapOptions):
                case nameof(SettingsService.FilePalette):
                    await RenderAsync();
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
        private readonly DispatcherTimer resizeTimer;

        private Task renderTask;
        private AutoResetEvent renderGate;

        private FileItemBase root;
        private bool resizing;
        /// <summary>Check if the running render thread has finished rendering the treemap.</summary>
        private volatile bool treemapRendered;
        private volatile bool fullRender;
        //private TreemapOptions options;

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

            renderGate = new(true);
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

        public async void HighlightExtension(string extension) {
            if (this.extension == extension)
                return;

            highlightMode = HighlightMode.Extension;
            this.extension = extension;
            selection = null;
            if (IsRendering) {
                await RenderHighlightAsync();
            }
            else if (!IsDimmed && Root != null) {
                RenderHighlight(treemapSize);
                if (imageHighlight.Source != highlight)
                    imageHighlight.Source = highlight;
            }
            //RenderHighlightAsync(ThreadPriority.Normal);
            //Render(true);
        }

        public async void HighlightSelection(IEnumerable selection) {
            if (this.selection != null && this.selection.Intersect(selection.Cast<FileItemBase>()).Count() == this.selection.Length)
                return;

            highlightMode = HighlightMode.Selection;
            this.selection = selection.Cast<FileItemBase>().ToArray();
            extension = null;
            if (IsRendering) {
                await RenderHighlightAsync();
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
            //await AbortRenderAsync();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
            resizing = true;
            IsDimmed = true;
            // Restart the timer by stopping it first
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private async void OnResizeTimerTick(object sender, EventArgs e) {
            resizing = false;
            resizeTimer.Stop();
            await RenderAsync();
        }



        /// <summary>Gets if anything is in the process of being rendered.</summary>
        private bool IsRendering => !renderTask?.IsCompleted ?? false;

        /// <summary>Gets if the treemap is in the process of being rendered.</summary>
        private bool IsRenderingTreemap => fullRender && IsRendering;

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
            //await AbortRenderAsync();
            treemap = null;
            highlight = null;
            treemapSize = Point2I.Zero;
            highlightSize = Point2I.Zero;
        }

        //public async Task AbortRenderAsync(bool waitForExit = true) {
        //    //FIXME: waitForExit was never observed anyway
        //    if (waitForExit && IsRendering) {
        //        await renderTask;
        //    }
        //    UpdateDimmed();
        //}

        public Task RenderHighlightAsync(ThreadPriority? priority = null) {
            fullRender = !treemapRendered;
            return RenderCoreAsync(priority);
        }

        public Task RenderAsync(ThreadPriority? priority = null) {
            fullRender = true;
            return RenderCoreAsync(priority);
        }

        private async Task RenderCoreAsync(ThreadPriority? priority = null) {
            if (!IsEnabled)
                return;

            if (Root == null)
                return;

            var acquired = renderGate.WaitOne(0);
            if (!acquired)
                return;

            //await AbortRenderAsync();
            treemapRendered = false;

            var size = new Point2I((int) ActualWidth, (int) ActualHeight);

            Debug.Assert(renderTask == null || renderTask.IsCompleted);
            renderTask = Task.Run(() => RenderWorker(size));
            UpdateDimmed();
            await renderTask;

            Dispatcher.Invoke(RenderFinished);
        }

        private void RenderWorker(Point2I size) {
            if (size.X != 0 && size.Y != 0) {
                if (fullRender) {
                    RenderTreemap(size);
                }
                treemapRendered = true;

                if (highlightMode != HighlightMode.None) {
                    RenderHighlight(size);
                }
            }
        }

        private void RenderFinished() {
            // Let the control know we're not rendering anymore
            imageTreemap.Source = treemap;
            if (highlightMode != HighlightMode.None) {
                imageHighlight.Source = highlight;
            }
            UpdateDimmed();

            renderGate.Set();
        }

        private void RenderTreemap(Point2I size) {
            Stopwatch sw = Stopwatch.StartNew();
            if (treemap == null || treemapSize != size) {
                treemapSize = size;
                //treemap = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null);
                Dispatcher.Invoke(() => treemap = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null));
            }
            Treemap.DrawTreemap(treemap, new Rectangle2I(size), root);
            sw.Stop();

            Debug.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render treemap");
        }
        private void RenderHighlight(Point2I size) {
            Stopwatch sw = Stopwatch.StartNew();
            if (highlight == null || highlightSize != size) {
                highlightSize = size;
                //highlight = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null);
                Dispatcher.Invoke(() => highlight = new WriteableBitmap(size.X, size.Y, 96, 96, PixelFormats.Bgra32, null));
            }
            sw.Stop();
            Debug.WriteLine($"Took {sw.ElapsedMilliseconds}ms to setup highlight bitmap");

            sw.Restart();
            if (highlightMode == HighlightMode.Extension) {
                Treemap.HighlightExtensions(highlight, new Rectangle2I(size), root, Settings.HighlightColor, extension);
            }
            else if (highlightMode == HighlightMode.Selection) {
                Treemap.HighlightItems(highlight, new Rectangle2I(size), Settings.HighlightColor, selection);
            }
            sw.Stop();
            Debug.WriteLine($"Took {sw.ElapsedMilliseconds}ms to render highlight");
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
            Hover = (FileItemBase) TreemapRenderer.FindItemAtPoint(root, point);
        }
    }
}
