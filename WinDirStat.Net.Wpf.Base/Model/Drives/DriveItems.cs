using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using WinDirStat.Net.Services;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Model.Drives {
    /// <summary>A collection of see <see cref="DriveItem"/>s.</summary>
    public class DriveItems : ObservablePropertyCollectionObject, IReadOnlyList<DriveItem> {

		#region Fields

		/// <summary>The scanning service that contains this collection.</summary>
		private readonly ScanningService scanning;
		/// <summary>The collection of drives.</summary>
		private readonly List<DriveItem> drives;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="DriveItems"/> list.</summary>
		public DriveItems(ScanningService scanning) {
			this.scanning = scanning;
			drives = new List<DriveItem>();
		}

		#endregion

		#region Refresh

		/// <summary>Refreshes the drive list.</summary>
		public void Refresh() {
			if (drives.Count > 0) {
				List<DriveItem> oldItems = drives.GetFullRange();
				drives.Clear();
				OnPropertyChanged(nameof(Count));
				RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, oldItems, 0);
			}
			drives.AddRange(scanning.ScanDrives());
			drives.Sort();
			RaiseCollectionChanged(NotifyCollectionChangedAction.Add, drives.GetFullRange(), 0);
		}

		#endregion

		#region Properties

		/// <summary>Gets the number of drives in the list.</summary>
		public int Count => drives.Count;

		/// <summary>Gets the drive item at the specified index in the list.</summary>
		public DriveItem this[int index] => drives[index];

		#endregion

		#region IEnumerator Implementation

		/// <summary>Gets the enumerator for the drive items.</summary>
		IEnumerator<DriveItem> IEnumerable<DriveItem>.GetEnumerator() {
			return drives.GetEnumerator();
		}
		/// <summary>Gets the enumerator for the drive items.</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return drives.GetEnumerator();
		}

		#endregion
	}
}
