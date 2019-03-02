using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.ViewModel.Drives {
	/// <summary>The view model that represents a drive item.</summary>
	public class DriveItemViewModel : ObservableObject {

		#region Fields

		/// <summary>The collection containing this drive item view model.</summary>
		private readonly DriveItemViewModelCollection drives;
		/// <summary>The model that this view model represents.</summary>
		public DriveItem Model { get; }

		/// <summary>The display icon for the drive.</summary>
		private IImage icon;
		/// <summary>The display name for the drive.</summary>
		private string displayName;

		#endregion

		#region Constructors

		/// <summary>Constructs the see <see cref="DriveItemViewModel"/>.</summary>
		/// 
		/// <param name="drives">The collection containing this drive item view model.</param>
		/// <param name="model">The model that this view model represents.</param>
		public DriveItemViewModel(DriveItemViewModelCollection drives, DriveItem model) {
			this.drives = drives;
			Model = model;

			IIconAndName iconName = IconCache.CacheIconAndDisplayName(Name);
			if (iconName != null) {
				Icon = iconName.Icon;
				DisplayName = iconName.Name;
			}
			else {
				Icon = IconCache.VolumeIcon;
				DisplayName = $"({PathUtils.TrimSeparatorEnd(Name)})";
			}
		}

		#endregion

		#region Properties

		/// <summary>Gets the display icon for the drive.</summary>
		public IImage Icon {
			get => icon;
			private set => Set(ref icon, value);
		}

		/// <summary>Gets the display name for the drive.</summary>
		public string DisplayName {
			get => displayName;
			private set => Set(ref displayName, value);
		}

		/// <summary>Gets the name/path of the drive.</summary>
		public string Name => Model.Name;
		/// <summary>Gets the total size of the drive.</summary>
		public long TotalSize => Model.TotalSize;
		/// <summary>Gets the freespace on the drive in bytes.</summary>
		public long FreeSpace => Model.FreeSpace;
		/// <summary>Gets the type of the drive.</summary>
		public DriveType DriveType => Model.DriveType;
		/// <summary>Gets the partition format of the drive.</summary>
		public string DriveFormat => Model.DriveFormat;
		/// <summary>Gets the used space on the drive in bytes.</summary>
		public long UsedSpace => Model.UsedSpace;
		/// <summary>Gets the used percentage of the drive.</summary>
		public double Percent => Model.Percent;

		/// <summary>Gets the icon cache service.</summary>
		private IIconCacheService IconCache => drives.IconCache;

		/// <summary>Gets the program settings service.</summary>
		private SettingsService Settings => drives.Settings;

		/// <summary>Gets the icon cache mode setting.</summary>
		private IconCacheMode CacheMode => drives.Settings.IconCacheMode;

		#endregion
	}
}
