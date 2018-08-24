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
using WinDirStat.Net.Model.Data;
using WinDirStat.Net.Settings.Geometry;

namespace WinDirStat.Net.Model.Data.Extensions {
	[Serializable]
	public class ExtensionRecords : ObservableCollectionObject, IReadOnlyList<ExtensionRecord>,
		IReadOnlyDictionary<string, ExtensionRecord>
	{
		
		private readonly WinDirStatModel model;
		private readonly List<ExtensionRecord> sortedRecords;
		private readonly Dictionary<string, ExtensionRecord> records;
		private long totalSize;

		public ExtensionRecords(WinDirStatModel model) {
			this.model = model;
			sortedRecords = new List<ExtensionRecord>();
			records = new Dictionary<string, ExtensionRecord>(StringComparer.OrdinalIgnoreCase);
		}

		public static string FixExtension(string extension) {
			if (!extension.StartsWith("."))
				return "." + extension;
			return extension;
		}

		public void Clear() {
			ThrowOnReentrancy();
			List<ExtensionRecord> oldRecords = sortedRecords.GetRange(0, sortedRecords.Count);
			records.Clear();
			sortedRecords.Clear();
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldRecords, 0));
		}

		public ExtensionRecord Include(string extension, long size) {
			return IncludeMany(extension, size, 1);
		}

		public ExtensionRecord IncludeMany(string extension, long size, int fileCount) {
			extension = FixExtension(extension);
			if (size < 0)
				throw new ArgumentOutOfRangeException(nameof(size));
			if (fileCount < 0)
				throw new ArgumentOutOfRangeException(nameof(fileCount));
			if (fileCount == 0)
				return null;

			ExtensionRecord record = GetOrCreate(extension, out bool created);
			totalSize += size;
			record.Size += size;
			record.FileCount += fileCount;
			if (created) {
				// record.Extension already has ToLower() applied
				records.Add(record.Extension, record);
				sortedRecords.Add(record);
				RaisePropertyChanged(nameof(Count));
				RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, record));
			}
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
				RaisePropertyChanged(nameof(Count));
				RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, record));
			}
			return removed;
		}

		public void Refresh(string extension, long sizeDifference) {
			extension = FixExtension(extension);
			ExtensionRecord record = records[extension];
			record.Size += sizeDifference;
		}

		internal void FinalValidation(Comparison<ExtensionRecord> comparison) {
			sortedRecords.Sort(comparison);
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		internal void Sort(Comparison<ExtensionRecord> comparison) {
			sortedRecords.Sort(comparison);
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public WinDirStatModel Model {
			get => model;
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


		public ReadOnlyCollection<ExtensionRecord> Records {
			get => sortedRecords.AsReadOnly();
		}

		public IEnumerable<string> Extensions {
			get => sortedRecords.Select(r => r.Extension);
		}

		public bool ContainsKey(string extension) {
			return records.ContainsKey(FixExtension(extension));
		}

		public bool TryGetValue(string extension, out ExtensionRecord record) {
			return records.TryGetValue(FixExtension(extension), out record);
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
	}
}
