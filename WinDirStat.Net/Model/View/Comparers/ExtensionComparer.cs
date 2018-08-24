using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Data;
using WinDirStat.Net.Model.View.Extensions;

namespace WinDirStat.Net.Model.View.Comparers {
	[Serializable]
	public enum ExtensionSortMode {
		None,
		Extension,
		Color,
		Description,
		Bytes,
		Percent,
		Files,
		[Browsable(false)]
		Count,
	}

	public class ExtensionComparer : SortComparer<ExtensionRecordViewModel, ExtensionSortMode> {

		public ExtensionComparer()
			: base(ExtensionSortMode.Bytes, ListSortDirection.Descending)
		{
		}

		protected override int SecondaryCompare(ExtensionRecordViewModel a, ExtensionRecordViewModel b) {
			return string.Compare(a.Extension, b.Extension, true);
		}

		protected override Comparison<ExtensionRecordViewModel> GetSortComparison(ExtensionSortMode mode) {
			switch (mode) {
			case ExtensionSortMode.None:
			case ExtensionSortMode.Bytes:
			case ExtensionSortMode.Percent:
			case ExtensionSortMode.Color: return SortBySize;
			case ExtensionSortMode.Extension: return SortByExtension;
			case ExtensionSortMode.Description: return SortByDescription;
			case ExtensionSortMode.Files: return SortByFiles;
			default:
				throw new ArgumentException($"Invalid {typeof(ExtensionSortMode).Name} ({mode})!", nameof(mode));
			}
		}

		private static int SortByExtension(ExtensionRecordViewModel a, ExtensionRecordViewModel b) {
			return string.Compare(a.Model.Extension, b.Model.Extension, true);
		}
		private static int SortByDescription(ExtensionRecordViewModel a, ExtensionRecordViewModel b) {
			return string.Compare(a.Name, b.Name, true);
		}
		private static int SortBySize(ExtensionRecordViewModel a, ExtensionRecordViewModel b) {
			return a.Model.Size.CompareTo(b.Model.Size);
		}
		private static int SortByFiles(ExtensionRecordViewModel a, ExtensionRecordViewModel b) {
			return a.Model.FileCount - b.Model.FileCount;
		}
	}
}
