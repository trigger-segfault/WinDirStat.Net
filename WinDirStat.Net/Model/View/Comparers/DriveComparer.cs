using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.View.Drives;

namespace WinDirStat.Net.Model.View.Comparers {
	[Serializable]
	public enum DriveSortMode {
		None,
		Name,
		Total,
		Free,
		UsedTotalBar,
		UsedTotalPercent,
	}

	public class DriveComparer : SortComparer<DriveViewModel, DriveSortMode> {
		
		public DriveComparer()
			: base(DriveSortMode.Name, ListSortDirection.Ascending)
		{
		}

		protected override Comparison<DriveViewModel> GetSortComparison(DriveSortMode mode) {
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

		protected override int SecondaryCompare(DriveViewModel a, DriveViewModel b) {
			return string.Compare(a.Name, b.Name, true);
		}

		private static int GetDriveTypeScore(DriveType type) {
			switch (type) {
			case DriveType.Fixed: return 0;
			case DriveType.Removable: return 1;
			case DriveType.CDRom: return 2;
			case DriveType.Network: return 3;
			case DriveType.NoRootDirectory: return 4;
			case DriveType.Ram: return 5;
			default: return 6;
			}
		}
		private static int SortByNameAndDriveType(DriveViewModel a, DriveViewModel b) {
			int diff = GetDriveTypeScore(a.Type) - GetDriveTypeScore(b.Type);
			if (diff == 0)
				return SortByName(a, b);
			return diff;
		}
		private static int SortByName(DriveViewModel a, DriveViewModel b) {
			return string.Compare(a.Name, b.Name, true);
		}
		private static int SortByTotal(DriveViewModel a, DriveViewModel b) {
			return a.Total.CompareTo(b.Total);
		}
		private static int SortByFree(DriveViewModel a, DriveViewModel b) {
			return a.Free.CompareTo(b.Free);
		}
		private static int SortByPercent(DriveViewModel a, DriveViewModel b) {
			return a.Percent.CompareTo(b.Percent);
		}
	}
}
