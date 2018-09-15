using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Rendering;
using WinDirStat.Net.Structures;

namespace WinDirStat.Net.Services {
	partial class SettingsService {
		/// <summary>The default file palette colors.</summary>
		public static readonly ReadOnlyCollection<Rgb24Color> DefaultFilePalette =
			Array.AsReadOnly(new[] {
			new Rgb24Color(0, 0, 255),
			new Rgb24Color(255, 0, 0),
			new Rgb24Color(0, 255, 0),
			new Rgb24Color(0, 255, 255),
			new Rgb24Color(255, 0, 255),
			new Rgb24Color(255, 255, 0),
			new Rgb24Color(150, 150, 255),
			new Rgb24Color(255, 150, 150),
			new Rgb24Color(150, 255, 150),
			new Rgb24Color(150, 255, 255),
			new Rgb24Color(255, 150, 255),
			new Rgb24Color(255, 255, 150),
			new Rgb24Color(255, 255, 255),
		});
		
		/// <summary>The default subtree percentage bar colors.</summary>
		public static readonly ReadOnlyCollection<Rgb24Color> DefaultSubtreePalette =
			Array.AsReadOnly(new[] {
			new Rgb24Color(64, 64, 140),
			new Rgb24Color(140, 64, 64),
			new Rgb24Color(64, 140, 64),
			new Rgb24Color(140, 140, 64),
		});

		/*/// <summary>The default interval for updating the RAM usage.</summary>
		public static readonly TimeSpan DefaultRAMInterval = TimeSpan.FromSeconds(1.0);
		/// <summary>The default interval for updating the scan status.</summary>
		public static readonly TimeSpan DefaultStatusInterval = TimeSpan.FromSeconds(0.05);
		/// <summary>The default interval for updating the file tree in the UI.</summary>
		public static readonly TimeSpan DefaultValidateInterval = TimeSpan.FromSeconds(2);*/

		/// <summary>Resets the settings to their defaults.</summary>
		public void Reset() {
			// Drive Select
			DriveSelectMode = DriveSelectMode.Individual;
			SelectedDrives = new[] { @"C:\" };
			SelectedFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

			// CPU Usage
			ScanPriority = ThreadPriority.Highest;
			RenderPriority = ThreadPriority.AboveNormal;
			RAMInterval = TimeSpan.FromSeconds(1.0);
			StatusInterval = TimeSpan.FromSeconds(0.05);
			ValidateInterval = TimeSpan.FromSeconds(1);

			// Space Display
			ShowFreeSpace = false;
			ShowUnknown = false;
			ShowTotalSpace = false;

			// UI Display
			ShowFileTypes = true;
			ShowTreemap = true;
			ShowToolBar = true;
			ShowStatusBar = true;
			IconCacheMode = IconCacheMode.Individual;

			// Treemap
			TreemapOptions = TreemapOptions.Default;
			HighlightColor = Rgba32Color.White;

			// File Palette
			SetFilePalette(DefaultFilePalette);

			// Subtree Percentage
			SetSubtreePalette(DefaultSubtreePalette);
		}
	}
}
