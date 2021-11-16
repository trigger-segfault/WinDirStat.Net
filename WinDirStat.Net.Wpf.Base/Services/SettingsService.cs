using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Rendering;
using WinDirStat.Net.Structures;

namespace WinDirStat.Net.Services {
    /// <summary>The service for containing all program settings.</summary>
    public partial class SettingsService : ObservableObjectEx {

		#region Fields

		/// <summary>The UI service.</summary>
		private readonly UIService ui;
		/// <summary>The treemap renderer.</summary>
		private readonly TreemapRenderer treemap;

		// Drive Select
		/// <summary>The default mode for selecting paths to scan.</summary>
		protected DriveSelectMode driveSelectMode;
		/// <summary>The selected drives from the individual list.</summary>
		protected string[] selectedDrives;
		/// <summary>The selected folder path.</summary>
		protected string selectedFolderPath;

		// CPU Usage
		/// <summary>The thread priority for scanning the file tree.</summary>
		protected ThreadPriority scanPriority;
		/// <summary>The thread priority for rendering the treemap.</summary>
		protected ThreadPriority renderPriority;
		/// <summary>How often the RAM Usage is updated.</summary>
		protected TimeSpan ramInterval;
		/// <summary>How often the scan status is updated.</summary>
		protected TimeSpan statusInterval;
		/// <summary>How often the scanned file tree is updated in the UI.</summary>
		protected TimeSpan validateInterval;

		// Space Display
		/// <summary>True if free space items are displayed for drives.</summary>
		protected bool showFreeSpace;
		/// <summary>True if unknown space items are displayed for drives.</summary>
		protected bool showUnknown;
		/// <summary>
		/// True if space items should show one single root item instead of one for each individual drive.
		/// </summary>
		protected bool showTotalSpace;

		// UI Display
		/// <summary>True if the file types list should be shown.</summary>
		protected bool showFileTypes;
		/// <summary>True if the treemap should be shown.</summary>
		protected bool showTreemap;
		/// <summary>True if the tool bar should be shown.</summary>
		protected bool showToolBar;
		/// <summary>True if the status bar should be shown.</summary>
		protected bool showStatusBar;
		/// <summary>The setting for how icons are cached for file tree items.</summary>
		protected IconCacheMode iconCacheMode;

		// Treemap
		/// <summary>The options for how the treemap is rendered.</summary>
		protected TreemapOptions treemapOptions;
		/// <summary>The treemap highlight color.</summary>
		protected Rgba32Color highlightColor;

		// File Palette
		/// <summary>The array of the original (not equalized) file palette colors.</summary>
		protected Rgb24Color[] filePalette = new Rgb24Color[0];
		/// <summary>The array of the prepared (equalized) file palette colors.</summary>
		protected Rgb24Color[] equalizedFilePalette = new Rgb24Color[0];
		/// <summary>The array of file palette preview images.</summary>
		protected ImageSource[] filePalettePreviews = new ImageSource[0];

		// Subtree Palette
		/// <summary>The array of subtree percentage bar colors.</summary>
		protected Rgb24Color[] subtreePalette = new Rgb24Color[0];

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="SettingsService"/>.</summary>
		public SettingsService(UIService ui,
							   TreemapRendererFactory treemapFactory)
		{
			this.ui = ui;
			treemap = treemapFactory.Create();
			Reset();
		}

		#endregion

		#region Drive Select

		/// <summary>Gets or sets the default mode for selecting paths to scan.</summary>
		public DriveSelectMode DriveSelectMode {
			get => driveSelectMode;
			set => SetProperty(ref driveSelectMode, value);
		}

		/// <summary>Gets or sets the selected drives from the individual list.</summary>
		public string[] SelectedDrives {
			get => selectedDrives;
			set {
				if (value == null || value.Any(s => string.IsNullOrEmpty(s)))
					throw new ArgumentNullException(nameof(SelectedDrives));
                SetProperty(ref selectedDrives, value);
			}
		}

		/// <summary>Gets or sets the selected folder path.</summary>
		public string SelectedFolderPath {
			get => selectedFolderPath;
			set => SetProperty(ref selectedFolderPath, value);
		}

		#endregion

		#region CPU Usage

		/// <summary>Gets or sets the thread priority for scanning the file tree.</summary>
		public ThreadPriority ScanPriority {
			get => scanPriority;
			set => SetProperty(ref scanPriority, value);
		}

		/// <summary>Gets or sets the thread priority for rendering the treemap.</summary>
		public ThreadPriority RenderPriority {
			get => renderPriority;
			set => SetProperty(ref renderPriority, value);
		}

		/// <summary>Gets or sets how often the RAM Usage is updated.</summary>
		public TimeSpan RAMInterval {
			get => ramInterval;
			set => SetProperty(ref ramInterval, value);
		}

		/// <summary>Gets or sets how often the scan status is updated.</summary>
		public TimeSpan StatusInterval {
			get => statusInterval;
			set => SetProperty(ref statusInterval, value);
		}

		/// <summary>Gets or sets how often the scanned file tree is updated in the UI.</summary>
		public TimeSpan ValidateInterval {
			get => validateInterval;
			set => SetProperty(ref validateInterval, value);
		}

		#endregion

		#region Space Display

		/// <summary>Gets or sets if free space items are displayed for drives.</summary>
		public bool ShowFreeSpace {
			get => showFreeSpace;
			set => SetProperty(ref showFreeSpace, value);
		}

		/// <summary>Gets or sets if unknown space items are displayed for drives.</summary>
		public bool ShowUnknown {
			get => showUnknown;
			set => SetProperty(ref showUnknown, value);
		}

		/// <summary>
		/// Gets or sets if space items should show one single root item instead of one for each individual
		/// drive.
		/// </summary>
		public bool ShowTotalSpace {
			get => showTotalSpace;
			set => SetProperty(ref showTotalSpace, value);
		}

		#endregion

		#region UI Display

		/// <summary>Gets or sets if the file types list should be shown.</summary>
		public bool ShowFileTypes {
			get => showFileTypes;
			set => SetProperty(ref showFileTypes, value);
		}

		/// <summary>Gets or sets if the treemap should be shown.</summary>
		public bool ShowTreemap {
			get => showTreemap;
			set => SetProperty(ref showTreemap, value);
		}

		/// <summary>Gets or sets if the tool bar should be shown.</summary>
		public bool ShowToolBar {
			get => showToolBar;
			set => SetProperty(ref showToolBar, value);
		}

		/// <summary>Gets or sets if the status bar should be shown.</summary>
		public bool ShowStatusBar {
			get => showStatusBar;
			set => SetProperty(ref showStatusBar, value);
		}

		/// <summary>Gets or sets the setting for how icons are cached for file tree items.</summary>
		public IconCacheMode IconCacheMode {
			get => iconCacheMode;
			set => SetProperty(ref iconCacheMode, value);
		}

		#endregion

		#region Treemap

		/// <summary>Gets or sets the options for how the treemap is rendered.</summary>
		public TreemapOptions TreemapOptions {
			get => treemapOptions;
			set {
				treemapOptions = value;
				UpdateFilePalettePreviews();
				OnPropertyChanged();
			}
		}

		/// <summary>Gets or sets the treemap highlight color.</summary>
		public Rgba32Color HighlightColor {
			get => highlightColor;
			set => SetProperty(ref highlightColor, value);
		}

		/// <summary>Gets the array of the prepared (equalized) file palette colors.</summary>
		public ReadOnlyCollection<Rgb24Color> FilePalette {
			get => Array.AsReadOnly(equalizedFilePalette);
		}

		/// <summary>Gets the array of the original (not equalized) file palette colors.</summary>
		public ReadOnlyCollection<Rgb24Color> OriginalFilePalette {
			get => Array.AsReadOnly(filePalette);
		}

		/// <summary>Gets the array of file palette preview images.</summary>
		public ReadOnlyCollection<ImageSource> FilePalettePreviews {
			get => Array.AsReadOnly(filePalettePreviews);
		}

		/// <summary>Gets the file palette color at the specified index.</summary>
		/// 
		/// <param name="index">The index of the extension in the list ordered by size descending.</param>
		/// <returns>The color of the file palette.</returns>
		public Rgb24Color GetFilePaletteColor(int index) {
			return equalizedFilePalette[Math.Min(index, equalizedFilePalette.Length - 1)];
		}

		/// <summary>Gets the file palette color preview image at the specified index.</summary>
		/// 
		/// <param name="index">The index of the extension in the list ordered by size descending.</param>
		/// <returns>The preview image of the file palette color.</returns>
		public ImageSource GetFilePalettePreview(int index) {
			return filePalettePreviews[Math.Min(index, filePalettePreviews.Length - 1)];
		}

		/// <summary>Sets the new file palette colors.</summary>
		/// 
		/// <param name="filePalette">The new collection of colors to use.</param>
		public void SetFilePalette(IEnumerable<Rgb24Color> filePalette) {
			this.filePalette = filePalette.ToArray();
			this.equalizedFilePalette = ColorSpace.EqualizeColors(filePalette);
			UpdateFilePalettePreviews();
			OnPropertyChanged(nameof(FilePalette));
			OnPropertyChanged(nameof(OriginalFilePalette));
		}

		#endregion

		#region Subtree Percentage

		/// <summary>Gets the array of subtree percentage bar colors.</summary>
		public ReadOnlyCollection<Rgb24Color> SubtreePalette {
			get => Array.AsReadOnly(subtreePalette);
		}

		/// <summary>Gets the subtree percentage bar color at the specified level.</summary>
		/// 
		/// <param name="level">The 0-indexed level.</param>
		/// <returns>The color of the subtree percentage bar.</returns>
		public Rgb24Color GetSubtreePaletteColor(int level) {
			return subtreePalette[level % subtreePalette.Length];
		}

		/// <summary>Sets the new subtree percentage bar colors.</summary>
		/// 
		/// <param name="subtreePalette">The new collection of colors to use.</param>
		public void SetSubtreePalette(IEnumerable<Rgb24Color> subtreePalette) {
			this.subtreePalette = subtreePalette.ToArray();
			OnPropertyChanged(nameof(SubtreePalette));
		}

		#endregion

		#region Private Methods

		/// <summary>Updates the file palette previews after a change has been made to the list.</summary>
		protected void UpdateFilePalettePreviews() {
			if (filePalettePreviews == null)
				filePalettePreviews = new WriteableBitmap[filePalette.Length];
			else if (filePalette.Length != filePalettePreviews.Length)
				Array.Resize(ref filePalettePreviews, filePalette.Length);
			for (int i = 0; i < filePalettePreviews.Length; i++) {
				DrawFilePalettePreview(ref filePalettePreviews[i], equalizedFilePalette[i]);
			}
			OnPropertyChanged(nameof(FilePalettePreviews));
		}

		/// <summary>Draws a single file palette preview.</summary>
		/// 
		/// <param name="image">The image reference to update.</param>
		protected void DrawFilePalettePreview(ref ImageSource image, Rgb24Color color) {
			WriteableBitmap bitmap = (WriteableBitmap) image;
			if (bitmap == null) {
				bitmap = new WriteableBitmap(256, 13, 96, 96, PixelFormats.Bgra32, null);
				image = bitmap;
			}
			treemap.DrawColorPreview(bitmap, new Rectangle2I(256, 13), color);
		}

		#endregion
	}
}
