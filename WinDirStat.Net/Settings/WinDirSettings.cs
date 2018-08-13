using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Controls;
using WinDirStat.Net.Data;
using WinDirStat.Net.Settings.Geometry;

namespace WinDirStat.Net.Settings {
	public partial class WinDirSettings : IDisposable, INotifyPropertyChanged {
		
		private TreemapOptions treemapOptions;

		private Rgb24Color[] filePalette;

		private Rgb24Color[] subtreePalette;
		
		private bool showFreeSpace;
		private bool showUnknown;
		private IconCacheMode iconCacheMode;

		private TimeSpan ramInterval;
		private TimeSpan statusInterval;
		private TimeSpan validateInterval;

		private ThreadPriority renderPriority;
		private ThreadPriority scanPriority;

		private readonly WinDirDocument document;

		public WinDirSettings(WinDirDocument document) {
			this.document = document;
			Reset();
		}

		public WinDirDocument Document {
			get => document;
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
			get => Array.AsReadOnly(filePalette);
		}

		public ReadOnlyCollection<Rgb24Color> SubtreePalette {
			get => Array.AsReadOnly(subtreePalette);
		}
		
		public Rgb24Color GetFilePaletteColor(int index) {
			return filePalette[index];
		}

		public void SetFilePalette(IEnumerable<Rgb24Color> filePalette) {
			this.filePalette = EqualizeColors(filePalette);
			RaisePropertyChanged(nameof(FilePalette));
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

		public void Dispose() {
			
		}
	}
}
