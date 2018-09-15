using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.ViewModel.Files {
	/// <summary>The internal modifiable <see cref="FileItemViewModel"/> collection.</summary>
	internal class FileItemViewModelCollection
		: ObservableCollectionObject, IList<FileItemViewModel>, IReadOnlyFileItemViewModelCollection
	{
		#region Fields

		/// <summary>The owner of this collection.</summary>
		private readonly FileItemViewModel parent;
		/// <summary>The collection of file item view models.</summary>
		private readonly List<FileItemViewModel> list = new List<FileItemViewModel>();

		#endregion

		#region Constructors

		/// <summary>Constructs a <see cref="FileItemViewModelCollection"/> for empty use.</summary>
		public FileItemViewModelCollection() {
			// Empty collection for readonly use only
		}

		/// <summary>Constructs a <see cref="FileItemViewModelCollection"/> for normal use.</summary>
		/// 
		/// <param name="parent">The owner of this collection.</param>
		public FileItemViewModelCollection(FileItemViewModel parent) {
			this.parent = parent;
		}

		#endregion

		#region RaiseCollectionReset

		/// <summary>
		/// Raises a special collection reset event, which allows the <see cref="FileTreeFlattener"/> to
		/// properly make changes to the list.
		/// </summary>
		/// 
		/// <param name="list">The new current list of the collection.</param>
		/// <param name="oldList">The old list of the collection.</param>
		private void RaiseCollectionReset(List<FileItemViewModel> list, List<FileItemViewModel> oldList) {
			Debug.Assert(!IsRaisingEvent);
			IsRaisingEvent = true;
			try {
				parent.OnChildrenReset(list, oldList);
				InvokeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
			finally {
				IsRaisingEvent = false;
			}
		}

		/// <summary>Raises a new <see cref="CollectionChanged"/> event.</summary>
		/// 
		/// <param name="e">The arguments for the event.</param>
		protected override void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e) {
			//Debug.Assert(!isRaisingEvent);
			ThrowOnReentrancy();
			IsRaisingEvent = true;
			try {
				parent.OnChildrenChanged(e);
				InvokeCollectionChanged(e);
			}
			finally {
				IsRaisingEvent = false;
			}
		}

		#endregion

		#region Throw Helpers

		/// <summary>Throws an exception if the item is not valid for adding to the collection.</summary>
		/// 
		/// <param name="item">The item to check.</param>
		private void ThrowIfValueIsNullOrHasParent(FileItemViewModel item) {
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			if (item.Parent != null)
				throw new ArgumentException("The item already has a parent", nameof(item));
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets the item at the specified index in the collection.</summary>
		public FileItemViewModel this[int index] {
			get => list[index];
			set {
				ThrowOnReentrancy();
				var oldItem = list[index];
				if (oldItem == value)
					return;
				ThrowIfValueIsNullOrHasParent(value);
				list[index] = value;
				RaiseCollectionChanged(NotifyCollectionChangedAction.Replace, value, oldItem, index);
			}
		}

		/// <summary>Gets the number of items in the collection.</summary>
		public int Count => list.Count;

		#endregion

		#region Sorting

		/// <summary>Sorts the collection based on the comparison.</summary>
		/// 
		/// <param name="comparison">The comparison to sort with.</param>
		public void Sort(Comparison<FileItemViewModel> comparison) {
			if (list.Count > 0) {
				List<FileItemViewModel> oldList = list.GetFullRange();
				list.Sort(comparison);
				RaiseCollectionReset(list, oldList);
			}
		}

		#endregion

		#region Accessors

		/// <summary>Checks if the collection contains the specified view model item.</summary>
		/// 
		/// <param name="item">The view model item to check for.</param>
		/// <returns>True if the collection contains the item.</returns>
		public bool Contains(FileItemViewModel item) {
			return IndexOf(item) >= 0;
		}

		/// <summary>Checks if the collection contains the specified model item.</summary>
		/// 
		/// <param name="item">The model item to check for.</param>
		/// <returns>True if the collection contains the item.</returns>
		public bool Contains(FileItem item) {
			return IndexOf(item) >= 0;
		}

		/// <summary>Gets the index of the specified view model item in the collection.</summary>
		/// 
		/// <param name="item">The item to get the index of.</param>
		/// <returns>The index of the item if it exists in the collection, otherwise -1.</returns>
		public int IndexOf(FileItemViewModel item) {
			if (item == null || item.Parent != parent)
				return -1;
			else
				return list.IndexOf(item);
		}

		/// <summary>Gets the index of the specified model item in the collection.</summary>
		/// 
		/// <param name="item">The item to get the index of.</param>
		/// <returns>The index of the item if it exists in the collection, otherwise -1.</returns>
		public int IndexOf(FileItemBase item) {
			if (item == null)
				return -1;
			else
				return list.FindIndex(n => n.Model == item);
		}

		/// <summary>Finds the view model item that contains this model item.</summary>
		/// 
		/// <param name="item">The model item to look for.</param>
		/// <returns>The view model item that contains this model item, null if none was found.</returns>
		public FileItemViewModel Find(FileItemBase item) {
			return list.Find(v => v.Model == item);
		}

		/// <summary>Converts the collection to an array.</summary>
		/// 
		/// <returns>The new array of the elements.</returns>
		public FileItemViewModel[] ToArray() {
			return list.ToArray();
		}

		/// <summary>Copies the collection to the specified array.</summary>
		///
		/// <param name="array">The array to copy the items to.</param>
		/// <param name="arrayIndex">The index to start copying items to in the array.</param>
		public void CopyTo(FileItemViewModel[] array, int arrayIndex) {
			list.CopyTo(array, arrayIndex);
		}

		#endregion

		#region Add

		/// <summary>Adds the item to the end of collection.</summary>
		/// 
		/// <param name="item">The item to add.</param>
		public void Add(FileItemViewModel item) {
			ThrowOnReentrancy();
			ThrowIfValueIsNullOrHasParent(item);
			//item.Parent = parent;
			list.Add(item);
			RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item, list.Count - 1);
		}

		/*public void AddSilent(FileItemViewModel item) {
			ThrowOnReentrancy();
			ThrowIfValueIsNullOrHasParent(item);
			list.Add(item);
		}*/

		/// <summary>Adds the items to the end of collection.</summary>
		/// 
		/// <param name="items">The items to add.</param>
		public void AddRange(IEnumerable<FileItemViewModel> items) {
			InsertRange(list.Count, items);
		}

		#endregion

		#region Insert

		/// <summary>Inserts the item at the specified index in the collection.</summary>
		/// 
		/// <param name="index">The index to insert at.</param>
		/// <param name="item">The item to insert.</param>
		public void Insert(int index, FileItemViewModel item) {
			ThrowOnReentrancy();
			ThrowIfValueIsNullOrHasParent(item);
			//item.Parent = parent;
			list.Insert(index, item);
			RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
		}

		/// <summary>Inserts the items at the specified index in the collection.</summary>
		/// 
		/// <param name="index">The index to insert at.</param>
		/// <param name="items">The items to insert.</param>
		public void InsertRange(int index, IEnumerable<FileItemViewModel> items) {
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			ThrowOnReentrancy();
			List<FileItemViewModel> newItems = items.ToList();
			if (newItems.Count == 0)
				return;
			foreach (FileItemViewModel item in newItems) {
				ThrowIfValueIsNullOrHasParent(item);
				//item.Parent = parent;
			}
			list.InsertRange(index, newItems);
			RaiseCollectionChanged(NotifyCollectionChangedAction.Add, newItems, index);
		}

		#endregion

		#region Remove

		/// <summary>Removes the items at the specified index in the collection.</summary>
		/// 
		/// <param name="index">The index to remove the item at.</param>
		public void RemoveAt(int index) {
			ThrowOnReentrancy();
			var oldItem = list[index];
			//oldItem.Parent = null;
			list.RemoveAt(index);
			RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, oldItem, index);
		}

		/// <summary>Removes the itemss at the specified index in the collection.</summary>
		/// 
		/// <param name="index">The index to remove the items at.</param>
		/// <param name="count">The number of items to remove.</param>
		public void RemoveRange(int index, int count) {
			ThrowOnReentrancy();
			if (count == 0)
				return;
			var oldItems = list.GetRange(index, count);
			//for (int i = 0; i < oldItems.Count; i++)
			//	oldItems[i].Parent = null;
			list.RemoveRange(index, count);
			RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, oldItems, index);
		}

		/// <summary>Removes the specified view model item from the collection.</summary>
		/// 
		/// <param name="item">The view model item to remove.</param>
		/// <returns>True if the item was found and removed.</returns>
		public bool Remove(FileItemViewModel item) {
			int pos = IndexOf(item);
			if (pos >= 0) {
				RemoveAt(pos);
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>Removes the specified model item from the collection.</summary>
		/// 
		/// <param name="item">The model item to remove.</param>
		/// <returns>True if the item was found and removed.</returns>
		public bool Remove(FileItemBase item) {
			int pos = list.FindIndex(n => n.Model == item);
			if (pos >= 0) {
				RemoveAt(pos);
				return true;
			}
			else {
				return false;
			}
		}

		/*public void RemoveAll(Predicate<FileItemViewModel> match) {
			if (match == null)
				throw new ArgumentNullException(nameof(match));
			ThrowOnReentrancy();
			int firstToRemove = 0;
			for (int i = 0; i < list.Count; i++) {
				bool removeItem;
				IsRaisingEvent = true;
				try {
					removeItem = match(list[i]);
				}
				finally {
					IsRaisingEvent = false;
				}
				if (!removeItem) {
					if (firstToRemove < i) {
						RemoveRange(firstToRemove, i - firstToRemove);
						i = firstToRemove - 1;
					}
					else {
						firstToRemove = i + 1;
					}
					Debug.Assert(firstToRemove == i + 1);
				}
			}
			if (firstToRemove < list.Count) {
				RemoveRange(firstToRemove, list.Count - firstToRemove);
			}
		}*/

		#endregion

		#region Move

		/*public void Move(int index, int oldIndex) {
			ThrowOnReentrancy();
			FileItemViewModel item = list[oldIndex];
			list.RemoveAt(oldIndex);
			list.Insert(index, item);
			RaiseCollectionChanged(NotifyCollectionChangedAction.Move, item, index, oldIndex);
		}*/

		#endregion

		#region Clear

		/// <summary>Clears the collection.</summary>
		public void Clear() {
			ThrowOnReentrancy();
			if (list.Count > 0) {
				//var oldList = new List<FileItem>(list);
				//list.Clear();
				var oldList = list.GetFullRange();
				//for (int i = 0; i < oldList.Count; i++)
				//	oldList[i].Parent = null;
				list.Clear();
				RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, oldList, 0);
			}
		}

		#endregion

		#region Explicit Interface Implementation

		/// <summary>Gets if the collection is readonly - it is not.</summary>
		bool ICollection<FileItemViewModel>.IsReadOnly => false;

		/// <summary>Gets the enumerator for the collection.</summary>
		IEnumerator<FileItemViewModel> IEnumerable<FileItemViewModel>.GetEnumerator() {
			return list.GetEnumerator();
		}

		/// <summary>Gets the enumerator for the collection.</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return list.GetEnumerator();
		}

		#endregion
	}
}
