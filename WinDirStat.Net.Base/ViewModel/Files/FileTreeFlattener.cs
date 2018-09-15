// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace WinDirStat.Net.ViewModel.Files {
	/// <summary>A flattener for a <see cref="FileItemViewModel"/> tree.</summary>
	public sealed class FileTreeFlattener : ObservableCollectionObject, IList {

		#region Fields

		/// <summary>
		/// The root node of the flat list tree.<para/>
		/// This is not necessarily the root of the model!
		/// </summary>
		internal FileItemViewModel root;
		private readonly bool includeRoot;
		private readonly object syncRoot = new object();

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="FileTreeFlattener"/>.</summary>
		/// 
		/// <param name="modelRoot">The root item of the list.</param>
		/// <param name="includeRoot">True if the root is visible in the list.</param>
		public FileTreeFlattener(FileItemViewModel modelRoot, bool includeRoot) {
			this.root = modelRoot;
			while (root.listParent != null)
				root = root.listParent;
			root.treeFlattener = this;
			this.includeRoot = includeRoot;
		}

		#endregion

		#region Nodes Events

		/// <summary>Raises a collection reset event.</summary>
		public void NodesReset() {
			RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
		}

		/// <summary>Raises a collection add event.</summary>
		/// 
		/// <param name="index">The index the nodes were added at.</param>
		/// <param name="nodes">The nodes that were added.</param>
		public void NodesInserted(int index, IEnumerable<FileItemViewModel> nodes) {
			if (!includeRoot) index--;
			foreach (FileItemViewModel node in nodes) {
				RaiseCollectionChanged(NotifyCollectionChangedAction.Add, node, index++);
			}
		}

		/// <summary>Raises a collection remove event.</summary>
		/// 
		/// <param name="index">The index the nodes were removed at.</param>
		/// <param name="nodes">The nodes that were removed.</param>
		public void NodesRemoved(int index, IEnumerable<FileItemViewModel> nodes) {
			if (!includeRoot) index--;
			foreach (FileItemViewModel node in nodes) {
				RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, node, index);
			}
		}

		/// <summary>Raises a collection move event.</summary>
		/// 
		/// <param name="index">The index the nodes were moved at.</param>
		/// <param name="nodes">The nodes that were moved.</param>
		public void NodesMoved(int index, int oldIndex, IEnumerable<FileItemViewModel> nodes) {
			if (!includeRoot) index--;
			foreach (FileItemViewModel node in nodes) {
				RaiseCollectionChanged(NotifyCollectionChangedAction.Move, node, index++, oldIndex++);
			}
		}

		#endregion

		#region Stop

		/// <summary>Stops the list and detaches it from the <see cref="FileItemViewModel"/>.</summary>
		public void Stop() {
			Debug.Assert(root.treeFlattener == this);
			root.treeFlattener = null;
		}

		#endregion

		#region IList Implementation

		/// <summary>Gets the item at the specified index in the list.</summary>
		/// 
		/// <param name="index">The index of the item.</param>
		public object this[int index] {
			get {
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException();
				return FileItemViewModel.GetNodeByVisibleIndex(root, includeRoot ? index : index + 1);
			}
			set => throw new NotSupportedException();
		}

		/// <summary>Gets the number of visible items in the list.</summary>
		public int Count {
			get => includeRoot ? root.GetTotalListLength() : root.GetTotalListLength() - 1;
		}

		/// <summary>Gets the index of the item in the list.</summary>
		/// 
		/// <param name="item">The item to get the index of.</param>
		/// <returns>The index of the item if found, otherwise -1.</returns>
		public int IndexOf(object item) {
			if (item is FileItemViewModel node && node.IsVisible && node.GetListRoot() == root) {
				if (includeRoot)
					return FileItemViewModel.GetVisibleIndexForNode(node);
				else
					return FileItemViewModel.GetVisibleIndexForNode(node) - 1;
			}
			else {
				return -1;
			}
		}

		/// <summary>Returns true if the list contains the specified item.</summary>
		/// 
		/// <param name="item">The item to check for.</param>
		/// <returns>True if the item was found.</returns>
		public bool Contains(object item) {
			return IndexOf(item) >= 0;
		}

		/// <summary>Copies the list to an array.</summary>
		/// 
		/// <param name="array">The array to copy to.</param>
		/// <param name="arrayIndex">The starting index in the array to copy to.</param>
		public void CopyTo(Array array, int arrayIndex) {
			for (int i = 0; i < Count; i++)
				array.SetValue(this[i], arrayIndex++);
		}


		/// <summary>Gets the enumerator for this list.</summary>
		/// 
		/// <returns>The enumerator for all visible elements in the list.</returns>
		public IEnumerator GetEnumerator() {
			for (int i = 0; i < Count; i++)
				yield return this[i];
		}

		#endregion

		#region NotSupported IList/ICollection Implementation

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => syncRoot;

		bool IList.IsReadOnly => true;

		bool IList.IsFixedSize => false;

		void IList.Insert(int index, object item) => throw new NotSupportedException();

		void IList.RemoveAt(int index) => throw new NotSupportedException();

		int IList.Add(object item) => throw new NotSupportedException();

		void IList.Clear() => throw new NotSupportedException();

		void IList.Remove(object item) => throw new NotSupportedException();

		#endregion
	}
}
