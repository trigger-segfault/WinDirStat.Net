using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.View.Drives {
	public class DriveCollection : ObservableCollectionObject, IReadOnlyList<DriveViewModel> {

		private readonly DriveSelectViewModel viewModel;
		private readonly List<DriveViewModel> drives;

		public DriveCollection(DriveSelectViewModel viewModel) {
			this.viewModel = viewModel;
			drives = new List<DriveViewModel>();
		}

		public void Refresh(Comparison<DriveViewModel> comparison) {
			drives.Clear();
			DriveInfo[] driveInfos = DriveInfo.GetDrives();
			for (int i = 0; i < driveInfos.Length; i++) {
				DriveInfo driveInfo = driveInfos[i];
				if (driveInfo.IsReady) {
					drives.Add(new DriveViewModel(viewModel, driveInfo));
				}
			}
			drives.Sort(comparison);
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public void Sort(Comparison<DriveViewModel> comparison) {
			drives.Sort(comparison);
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public IReadOnlyList<DriveViewModel> Drives {
			get => drives.AsReadOnly();
		}

		public IEnumerator<DriveViewModel> GetEnumerator() {
			return drives.GetEnumerator();
		}

		public DriveViewModel this[int index] {
			get => drives[index];
		}

		public int Count {
			get => drives.Count;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
