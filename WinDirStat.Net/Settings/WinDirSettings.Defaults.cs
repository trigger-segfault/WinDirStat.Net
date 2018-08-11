using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Controls;
using WinDirStat.Net.Drawing;
using WinDirStat.Net.Settings.Geometry;

namespace WinDirStat.Net.Settings {
	public partial class WinDirSettings : IDisposable, INotifyPropertyChanged {

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


		public static readonly ReadOnlyCollection<Rgb24Color> DefaultSubtreePalette =
			Array.AsReadOnly(new[] {
			new Rgb24Color(64, 64, 140),
			new Rgb24Color(140, 64, 64),
			new Rgb24Color(64, 140, 64),
			new Rgb24Color(140, 140, 64),
		});

		public const float PaletteBrightness = 0.6f;
		
		private static Rgb24Color[] EqualizeColors(IEnumerable<Rgb24Color> colors) {
			List<Rgb24Color> result = new List<Rgb24Color>();
			foreach (Rgb24Color color in colors)
				result.Add(ColorSpace.SetBrightness(color, PaletteBrightness));
			return result.ToArray();
		}

		public static readonly TimeSpan DefaultRAMInterval = TimeSpan.FromSeconds(1.0);
		public static readonly TimeSpan DefaultStatusInterval = TimeSpan.FromSeconds(0.05);
		public static readonly TimeSpan DefaultValidateInterval = TimeSpan.FromSeconds(2);

		public void Reset() {
			IconCacheMode = IconCacheMode.Individual;
			TreemapOptions = TreemapOptions.Default;
			SetFilePalette(DefaultFilePalette);
			SetSubtreePalette(DefaultSubtreePalette);
			ShowUnknown = false;
			ShowFreeSpace = false;
			RAMInterval = DefaultRAMInterval;
			StatusInterval = DefaultStatusInterval;
			ValidateInterval = DefaultValidateInterval;
			RenderPriority = ThreadPriority.BelowNormal;
			ScanPriority = ThreadPriority.AboveNormal;
		}

	}
}
