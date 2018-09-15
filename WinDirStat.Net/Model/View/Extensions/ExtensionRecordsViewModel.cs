using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Data;
using WinDirStat.Net.Model.Data.Extensions;
using WinDirStat.Net.Utils;
using static WinDirStat.Net.Model.Data.Extensions.ExtensionRecords;

namespace WinDirStat.Net.Model.View.Extensions {
	public class ExtensionRecordsViewModel : ObservableCollectionObject, IReadOnlyList<ExtensionRecordViewModel>,
		IReadOnlyDictionary<string, ExtensionRecordViewModel>
	{
		private readonly ExtensionRecords model;
		private readonly WinDirStatViewModel viewModel;
		private readonly List<ExtensionRecordViewModel> sortedRecords;
		private readonly Dictionary<string, ExtensionRecordViewModel> records;
		
		private bool isScanning;

		public ExtensionRecordsViewModel(WinDirStatViewModel viewModel) {
			this.viewModel = viewModel;
			model = viewModel.Model.Extensions;
			sortedRecords = new List<ExtensionRecordViewModel>();
			records = new Dictionary<string, ExtensionRecordViewModel>();
			model.CollectionChanged += OnModelCollectionChanged;
		}

		private void OnModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			Debug.Assert(isScanning, "Cannot change collection while not scanning!");
			switch (e.Action) {
			case NotifyCollectionChangedAction.Reset:
				sortedRecords.Clear();
				records.Clear();
				for (int i = 0; i < model.Count; i++) {
					ExtensionRecord newRecord = (ExtensionRecord) model[i];
					ExtensionRecordViewModel newRecordViewModel = new ExtensionRecordViewModel(this, newRecord);
					sortedRecords.Add(newRecordViewModel);
					records.Add(newRecord.Extension, newRecordViewModel);
				}
				break;
			case NotifyCollectionChangedAction.Add:
				for (int i = 0; i < e.NewItems.Count; i++) {
					ExtensionRecord newRecord = (ExtensionRecord) e.NewItems[i];
					ExtensionRecordViewModel newRecordViewModel = new ExtensionRecordViewModel(this, newRecord);
					sortedRecords.Add(newRecordViewModel);
					records.Add(newRecord.Extension, newRecordViewModel);
				}
				break;
			case NotifyCollectionChangedAction.Remove:
				for (int i = 0; i < e.OldItems.Count; i++) {
					ExtensionRecord oldRecord = (ExtensionRecord) e.OldItems[i];
					ExtensionRecordViewModel oldRecordViewModel = records[oldRecord.Extension];
					sortedRecords.Remove(oldRecordViewModel);
					records.Remove(oldRecord.Extension);
				}
				break;
			}
		}

		internal bool IsScanning {
			get => isScanning;
			set {
				if (isScanning != value) {
					isScanning = value;
					if (records.Count > 0) {
						RaisePropertyChanged(nameof(Count));
						RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					}
					AutoRaisePropertyChanged();
				}
			}
		}

		private void RefreshPalette() {
			var colors = viewModel.Settings.FilePalette;
			var previews = viewModel.Settings.FilePalettePreviews;
			int index = 0;
			foreach (ExtensionRecordViewModel record in sortedRecords.OrderBy(r => r.Model)) {
				record.Color = colors[index];
				record.Preview = previews[index];
				if (index < colors.Count - 1)
					index++;
			}
		}

		public void PrepareForDisplay(Comparison<ExtensionRecordViewModel> comparison) {
			RefreshPalette();
			Sort(comparison);
		}

		public ExtensionRecords Model {
			get => model;
		}

		public WinDirStatViewModel ViewModel {
			get => viewModel;
		}

		public IconCache Icons {
			get => viewModel.Icons;
		}

		public int Count {
			get {
				if (isScanning)
					return 0;
				return records.Count;
			}
		}

		public long TotalSize {
			get => model.TotalSize;
		}

		public void Sort(Comparison<ExtensionRecordViewModel> comparison) {
			sortedRecords.Sort(comparison);
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public ExtensionRecordViewModel this[int index] {
			get {
				if (isScanning)
					throw new InvalidOperationException();
				return sortedRecords[index];
			}
		}

		public ExtensionRecordViewModel this[string extension] {
			get {
				//if (isScanning)
				//	throw new InvalidOperationException();
				return records[FixExtension(extension)];
			}
		}

		public ReadOnlyCollection<ExtensionRecordViewModel> Records {
			get {
				if (isScanning)
					return new List<ExtensionRecordViewModel>().AsReadOnly();
				return sortedRecords.AsReadOnly();
			}
		}

		public IEnumerable<string> Extensions {
			get {
				if (isScanning)
					return Enumerable.Empty<string>();
				return sortedRecords.Select(r => r.Extension);
			}
		}

		public bool ContainsKey(string extension) {
			if (isScanning)
				return false;
			return records.ContainsKey(FixExtension(extension));
		}

		public bool TryGetValue(string extension, out ExtensionRecordViewModel record) {
			if (isScanning) {
				record = null;
				return false;
			}
			return records.TryGetValue(FixExtension(extension), out record);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			if (isScanning)
				return Enumerator.Empty<ExtensionRecordViewModel>();
			return sortedRecords.GetEnumerator();
		}

		IEnumerator<ExtensionRecordViewModel> IEnumerable<ExtensionRecordViewModel>.GetEnumerator() {
			if (isScanning)
				return Enumerator.Empty<ExtensionRecordViewModel>();
			return sortedRecords.GetEnumerator();
		}

		IEnumerable<string> IReadOnlyDictionary<string, ExtensionRecordViewModel>.Keys {
			get {
				if (isScanning)
					return Enumerable.Empty<string>();
				return records.Keys;
			}
		}

		IEnumerable<ExtensionRecordViewModel> IReadOnlyDictionary<string, ExtensionRecordViewModel>.Values {
			get {
				if (isScanning)
					return Enumerable.Empty<ExtensionRecordViewModel>();
				return records.Values;
			}
		}

		IEnumerator<KeyValuePair<string, ExtensionRecordViewModel>> IEnumerable<KeyValuePair<string, ExtensionRecordViewModel>>.GetEnumerator() {
			if (isScanning)
				return Enumerator.Empty<KeyValuePair<string, ExtensionRecordViewModel>>();
			return records.GetEnumerator();
		}
	}
}
