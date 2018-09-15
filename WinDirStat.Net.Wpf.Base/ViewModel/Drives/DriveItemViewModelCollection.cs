using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.ViewModel.Drives {
	/// <summary>A collection manager for <see cref="DriveItemViewModel"/>s.</summary>
	public class DriveItemViewModelCollection
		: ObservablePropertyCollectionObject, IReadOnlyList<DriveItemViewModel>
	{
		#region Fields

		/// <summary>Gets the drive select view model that contains this collection.</summary>
		public DriveSelectViewModel ViewModel { get; }
		/// <summary>Gets the model collection that this view model represents.</summary>
		public DriveItems Model { get; }

		// Services
		/// <summary>Gets the scanning service.</summary>
		public ScanningService Scanning { get; }
		/// <summary>Gets the icon cache service.</summary>
		public IconCacheService IconCache { get; }
		/// <summary>Gets the program settings service.</summary>
		public SettingsService Settings { get; }
		/// <summary>Gets the UI service.</summary>
		public UIService UI { get; }
		
		/// <summary>The list of sorted drive items.</summary>
		private readonly List<DriveItemViewModel> drives;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="DriveItemViewModelCollection"/>.</summary>
		/// 
		/// <param name="viewModel">The main view model containing this collection.</param>
		public DriveItemViewModelCollection(DriveSelectViewModel viewModel) {
			ViewModel = viewModel;
			Scanning = ViewModel.Scanning;
			Model = Scanning.Drives;
			IconCache = ViewModel.IconCache;
			Settings = ViewModel.Settings;
			UI = ViewModel.UI;
			
			drives = new List<DriveItemViewModel>();

			Model.CollectionChanged += OnModelCollectionChanged;
		}

		#endregion

		#region Event Handlers

		private void OnModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			UI.Invoke(() => {
				switch (e.Action) {
				case NotifyCollectionChangedAction.Reset:
					drives.Clear();
					for (int i = 0; i < Model.Count; i++) {
						DriveItem newItem = Model[i];
						DriveItemViewModel newItemViewModel = new DriveItemViewModel(this, newItem);
						drives.Add(newItemViewModel);
					}
					Sort(ViewModel.DriveComparer.Compare);
					//RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
					break;
				case NotifyCollectionChangedAction.Add:
					//List<DriveItemViewModel> newItemViewModels = new List<DriveItemViewModel>();
					for (int i = 0; i < e.NewItems.Count; i++) {
						DriveItem newItem = (DriveItem) e.NewItems[i];
						DriveItemViewModel newItemViewModel = new DriveItemViewModel(this, newItem);
						drives.Add(newItemViewModel);
						//newItemViewModels.Add(newItemViewModel);
					}
					Sort(ViewModel.DriveComparer.Compare);
					//RaiseCollectionChanged(NotifyCollectionChangedAction.Add, newItemViewModels, e.NewStartingIndex);
					break;
				case NotifyCollectionChangedAction.Remove:
					//List<DriveItemViewModel> oldItemViewModels = new List<DriveItemViewModel>();
					for (int i = 0; i < e.OldItems.Count; i++) {
						DriveItem oldItem = (DriveItem) e.OldItems[i];
						DriveItemViewModel oldItemViewModel = drives.Find(d => d.Model == oldItem);
						//oldItemViewModel.Dispose();
						drives.Remove(oldItemViewModel);
						//oldItemViewModels.Add(oldItemViewModel);
					}
					Sort(ViewModel.DriveComparer.Compare);
					//RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, oldItemViewModels, e.OldStartingIndex);
					break;
				}
			});
		}

		#endregion

		#region Properties

		/// <summary>Gets the number of drives.</summary>
		public int Count => drives.Count;
		
		/// <summary>Gets the drive item at the specified index in the collection.</summary>
		public DriveItemViewModel this[int index] => drives[index];

		#endregion

		#region Sort

		/// <summary>Sorts the collection based on the comparison.</summary>
		/// 
		/// <param name="comparison">The comparison to sort with.</param>
		public void Sort(Comparison<DriveItemViewModel> comparison) {
			drives.Sort(comparison);
			RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
		}

		#endregion

		#region IEnumerator Implementation

		/// <summary>Gets the enumerator for the drive items.</summary>
		IEnumerator<DriveItemViewModel> IEnumerable<DriveItemViewModel>.GetEnumerator() {
			return drives.GetEnumerator();
		}
		/// <summary>Gets the enumerator for the drive items.</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return drives.GetEnumerator();
		}

		#endregion
	}
}
