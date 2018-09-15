using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Model.View.Drives {
	public class DriveViewModel : ObservableObject {

		private readonly DriveSelectViewModel viewModel;
		private readonly string name;
		private readonly long total;
		private readonly long free;
		private readonly DriveType type;
		private readonly string format;

		private readonly string displayName;
		private readonly ImageSource icon;


		public DriveViewModel(DriveSelectViewModel viewModel, DriveInfo info) {
			this.viewModel = viewModel;
			name = info.Name;
			total = info.TotalSize;
			free = info.AvailableFreeSpace;
			type = info.DriveType;
			format = info.DriveFormat;

			icon = viewModel.Icons.CacheIcon(name, 0, out displayName);
			if (icon == null)
				icon = IconCache.VolumeIcon;
		}

		public long Used {
			get => Math.Max(0L, total - free);
		}

		public long Total {
			get => total;
		}

		public long Free {
			get => free;
		}

		public string Name {
			get => name;
		}
		public string DisplayName {
			get => displayName ?? $"({PathUtils.TrimSeparatorEnd(name)})";
		}
		public ImageSource Icon {
			get => icon;
		}

		public DriveType Type {
			get => type;
		}
		public string Format {
			get => format;
		}

		public double Percent {
			get => (double) Used / total;
		}
	}
}
