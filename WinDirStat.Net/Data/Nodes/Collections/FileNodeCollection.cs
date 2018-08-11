using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WinDirStat.Net.Data.Nodes {
	public class FileNodeCollection : IFileNodeCollection {
		private readonly FolderNode parent;
		private List<FileNode> list = new List<FileNode>();
		private bool isRaisingEvent;

		public FileNodeCollection(FolderNode parent) {
			this.parent = parent;
		}

		/*public IComparer CustomSort {
			get => null;
			set { }
		}*/

		[field: NonSerialized]
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		void OnCollectionReset(List<FileNode> newList, List<FileNode> oldList) {
			Debug.Assert(!isRaisingEvent);
			isRaisingEvent = true;
			try {
				parent.OnCollectionReset(newList, oldList);
				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
			finally {
				isRaisingEvent = false;
			}
		}

		void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
			Debug.Assert(!isRaisingEvent);
			isRaisingEvent = true;
			try {
				parent.OnChildrenChanged(e);
				CollectionChanged?.Invoke(this, e);
			}
			finally {
				isRaisingEvent = false;
			}
		}

		void ThrowOnReentrancy() {
			if (isRaisingEvent)
				throw new InvalidOperationException();
		}

		void ThrowIfValueIsNullOrHasParent(FileNode node) {
			if (node == null)
				throw new ArgumentNullException("node");
			if (node.vi.parent != null)
				throw new ArgumentException("The node already has a parent", "node");
		}

		public FileNode this[int index] {
			get => list[index];
			set {
				ThrowOnReentrancy();
				var oldItem = list[index];
				if (oldItem == value)
					return;
				ThrowIfValueIsNullOrHasParent(value);
				list[index] = value;
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
			}
		}

		public int Count {
			get => list.Count;
		}

		bool ICollection<FileNode>.IsReadOnly {
			get => false;
		}

		public void Sort(Comparison<FileNode> comparison) {
			var oldList = list.ToList();
			//list = new List<FileNode>(oldList.Capacity);
			//OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldList, 0));
			list.Sort(comparison);
			//InsertRange(0, oldList);
			OnCollectionReset(list, oldList);
			//OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, list, oldList, 0));
		}

		public bool IsWatched {
			get => CollectionChanged != null;
		}

		public int IndexOf(FileNode node) {
			if (node == null || node.vi.parent != parent)
				return -1;
			else
				return list.IndexOf(node);
		}

		public void Insert(int index, FileNode node) {
			ThrowOnReentrancy();
			ThrowIfValueIsNullOrHasParent(node);
			list.Insert(index, node);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index));
		}

		public void InsertRange(int index, IEnumerable<FileNode> nodes) {
			if (nodes == null)
				throw new ArgumentNullException("nodes");
			ThrowOnReentrancy();
			List<FileNode> newNodes = nodes.ToList();
			if (newNodes.Count == 0)
				return;
			foreach (FileNode node in newNodes) {
				ThrowIfValueIsNullOrHasParent(node);
			}
			list.InsertRange(index, newNodes);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newNodes, index));
		}

		public void Move(int index, int oldIndex) {
			ThrowOnReentrancy();
			FileNode node = list[oldIndex];
			list.RemoveAt(oldIndex);
			list.Insert(index, node);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, node, index, oldIndex));
		}

		public void RemoveAt(int index) {
			ThrowOnReentrancy();
			var oldItem = list[index];
			list.RemoveAt(index);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
		}

		public void RemoveRange(int index, int count) {
			ThrowOnReentrancy();
			if (count == 0)
				return;
			var oldItems = list.GetRange(index, count);
			list.RemoveRange(index, count);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, index));
		}

		public void Add(FileNode node) {
			ThrowOnReentrancy();
			ThrowIfValueIsNullOrHasParent(node);
			list.Add(node);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, list.Count - 1));
		}

		public FileNode[] ToArray() {
			return list.ToArray();
		}

		public void AddRange(IEnumerable<FileNode> nodes) {
			InsertRange(this.Count, nodes);
		}

		public void Clear() {
			ThrowOnReentrancy();
			//var oldList = new List<FileNode>(list);
			//list.Clear();
			var oldList = list;
			list = new List<FileNode>();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldList, 0));
		}

		public bool Contains(FileNode node) {
			return IndexOf(node) >= 0;
		}

		public void CopyTo(FileNode[] array, int arrayIndex) {
			list.CopyTo(array, arrayIndex);
		}

		public bool Remove(FileNode item) {
			int pos = IndexOf(item);
			if (pos >= 0) {
				RemoveAt(pos);
				return true;
			}
			else {
				return false;
			}
		}

		public IEnumerator<FileNode> GetEnumerator() {
			return list.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return list.GetEnumerator();
		}

		public void RemoveAll(Predicate<FileNode> match) {
			if (match == null)
				throw new ArgumentNullException("match");
			ThrowOnReentrancy();
			int firstToRemove = 0;
			for (int i = 0; i < list.Count; i++) {
				bool removeNode;
				isRaisingEvent = true;
				try {
					removeNode = match(list[i]);
				}
				finally {
					isRaisingEvent = false;
				}
				if (!removeNode) {
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
		}
	}
}
