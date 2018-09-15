using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services;
using WinDirStat.Net.Utils;
using static WinDirStat.Net.Model.Extensions.ExtensionItem;

namespace WinDirStat.Net.Model.Extensions {
	/// <summary>
	/// A collection that maintains information about all extensions encountered while scanning the file
	/// tree.
	/// </summary>
	public class ExtensionItems : ObservablePropertyCollectionObject, IReadOnlyList<ExtensionItem> {

		#region Fields

		/// <summary>The scanning service that contains this collection.</summary>
		private readonly ScanningService scanning;
		/// <summary>The program settings service.</summary>
		private readonly SettingsService settings;
		/// <summary>The map of in-use extensions.</summary>
		private readonly Dictionary<string, ExtensionItem> extensions;
		/// <summary>The map of unused extensions.</summary>
		private readonly Dictionary<string, ExtensionItem> unusedExtensions;
		/// <summary>The list of extensions sorted by size descending after the scan.</summary>
		private readonly List<ExtensionItem> sortedExtensions;

		/// <summary>the Total size of all files in the file tree.</summary>
		private long totalSize;
		/// <summary>The total number of files in the file tree.</summary>
		private long totalFileCount;
		
		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="ExtensionItems"/> list.</summary>
		public ExtensionItems(ScanningService scanning,
							  SettingsService settings)
		{
			this.scanning = scanning;
			this.settings = settings;
			extensions = new Dictionary<string, ExtensionItem>();
			unusedExtensions = new Dictionary<string, ExtensionItem>();
			sortedExtensions = new List<ExtensionItem>();

			scanning.PropertyChanged += OnScanningPropertyChanged;
			settings.PropertyChanged += OnSettingsPropertyChanged;
		}

		#endregion

		#region Event Handlers

		private void OnScanningPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			case nameof(ScanningService.ProgressState):
				if (scanning.ProgressState == ScanProgressState.Ended && !scanning.IsRefreshing)
					Validate();
				break;
			case nameof(ScanningService.IsRefreshing):
				if (!scanning.IsScanning)
					Validate();
				break;
			}
		}

		private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			case nameof(SettingsService.FilePalette):
				if (!scanning.IsScanning && !scanning.IsRefreshing)
					RefreshPalette();
				break;
			}
		}

		#endregion

		#region Properties

		/// <summary>Gets the total size of all files in the file tree.</summary>
		public long TotalSize {
			get => totalSize;
			internal set => Set(ref totalSize, value);
		}
		/// <summary>Gets the total number of files in the file tree.</summary>
		public long TotalFileCount {
			get => totalFileCount;
			internal set => Set(ref totalFileCount, value);
		}

		/// <summary>Gets the number of extensions in the file tree.</summary>
		public int Count => sortedExtensions.Count;
		/// <summary>Gets the number of unused extensions in the file tree.</summary>
		public int UnusedCount => unusedExtensions.Count;

		/// <summary>Gets the extension item at the specified index (in the list ordered by size).</summary>
		public ExtensionItem this[int index] => sortedExtensions[index];
		/// <summary>Gets the existing extension item of the specified extension.</summary>
		public ExtensionItem this[string extension] => extensions[NormalizeExtension(extension)];

		#endregion
		
		#region Collection

		/// <summary>Gets the existing extension item or creates one and adds it to the list.</summary>
		/// 
		/// <param name="extension">The extension to get or add an item for.</param>
		/// <returns>The extension item with the specified extension.</returns>
		public ExtensionItem GetOrAdd(string extension) {
			NormalizeExtension(ref extension);
			if (!extensions.TryGetValue(extension, out ExtensionItem item)) {
				bool unusedFound = unusedExtensions.TryGetValue(extension, out item);
				if (unusedFound)
					unusedExtensions.Remove(extension);
				else
					item = new ExtensionItem(this, extension);
				Debug.Assert(item != null);
				extensions.Add(extension, item);
				sortedExtensions.Add(item);
				if (unusedFound)
					RaisePropertyChanged(nameof(UnusedCount));
				RaisePropertyChanged(nameof(Count));
				RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item, sortedExtensions.Count - 1);
			}
			return item;
		}

		/// <summary>Gets the existing extension item or creates one and adds it to the list.</summary>
		/// 
		/// <param name="path">The path to get the extension from.</param>
		/// <returns>The extension item with the specified extension.</returns>
		public ExtensionItem GetOrAddFromPath(string path) {
			return GetOrAdd(GetAndNormalizeExtension(path));
		}

		/// <summary>Removes the extension item from the list.</summary>
		/// 
		/// <param name="item">The extension item to remove</param>
		/// <returns>True if the extension item was contained in the list and removed.</returns>
		internal bool Remove(ExtensionItem item) {
			if (extensions.Remove(item.Extension)) {
				int index = sortedExtensions.IndexOf(item);
				sortedExtensions.RemoveAt(index);
				unusedExtensions.Add(item.Extension, item);
				RaisePropertyChanged(nameof(Count));
				RaisePropertyChanged(nameof(UnusedCount));
				RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
				return true;
			}
			return false;
		}

		/// <summary>Removes the specified item with the specified extension from the list.</summary>
		/// 
		/// <param name="extension">The extension of the item to remove</param>
		/// <returns>True if the extension was contained in the list and removed.</returns>
		internal bool Remove(string extension) {
			NormalizeExtension(ref extension);
			if (extensions.TryGetValue(extension, out ExtensionItem item)) {
				unusedExtensions.Add(extension, item);
				extensions.Remove(extension);
				int index = sortedExtensions.IndexOf(item);
				sortedExtensions.RemoveAt(index);
				RaisePropertyChanged(nameof(Count));
				RaisePropertyChanged(nameof(UnusedCount));
				RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
				return true;
			}
			return false;
		}

		/// <summary>Removes all extensions from the list.</summary>
		internal void Clear() {
			List<ExtensionItem> oldItems = sortedExtensions.GetFullRange();
			sortedExtensions.Clear();
			foreach (ExtensionItem item in oldItems) {
				item.ClearFilesMinimal();
				unusedExtensions.Add(item.Extension, item);
			}
			extensions.Clear();
			TotalSize = 0;
			TotalFileCount = 0;
			RaisePropertyChanged(nameof(Count));
			RaisePropertyChanged(nameof(UnusedCount));
			RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, oldItems, 0);
		}

		#endregion

		#region Validation

		/// <summary>Validates the extensions and sorts them in size descending order.</summary>
		private void Validate() {
			sortedExtensions.Sort();
			RefreshPalette();
			//RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
		}

		/// <summary>Refreshes the extensions file palette.</summary>
		private void RefreshPalette() {
			int count = sortedExtensions.Count;
			for (int i = 0; i < count; i++) {
				sortedExtensions[i].SetColor(settings.GetFilePaletteColor(i), i);
			}
		}

		#endregion
		
		#region IEnumerator Implementation

		/// <summary>Gets the enumerator for the extension items.</summary>
		IEnumerator<ExtensionItem> IEnumerable<ExtensionItem>.GetEnumerator() {
			return sortedExtensions.GetEnumerator();
		}
		/// <summary>Gets the enumerator for the extension items.</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return sortedExtensions.GetEnumerator();
		}

		#endregion
	}
}
