using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Rendering;
using WinDirStat.Net.Structures;

namespace WinDirStat.Net.Services {
	public interface ISettingsService : INotifyPropertyChanged {

		DriveSelectMode DriveSelectMode { get; set; }
		string[] SelectedDrives { get; set; }
		string SelectedFolderPath { get; set; }

		ThreadPriority RenderPriority { get; set; }
		ThreadPriority ScanPriority { get; set; }

		TimeSpan RAMInterval { get; set; }
		TimeSpan StatusInterval { get; set; }
		TimeSpan ValidateInterval { get; set; }

		TreemapOptions TreemapOptions { get; set; }
		Rgba32Color HighlightColor { get; set; }

		bool ShowUnknown { get; set; }
		bool ShowFreeSpace { get; set; }
		bool ShowTotalSpace { get; set; }

		bool ShowFileTypes { get; set; }
		bool ShowTreemap { get; set; }
		bool ShowToolBar { get; set; }
		bool ShowStatusBar { get; set; }

		IconCacheMode IconCacheMode { get; set; }

		ReadOnlyCollection<Rgb24Color> FilePalette { get; }
		ReadOnlyCollection<Rgb24Color> OriginalFilePalette { get; }
		IList FilePalettePreviews { get; }

		Rgb24Color GetFilePaletteColor(int index);
		void SetFilePalette(IEnumerable<Rgb24Color> filePalette);
		object GetFilePalettePreview(int index);

		ReadOnlyCollection<Rgb24Color> SubtreePalette { get; }

		Rgb24Color GetSubtreePaletteColor(int level);
		void SetSubtreePalette(IEnumerable<Rgb24Color> subtreePalette);

	}
}
