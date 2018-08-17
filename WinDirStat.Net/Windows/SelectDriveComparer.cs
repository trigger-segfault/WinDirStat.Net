using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace WinDirStat.Net.Windows {
	public enum SelectDriveSortMethod {
		None,
		Name,
		Total,
		Free,
		UsedTotalBar,
		UsedTotalPercent,
	}
	public class SelectDriveComparer : IComparer<SelectDrive> {
		public static readonly SelectDriveComparer Default = new SelectDriveComparer();

		private delegate int SelectDriveSortComparer(SelectDrive a, SelectDrive b);
		private SelectDriveSortMethod sortMethod;
		private ListSortDirection sortDirection;
		private SelectDriveSortComparer sortComparer;

		public SelectDriveComparer() {
			sortMethod = SelectDriveSortMethod.Name;
			sortDirection = ListSortDirection.Ascending;
			sortComparer = SortByNameAndDriveType;
		}

		public SelectDriveSortMethod SortMethod {
			get => sortMethod;
			set {
				if (sortMethod != value) {
					sortMethod = value;
					sortComparer = GetSortComparer(sortMethod);
				}
			}
		}
		public ListSortDirection SortDirection {
			get => sortDirection;
			set => sortDirection = value;
		}

		public int Compare(SelectDrive a, SelectDrive b) {
			int diff = sortComparer(a, b);
			// Always sort alphabetically after initial sort
			if (diff == 0)
				return SortByName(a, b);
			if (SortDirection == ListSortDirection.Ascending)
				return diff;
			else
				return -diff;
		}

		public bool ShouldSort(SelectDrive a, SelectDrive b) {
			int diff = sortComparer(a, b);
			// Always sort alphabetically after initial sort
			if (diff == 0)
				return SortByName(a, b) < 0;
			if (SortDirection == ListSortDirection.Ascending)
				return diff < 0;
			else
				return diff > 0;
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

		private static SelectDriveSortComparer GetSortComparer(SelectDriveSortMethod method) {
			switch (method) {
			case SelectDriveSortMethod.None:
			case SelectDriveSortMethod.Name:
				return SortByNameAndDriveType;
			case SelectDriveSortMethod.Total:
				return SortByTotal;
			case SelectDriveSortMethod.Free:
				return SortByFree;
			case SelectDriveSortMethod.UsedTotalBar:
			case SelectDriveSortMethod.UsedTotalPercent:
				return SortByPercent;
			default:
				throw new ArgumentException($"Invalid {typeof(SelectDriveSortMethod).Name} ({method})!", nameof(method));
			}
		}
		
		private static int SortByNameAndDriveType(SelectDrive a, SelectDrive b) {
			int diff = GetDriveTypeScore(a.Type) - GetDriveTypeScore(b.Type);
			if (diff == 0)
				return SortByName(a, b);
			return diff;
		}
		private static int SortByName(SelectDrive a, SelectDrive b) {
			return string.Compare(a.Name, b.Name, true);
		}
		private static int SortByTotal(SelectDrive a, SelectDrive b) {
			return a.Total.CompareTo(b.Total);
		}
		private static int SortByFree(SelectDrive a, SelectDrive b) {
			return a.Free.CompareTo(b.Free);
		}
		private static int SortByPercent(SelectDrive a, SelectDrive b) {
			return a.Percent.CompareTo(b.Percent);
		}
	}
}
