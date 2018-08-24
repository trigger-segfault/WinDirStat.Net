using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WinDirStat.Net.Controls;
using WinDirStat.Net.Settings.Geometry;

namespace WinDirStat.Net.Model.Data.Extensions {
	[Serializable]
	public class ExtensionRecord : INotifyPropertyChanged, IComparable<ExtensionRecord>, IComparable {
		public static readonly ExtensionRecord Empty = new ExtensionRecord();

		private readonly ExtensionRecords records;
		private readonly string extension;
		private Rgb24Color color;
		private long size;
		private int fileCount;

		public event ExtensionRecordEventHandler Changed;

		private void RaiseChanged(ExtensionRecordEventArgs e) {
			Changed?.Invoke(this, e);
		}
		private void RaiseChanged(ExtensionRecordAction action) {
			Changed?.Invoke(this, new ExtensionRecordEventArgs(action));
		}

		public object GetView() {
			ExtensionRecordEventArgs e = new ExtensionRecordEventArgs(ExtensionRecordAction.GetView);
			RaiseChanged(e);
			return e.View;
		}

		public TView GetView<TView>() {
			ExtensionRecordEventArgs e = new ExtensionRecordEventArgs(ExtensionRecordAction.GetView);
			RaiseChanged(e);
			return (TView) e.View;
		}

		private ExtensionRecord() {
			extension = "";
			//name = "Not a File";
		}

		public ExtensionRecord(ExtensionRecords records, string extension) {
			this.records = records;
			this.extension = extension.ToLower();
			color = new Rgb24Color(150, 150, 150);
			/*if (IsEmptyExtension)
				name = "File";
			else
				name = extension.TrimStart('.').ToUpper() + " File";*/
		}

		public string Extension {
			get => extension;
		}

		/*public string Name {
			get => name;
			internal set {
				if (name != value) {
					name = value;
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

		public ImageSource Preview {
			get => preview;
			internal set {
				if (preview != value) {
					preview = value;
					AutoRaisePropertyChanged();
				}
			}
		}*/

		public Rgb24Color Color {
			get => color;
			set {
				//if (color != value) {
				color = value;
				AutoRaisePropertyChanged();
				//}
			}
		}
		public long Size {
			get => size;
			set {
				if (size != value) {
					size = value;
					AutoRaisePropertyChanged();
					RaisePropertyChanged(nameof(Percent));
				}
			}
		}

		public double Percent {
			get => (double) size / records.TotalSize;
		}

		public int FileCount {
			get => fileCount;
			set {
				if (fileCount != value) {
					fileCount = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public bool IsEmptyExtension {
			get => extension == ".";
		}

		public event PropertyChangedEventHandler PropertyChanged;

		internal void RaisePropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void AutoRaisePropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public int CompareTo(ExtensionRecord other) {
			int diff = other.size.CompareTo(size);
			if (diff == 0)
				return string.Compare(extension, other.extension, true);
			return diff;
		}

		int IComparable.CompareTo(object obj) {
			return CompareTo((ExtensionRecord) obj);
		}

		public override string ToString() {
			return extension;
		}
	}
}
