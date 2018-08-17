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
using WinDirStat.Net.Data.Nodes;
using WinDirStat.Net.Settings.Geometry;

namespace WinDirStat.Net.Data {
	[Serializable]
	public class ExtensionRecord : INotifyPropertyChanged {
		public static readonly ExtensionRecord Empty = new ExtensionRecord();

		private readonly ExtensionRecords records;
		internal readonly List<FolderNode> containers = new List<FolderNode>();
		private readonly string extension;
		private string name;
		private Rgb24Color color;
		private ImageSource icon;
		private ImageSource preview;
		private long size;
		private int fileCount;

		private ExtensionRecord() {
			extension = "";
		}

		public ExtensionRecord(ExtensionRecords records, string extension) {
			this.records = records;
			this.extension = extension.ToLower();
			color = new Rgb24Color(150, 150, 150);
			if (IsEmptyExtension)
				name = "File";
			else
				name = extension.TrimStart('.').ToUpper() + " File";
		}

		public List<FolderNode> Containers {
			get => containers;
		}

		public string Extension {
			get => extension;
		}

		public string Name {
			get => name;
			set {
				if (name != value) {
					name = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public ImageSource Icon {
			get => icon;
			set {
				if (icon != value) {
					icon = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public ImageSource Preview {
			get => preview;
			set {
				if (preview != value) {
					preview = value;
					AutoRaisePropertyChanged();
				}
			}
		}

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
	}

	[Serializable]
	public class ExtensionRecords : IReadOnlyList<ExtensionRecord>, IReadOnlyDictionary<string, ExtensionRecord>, INotifyCollectionChanged {

		private readonly WinDirDocument document;
		private readonly List<ExtensionRecord> sortedRecords;
		private readonly IReadOnlyList<ExtensionRecord> readonlySortedRecords;
		private readonly Dictionary<string, ExtensionRecord> records;
		private readonly IReadOnlyDictionary<string, ExtensionRecord> readonlyRecords;
		private bool isRaisingEvent;
		private long totalSize;

		public ExtensionRecords(WinDirDocument document) {
			this.document = document;
			sortedRecords = new List<ExtensionRecord>();
			readonlySortedRecords = sortedRecords.AsReadOnly();
			records = new Dictionary<string, ExtensionRecord>(StringComparer.OrdinalIgnoreCase);
			readonlyRecords = new ReadOnlyDictionary<string, ExtensionRecord>(records);
		}

		public static string FixExtension(string extension) {
			if (!extension.StartsWith("."))
				return "." + extension;
			return extension;
		}

		public void Clear() {
			records.Clear();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public ExtensionRecord Include(string extension, long size) {
			return IncludeMany(extension, size, 1, out _);
		}

		public ExtensionRecord Include(string extension, long size, out ImageSource icon) {
			return IncludeMany(extension, size, 1, out icon);
		}

		public ExtensionRecord IncludeMany(string extension, long size, int fileCount) {
			return IncludeMany(extension, size, fileCount, out _);
		}

		public ExtensionRecord IncludeMany(string extension, long size, int fileCount, out ImageSource icon) {
			extension = FixExtension(extension);
			if (size < 0)
				throw new ArgumentOutOfRangeException(nameof(size));
			if (fileCount < 0)
				throw new ArgumentOutOfRangeException(nameof(fileCount));
			if (fileCount == 0) {
				// Nodbody must be asking for an icon
				icon = null;
				return null;
			}

			ExtensionRecord record = GetOrCreate(extension, out bool created);
			totalSize += size;
			record.Size += size;
			record.FileCount += fileCount;
			if (created) {
				// record.Extension already has ToLower() applied
				records.Add(record.Extension, record);
				sortedRecords.Add(record);
				// Use the default icon until we have something more fitting
				record.Icon = IconCache.FileIcon;
				if (!record.IsEmptyExtension) {
					Icons.CacheFileTypeAsync(extension, (ic, na) => {
						if (ic != null)
							record.Icon = ic;
						record.Name = na;
					});
				}
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, record));
			}
			icon = record.Icon;
			return record;
		}

		public bool Remove(string extension, long size) {
			return RemoveMany(extension, size, 1);
		}

		public bool RemoveMany(string extension, long size, int fileCount) {
			if (size < 0)
				throw new ArgumentOutOfRangeException(nameof(size));
			if (fileCount < 0)
				throw new ArgumentOutOfRangeException(nameof(fileCount));
			if (fileCount == 0)
				return false;
			extension = FixExtension(extension);

			ExtensionRecord record = records[extension];
			totalSize -= size;
			record.Size -= size;
			record.FileCount -= fileCount;
			bool removed = (record.FileCount == 0);
			if (removed) {
				records.Remove(extension);
				sortedRecords.Remove(record);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, record));
			}
			return removed;
		}

		public void Refresh(string extension, long sizeDifference) {
			extension = FixExtension(extension);
			ExtensionRecord record = records[extension];
			record.Size += sizeDifference;
		}

		public void FinalValidation() {
			sortedRecords.Sort(document.ExtensionComparer.Compare);
			RefreshPalette();
		}

		public void RefreshPalette() {
			var colors = document.Settings.FilePalette;
			var previews = document.Settings.FilePalettePreviews;
			int index = 0;
			foreach (ExtensionRecord record in sortedRecords) {
				record.Color = colors[index];
				record.Preview = previews[index];
				if (index < colors.Count - 1)
					index++;
			}
		}

		public void Sort() {
			ExtensionRecordComparer comparer = document.ExtensionComparer;
			sortedRecords.Sort(comparer);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		private IconCache Icons {
			get => document.Icons;
		}

		public ExtensionRecord this[int index] {
			get => sortedRecords[index];
		}

		public ExtensionRecord this[string extension] {
			get => records[FixExtension(extension)];
		}
		
		public int Count {
			get => records.Count;
		}

		public long TotalSize {
			get => totalSize;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;


		public IReadOnlyList<ExtensionRecord> Records {
			get => readonlySortedRecords;
		}

		public IEnumerable<string> Extensions {
			get => sortedRecords.Select(r => r.Extension);
		}

		public bool ContainsKey(string extention) {
			return records.ContainsKey(extention);
		}

		public bool TryGetValue(string extention, out ExtensionRecord record) {
			return records.TryGetValue(extention, out record);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return sortedRecords.GetEnumerator();
		}

		IEnumerator<ExtensionRecord> IEnumerable<ExtensionRecord>.GetEnumerator() {
			return sortedRecords.GetEnumerator();
		}

		IEnumerable<string> IReadOnlyDictionary<string, ExtensionRecord>.Keys {
			get => records.Keys;
		}

		IEnumerable<ExtensionRecord> IReadOnlyDictionary<string, ExtensionRecord>.Values {
			get => records.Values;
		}

		IEnumerator<KeyValuePair<string, ExtensionRecord>> IEnumerable<KeyValuePair<string, ExtensionRecord>>.GetEnumerator() {
			return records.GetEnumerator();
		}

		private ExtensionRecord GetOrCreate(string extension, out bool created) {
			created = !records.TryGetValue(extension, out ExtensionRecord record);
			if (created) {
				record = new ExtensionRecord(this, extension);
			}
			return record;
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
	}
}
