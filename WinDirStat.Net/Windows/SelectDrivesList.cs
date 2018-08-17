using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WinDirStat.Net.Data;

namespace WinDirStat.Net.Windows {
	public class SelectDrive : INotifyPropertyChanged {

		private string name;
		private string displayName;
		private long total;
		private long free;
		private ImageSource icon;
		private DriveType type;
		private string format;


		public SelectDrive(DriveInfo info) {
			name = info.Name;
			total = info.TotalSize;
			free = info.AvailableFreeSpace;
			type = info.DriveType;
			format = info.DriveFormat;
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
			get => displayName ?? name;
			internal set {
				if (displayName != value) {
					displayName = value;
					AutoRaisePropertyChanged();
				}
			}
		}
		public ImageSource Icon {
			get => icon;
			internal set {
				if (icon != value) {
					icon = value;
					AutoRaisePropertyChanged();
				}
			}
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

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
		private void AutoRaisePropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
	public class SelectDrivesList : IReadOnlyList<SelectDrive>, INotifyCollectionChanged {

		private readonly List<SelectDrive> drives;
		private readonly SelectDrivesDocument document;
		private bool isRaisingEvent;
		private readonly ReadOnlyCollection<SelectDrive> readOnlyDrives;

		public SelectDrivesList(SelectDrivesDocument document) {
			this.document = document;
			drives = new List<SelectDrive>();
			readOnlyDrives = drives.AsReadOnly();
		}

		public void Refresh() {
			drives.Clear();
			DriveInfo[] driveInfos = DriveInfo.GetDrives();
			for (int i = 0; i < driveInfos.Length; i++) {
				DriveInfo driveInfo = driveInfos[i];
				if (driveInfo.IsReady) {
					SelectDrive drive = new SelectDrive(driveInfo);
					drive.Icon = document.Icons.CacheIcon(drive.Name, 0, out string name);
					if (drive.Icon == null)
						drive.Icon = IconCache.VolumeIcon;
					else
						drive.DisplayName = name;
					drives.Add(drive);
				}
			}
			drives.Sort(document.DriveComparer.Compare);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public void Sort() {
			drives.Sort(document.DriveComparer.Compare);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;


		public IReadOnlyList<SelectDrive> Drives {
			get => readOnlyDrives;
		}

		public IEnumerator<SelectDrive> GetEnumerator() {
			return drives.GetEnumerator();
		}

		private void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
			Debug.Assert(!isRaisingEvent);
			isRaisingEvent = true;
			try {
				CollectionChanged?.Invoke(this, e);
			}
			finally {
				isRaisingEvent = false;
			}
		}

		private void ThrowOnReentrancy() {
			if (isRaisingEvent)
				throw new InvalidOperationException();
		}

		public SelectDrive this[int index] {
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
