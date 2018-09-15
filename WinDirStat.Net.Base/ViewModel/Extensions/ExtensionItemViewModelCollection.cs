using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Services;
using WinDirStat.Net.Utils;
using static WinDirStat.Net.Model.Extensions.ExtensionItem;

namespace WinDirStat.Net.ViewModel.Extensions {
	/// <summary>A collection manager for <see cref="ExtensionItemViewModel"/>s.</summary>
	public class ExtensionItemViewModelCollection
		: ObservablePropertyCollectionObject, IReadOnlyList<ExtensionItemViewModel>
	{
		#region Fields

		/// <summary>Gets the main view model that contains this collection.</summary>
		public MainViewModel ViewModel { get; }
		public ScanningService Scanning { get; }
		public ExtensionItems Model { get; }
		public IconCacheService IconCache { get; }
		/// <summary>Gets the images service.</summary>
		public ImagesServiceBase Images { get; }
		public SettingsService Settings { get; }
		public UIService UI { get; }

		/// <summary>The collection of mapped extension items.</summary>
		private readonly Dictionary<string, ExtensionItemViewModel> extensions;
		/// <summary>The list of sorted extension items.</summary>
		private readonly List<ExtensionItemViewModel> sortedExtensions;
		/// <summary>
		/// True if the file tree scanner is not open, and the collection should hide all extensions.
		/// </summary>
		private bool isNotOpen;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="ExtensionItemViewModelCollection"/>.</summary>
		/// 
		/// <param name="viewModel">The main view model containing this collection.</param>
		public ExtensionItemViewModelCollection(MainViewModel viewModel) {
			ViewModel = viewModel;
			Scanning = ViewModel.Scanning;
			Model = ViewModel.Scanning.Extensions;
			IconCache = ViewModel.IconCache;
			Images = ViewModel.Images;
			Settings = ViewModel.Settings;
			UI = ViewModel.UI;

			isNotOpen = false;
			extensions = new Dictionary<string, ExtensionItemViewModel>();
			sortedExtensions = new List<ExtensionItemViewModel>();

			ViewModel.Scanning.PropertyChanged += OnScanningPropertyChanged;
			Model.CollectionChanged += OnModelCollectionChanged;
		}

		#endregion

		#region Properties

		/// <summary>Gets the number of extensions. This value is always zero while scanning/refreshing.</summary>
		public int Count => (isNotOpen ? 0 : extensions.Count);

		/// <summary>Gets if the scanner is not open and this collection should hide its items.</summary>
		private bool IsNotOpen {
			get => isNotOpen;
			set {
				if (isNotOpen != value) {
					isNotOpen = value;
					if (extensions.Count > 0) {
						UI.Invoke(() => {
							RaisePropertyChanged(nameof(Count));
							RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
						});
					}
				}
			}
		}

		#endregion

		#region Event Handlers

		private void OnScanningPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			case nameof(ScanningService.IsOpen):
				IsNotOpen = !Scanning.IsOpen;
				break;
			}
		}

		private void OnModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			// We must be sorting, nothing to do here
			if (!IsNotOpen && e.Action == NotifyCollectionChangedAction.Reset)
				return;

			Debug.Assert(IsNotOpen, "Cannot change collection while not scanning!");

			switch (e.Action) {
			case NotifyCollectionChangedAction.Reset:
				sortedExtensions.Clear();
				for (int i = 0; i < sortedExtensions.Count; i++) {
					ExtensionItemViewModel oldItemViewModel = sortedExtensions[i];
					oldItemViewModel.Dispose();
				}
				extensions.Clear();
				for (int i = 0; i < Model.Count; i++) {
					ExtensionItem newItem = Model[i];
					ExtensionItemViewModel newItemViewModel = new ExtensionItemViewModel(this, newItem);
					sortedExtensions.Add(newItemViewModel);
					extensions.Add(newItem.Extension, newItemViewModel);
				}
				break;
			case NotifyCollectionChangedAction.Add:
				for (int i = 0; i < e.NewItems.Count; i++) {
					ExtensionItem newItem = (ExtensionItem) e.NewItems[i];
					ExtensionItemViewModel newItemViewModel = new ExtensionItemViewModel(this, newItem);
					sortedExtensions.Add(newItemViewModel);
					extensions.Add(newItem.Extension, newItemViewModel);
				}
				break;
			case NotifyCollectionChangedAction.Remove:
				for (int i = 0; i < e.OldItems.Count; i++) {
					ExtensionItem oldItem = (ExtensionItem) e.OldItems[i];
					ExtensionItemViewModel oldItemViewModel = extensions[oldItem.Extension];
					oldItemViewModel.Dispose();
					sortedExtensions.Remove(oldItemViewModel);
					extensions.Remove(oldItem.Extension);
				}
				break;
			}
		}

		#endregion

		#region Sort

		/// <summary>Sorts the collection based on the comparison.</summary>
		/// 
		/// <param name="comparison">The comparison to sort with.</param>
		public void Sort(Comparison<ExtensionItemViewModel> comparison) {
			sortedExtensions.Sort(comparison);
			RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
		}

		#endregion

		#region Accessors

		/// <summary>Gets the extension item at the specified index in the collection.</summary>
		public ExtensionItemViewModel this[int index] {
			get {
				if (IsNotOpen)
					throw new IndexOutOfRangeException(nameof(index));
				return sortedExtensions[index];
			}
		}

		/// <summary>Gets the extension item in the collection with the specified extension.</summary>
		public ExtensionItemViewModel this[string extension] {
			get => extensions[NormalizeExtension(extension)];
		}

		/// <summary>Gets if the collection contains the extension.</summary>
		/// 
		/// <param name="extension">The extension to check for.</param>
		/// <returns>True if the collection contains the extension.</returns>
		public bool ContainsExtension(string extension) {
			return extensions.ContainsKey(NormalizeExtension(extension));
		}

		/// <summary>Tries to get the extension item associated with the extension.</summary>
		/// 
		/// <param name="extension">The extension of the item to look for.</param>
		/// <param name="item">The output item result.</param>
		/// <returns>True if the extension was found.</returns>
		public bool TryGetItem(string extension, out ExtensionItemViewModel item) {
			return extensions.TryGetValue(NormalizeExtension(extension), out item);
		}

		#endregion

		#region IEnumeratable Implementation

		/// <summary>Gets the enumerator for the sorted extension list.</summary>
		public IEnumerator<ExtensionItemViewModel> GetEnumerator() {
			if (IsNotOpen)
				return Enumerator.Empty<ExtensionItemViewModel>();
			return sortedExtensions.GetEnumerator();
		}

		/// <summary>Gets the enumerator for the sorted extension list.</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			if (IsNotOpen)
				return Enumerator.Empty<ExtensionItemViewModel>();
			return sortedExtensions.GetEnumerator();
		}

		#endregion
	}
}
