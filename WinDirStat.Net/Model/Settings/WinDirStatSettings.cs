using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinDirStat.Net.Controls;
using WinDirStat.Net.Drawing;
using WinDirStat.Net.Model.View;
using WinDirStat.Net.Settings;
using WinDirStat.Net.Settings.Geometry;
using WinDirStat.Net.Windows;

namespace WinDirStat.Net.Model.Settings {
	/*public class TreemapInvalidatedEventArgs : EventArgs {
		public bool Render { get; }
		public bool Highlight { get; }

		public TreemapInvalidatedEventArgs(bool render, bool highlight) {
			Render = render;
			Highlight = highlight;
		}
	}

	public delegate void TreemapInvalidatedEventHandler(object sender, TreemapInvalidatedEventArgs e);*/

	public partial class WinDirStatSettings : IDisposable, INotifyPropertyChanged {
		
		private TreemapOptions treemapOptions;
		private Rgba32Color highlightColor;

		private Rgb24Color[] filePalette;
		private Rgb24Color[] equalizedFilePalette;
		private ImageSource[] filePalettePreviews;

		private Rgb24Color[] subtreePalette;
		
		private bool showFreeSpace;
		private bool showUnknown;
		private IconCacheMode iconCacheMode;

		private TimeSpan ramInterval;
		private TimeSpan statusInterval;
		private TimeSpan validateInterval;

		private ThreadPriority renderPriority;
		private ThreadPriority scanPriority;

		private DriveSelectMode selectDriveMode;
		private string[] selectedDrives;
		private string selectedFolderPath;

		private bool showFileTypes;
		private bool showTreemap;
		private bool showToolBar;
		private bool showStatusBar;

		private readonly WinDirStatViewModel model;

		public WinDirStatSettings(WinDirStatViewModel model) {
			this.model = model;
			Reset();
			PropertyChanged += OnPropertyChanged;
		}

		public WinDirStatViewModel Model {
			get => model;
		}

		public DriveSelectMode DriveSelectMode {
			get => selectDriveMode;
			set {
				if (selectDriveMode != value) {
					selectDriveMode = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public string[] SelectedDrives {
			get => selectedDrives;
			set {
				if (selectedDrives != value) {
					if (value == null || value.Any(s => string.IsNullOrEmpty(s)))
						throw new ArgumentNullException(nameof(SelectedDrives));
					selectedDrives = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public string SelectedFolderPath {
			get => selectedFolderPath;
			set {
				if (selectedFolderPath != value) {
					selectedFolderPath = value ?? throw new ArgumentNullException(nameof(SelectedFolderPath));
					AutoRaisePropertyChanged();
				}
			}
		}

		public ThreadPriority RenderPriority {
			get => renderPriority;
			set {
				if (renderPriority != value) {
					renderPriority = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public ThreadPriority ScanPriority {
			get => scanPriority;
			set {
				if (scanPriority != value) {
					scanPriority = value;
					AutoRaisePropertyChanged();
				}
			}
		}
		
		public TimeSpan RAMInterval {
			get => ramInterval;
			set {
				if (ramInterval != value) {
					ramInterval = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public TimeSpan StatusInterval {
			get => statusInterval;
			set {
				if (statusInterval != value) {
					statusInterval = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public TimeSpan ValidateInterval {
			get => validateInterval;
			set {
				if (validateInterval != value) {
					validateInterval = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public TreemapOptions TreemapOptions {
			get => treemapOptions;
			set {
				//if (treemapOptions != value) {
					treemapOptions = value;
					AutoRaisePropertyChanged();
				//}
			}
		}

		public Rgba32Color HighlightColor {
			get => highlightColor;
			set {
				highlightColor = value;
				AutoRaisePropertyChanged();
			}
		}

		public bool ShowUnknown {
			get => showUnknown;
			set {
				if (showUnknown != value) {
					showUnknown = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public bool ShowFreeSpace {
			get => showFreeSpace;
			set {
				if (showFreeSpace != value) {
					showFreeSpace = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public bool ShowFileTypes {
			get => showFileTypes;
			set {
				if (showFileTypes != value) {
					showFileTypes = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public bool ShowTreemap {
			get => showTreemap;
			set {
				if (showTreemap != value) {
					showTreemap = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public bool ShowToolBar {
			get => showToolBar;
			set {
				if (showToolBar != value) {
					showToolBar = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public bool ShowStatusBar {
			get => showStatusBar;
			set {
				if (showStatusBar != value) {
					showStatusBar = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public IconCacheMode IconCacheMode {
			get => iconCacheMode;
			set {
				if (iconCacheMode != value) {
					iconCacheMode = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public ReadOnlyCollection<Rgb24Color> FilePalette {
			get => Array.AsReadOnly(equalizedFilePalette);
		}

		public ReadOnlyCollection<Rgb24Color> OriginalFilePalette {
			get => Array.AsReadOnly(filePalette);
		}

		public ReadOnlyCollection<ImageSource> FilePalettePreviews {
			get => Array.AsReadOnly(filePalettePreviews);
		}

		public ReadOnlyCollection<Rgb24Color> SubtreePalette {
			get => Array.AsReadOnly(subtreePalette);
		}
		
		public Rgb24Color GetFilePaletteColor(int index) {
			return equalizedFilePalette[index];
		}

		public ImageSource GetFilePalettePreview(int index) {
			return filePalettePreviews[index];
		}

		public Rgb24Color GetSubtreePaletteColor(int level) {
			return subtreePalette[level % subtreePalette.Length];
		}

		public void SetFilePalette(IEnumerable<Rgb24Color> filePalette) {
			this.filePalette = filePalette.ToArray();
			this.equalizedFilePalette = EqualizeColors(filePalette);
			UpdateFilePalettePreviews();
			RaisePropertyChanged(nameof(FilePalette));
			RaisePropertyChanged(nameof(OriginalFilePalette));
		}

		public void SetSubtreePalette(IEnumerable<Rgb24Color> subtreePalette) {
			this.subtreePalette = subtreePalette.ToArray();
			RaisePropertyChanged(nameof(SubtreePalette));
		}

		public event PropertyChangedEventHandler PropertyChanged;
		
		private void RaisePropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void AutoRaisePropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			case nameof(TreemapOptions):
				UpdateFilePalettePreviews();
				break;
			}
		}

		public void UpdateFilePalettePreviews() {
			if (filePalettePreviews == null)
				filePalettePreviews = new ImageSource[filePalette.Length];
			else if (filePalette.Length != filePalettePreviews.Length)
				Array.Resize(ref filePalettePreviews, filePalette.Length);
			for (int i = 0; i < filePalettePreviews.Length; i++) {
				WriteableBitmap bitmap = (WriteableBitmap) filePalettePreviews[i];
				if (bitmap == null) {
					bitmap = new WriteableBitmap(256, 13, 96, 96, PixelFormats.Bgra32, null);
					filePalettePreviews[i] = bitmap;
				}
				Treemap.DrawColorPreview(bitmap, new Rectangle2I(256, 13), equalizedFilePalette[i], treemapOptions);
			}
			RaisePropertyChanged(nameof(FilePalettePreviews));
		}

		public void Dispose() {
			
		}
	}
}
