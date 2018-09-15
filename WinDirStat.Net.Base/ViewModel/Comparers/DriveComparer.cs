using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.ViewModel.Drives;

namespace WinDirStat.Net.ViewModel.Comparers {
	/// <summary>The sort mode for use in the drive select list.</summary>
	[Serializable]
	public enum DriveSortMode {
		None,
		Name,
		Total,
		Free,
		UsedTotalBar,
		UsedTotalPercent,
		[Browsable(false)]
		Count,
	}

	/// <summary>The comparer for <see cref="DriveItemViewModel"/>s.</summary>
	public class DriveComparer : SortComparer<DriveItemViewModel, DriveSortMode> {

		#region Constructors

		/// <summary>Constructs the default <see cref="DriveComparer"/>.</summary>
		public DriveComparer()
			: base(DriveSortMode.Name, ListSortDirection.Ascending)
		{
		}

		#endregion

		#region SortComparer Overrides

		/// <summary>A secondary comparison that's called when the sort mode comparison returns 0.</summary>
		/// 
		/// <param name="a">The first item to compare.</param>
		/// <param name="b">The second item to compare.</param>
		/// <returns>The comparison result.</returns>
		protected override Comparison<DriveItemViewModel> GetSortComparison(DriveSortMode mode) {
			switch (mode) {
			case DriveSortMode.None:
			case DriveSortMode.Name:
				return SortByNameAndDriveType;
			case DriveSortMode.Total:
				return SortByTotal;
			case DriveSortMode.Free:
				return SortByFree;
			case DriveSortMode.UsedTotalBar:
			case DriveSortMode.UsedTotalPercent:
				return SortByPercent;
			default:
				throw new ArgumentException($"Invalid {typeof(DriveSortMode).Name} ({mode})!", nameof(mode));
			}
		}

		/// <summary>Gets the comparison method for the specified sort mode.</summary>
		/// 
		/// <param name="mode">The mode to get the comparison of.</param>
		/// <returns>The comparison to use.</returns>
		protected override int SecondaryCompare(DriveItemViewModel a, DriveItemViewModel b) {
			return string.Compare(a.Model.Name, b.Model.Name, true);
		}

		#endregion

		#region Comparisons

		/// <summary>Scores the drives by type so that some are ordered before others.</summary>
		private static int GetDriveTypeScore(DriveType type) {
			switch (type) {
			case DriveType.Fixed:			return 0;
			case DriveType.Removable:		return 1;
			case DriveType.CDRom:			return 2;
			case DriveType.Network:			return 3;
			case DriveType.NoRootDirectory:	return 4;
			case DriveType.Ram:				return 5;
			default:						return 6;
			}
		}

		private static int SortByNameAndDriveType(DriveItemViewModel a, DriveItemViewModel b) {
			int diff = GetDriveTypeScore(a.Model.DriveType) - GetDriveTypeScore(b.Model.DriveType);
			if (diff == 0)
				return SortByName(a, b);
			return diff;
		}
		private static int SortByName(DriveItemViewModel a, DriveItemViewModel b) {
			return string.Compare(a.Model.Name, b.Model.Name, true);
		}
		private static int SortByTotal(DriveItemViewModel a, DriveItemViewModel b) {
			return a.Model.TotalSize.CompareTo(b.Model.TotalSize);
		}
		private static int SortByFree(DriveItemViewModel a, DriveItemViewModel b) {
			return a.Model.FreeSpace.CompareTo(b.Model.FreeSpace);
		}
		private static int SortByPercent(DriveItemViewModel a, DriveItemViewModel b) {
			return a.Model.Percent.CompareTo(b.Model.Percent);
		}

		#endregion
	}
}
