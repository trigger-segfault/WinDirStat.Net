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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WinDirStat.Net.Data.Nodes;

namespace WinDirStat.Net.TreeView {
	internal sealed class TreeFlattener : IList, INotifyCollectionChanged {
		/// <summary>
		/// The root node of the flat list tree.
		/// Tjis is not necessarily the root of the model!
		/// </summary>
		internal FileNode root;
		readonly bool includeRoot;
		readonly object syncRoot = new object();

		public TreeFlattener(FileNode modelRoot, bool includeRoot) {
			this.root = modelRoot;
			while (root.vi.listParent != null)
				root = root.vi.listParent;
			root.vi.treeFlattener = this;
			this.includeRoot = includeRoot;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void NodesReset() {
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e) {
			CollectionChanged?.Invoke(this, e);
		}

		public void NodesInserted(int index, IEnumerable<FileNode> nodes) {
			if (!includeRoot) index--;
			foreach (FileNode node in nodes) {
				RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index++));
			}
		}

		/*public void NodesInserted(int index, List<FileNode> nodes) {
			if (!includeRoot) index--;
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, nodes, index));
		}*/

		public void NodesRemoved(int index, IEnumerable<FileNode> nodes) {
			if (!includeRoot) index--;
			foreach (FileNode node in nodes) {
				RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node, index));
			}
		}

		/*public void NodesRemoved(int index, List<FileNode> nodes) {
			if (!includeRoot) index--;
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, nodes, index));
		}*/

		public void NodesMoved(int index, int oldIndex, IEnumerable<FileNode> nodes) {
			if (!includeRoot) index--;
			foreach (FileNode node in nodes) {
				//RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node, oldIndex++));
				//RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index++));
				RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, node, index++, oldIndex++));
			}
		}

		/*public void NodesMoved(int index, int oldIndex, List<FileNode> nodes) {
			if (!includeRoot) index--;
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, nodes, index, oldIndex));
		}*/

		public void Stop() {
			Debug.Assert(root.vi.treeFlattener == this);
			root.vi.treeFlattener = null;
		}

		public object this[int index] {
			get {
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException();
				return FileNode.GetNodeByVisibleIndex(root, includeRoot ? index : index + 1);
			}
			set => throw new NotSupportedException();
		}

		public int Count {
			get {
				return includeRoot ? root.GetTotalListLength() : root.GetTotalListLength() - 1;
			}
		}

		public int IndexOf(object item) {
			FileNode node = item as FileNode;
			if (node != null && node.IsVisible && node.GetListRoot() == root) {
				if (includeRoot)
					return FileNode.GetVisibleIndexForNode(node);
				else
					return FileNode.GetVisibleIndexForNode(node) - 1;
			}
			else {
				return -1;
			}
		}

		bool IList.IsReadOnly {
			get { return true; }
		}

		bool IList.IsFixedSize {
			get { return false; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get {
				return syncRoot;
			}
		}

		void IList.Insert(int index, object item) {
			throw new NotSupportedException();
		}

		void IList.RemoveAt(int index) {
			throw new NotSupportedException();
		}

		int IList.Add(object item) {
			throw new NotSupportedException();
		}

		void IList.Clear() {
			throw new NotSupportedException();
		}

		public bool Contains(object item) {
			return IndexOf(item) >= 0;
		}

		public void CopyTo(Array array, int arrayIndex) {
			foreach (object item in this)
				array.SetValue(item, arrayIndex++);
		}

		void IList.Remove(object item) {
			throw new NotSupportedException();
		}

		public IEnumerator GetEnumerator() {
			for (int i = 0; i < this.Count; i++) {
				yield return this[i];
			}
		}
	}
}
