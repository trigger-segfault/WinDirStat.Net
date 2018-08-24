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
using WinDirStat.Net.Model.Data.Nodes;

namespace WinDirStat.Net.Model.View.Nodes {
	public class FileNodeCollection : IFileNodeCollection {
		private readonly FileNodeViewModel parent;
		private List<FileNodeViewModel> list = new List<FileNodeViewModel>();
		private bool isRaisingEvent;

		public FileNodeCollection(FileNodeViewModel parent) {
			this.parent = parent;
		}

		/*public IComparer CustomSort {
			get => null;
			set { }
		}*/

		[field: NonSerialized]
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		void OnCollectionReset(List<FileNodeViewModel> newList, List<FileNodeViewModel> oldList) {
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

		void ThrowIfValueIsNullOrHasParent(FileNodeViewModel node) {
			if (node == null)
				throw new ArgumentNullException("node");
			if (node.parent != null)
				throw new ArgumentException("The node already has a parent", "node");
		}

		public FileNodeViewModel this[int index] {
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

		bool ICollection<FileNodeViewModel>.IsReadOnly {
			get => false;
		}

		public void Sort(Comparison<FileNodeViewModel> comparison) {
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

		public int IndexOf(FileNodeViewModel node) {
			if (node == null || node.parent != parent)
				return -1;
			else
				return list.IndexOf(node);
		}

		public int IndexOf(FileNodeBase node) {
			if (node == null)
				return -1;
			else
				return list.FindIndex(n => n.Model == node);
		}

		public void Insert(int index, FileNodeViewModel node) {
			ThrowOnReentrancy();
			ThrowIfValueIsNullOrHasParent(node);
			list.Insert(index, node);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index));
		}

		public void InsertRange(int index, IEnumerable<FileNodeViewModel> nodes) {
			if (nodes == null)
				throw new ArgumentNullException("nodes");
			ThrowOnReentrancy();
			List<FileNodeViewModel> newNodes = nodes.ToList();
			if (newNodes.Count == 0)
				return;
			foreach (FileNodeViewModel node in newNodes) {
				ThrowIfValueIsNullOrHasParent(node);
			}
			list.InsertRange(index, newNodes);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newNodes, index));
		}

		public void Move(int index, int oldIndex) {
			ThrowOnReentrancy();
			FileNodeViewModel node = list[oldIndex];
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

		public void Add(FileNodeViewModel node) {
			ThrowOnReentrancy();
			ThrowIfValueIsNullOrHasParent(node);
			list.Add(node);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, list.Count - 1));
		}

		public void AddSilent(FileNodeViewModel node) {
			ThrowOnReentrancy();
			ThrowIfValueIsNullOrHasParent(node);
			list.Add(node);
		}

		public FileNodeViewModel Find(FileNodeBase node) {
			return list.Find(v => v.Model == node);
		}

		public FileNodeViewModel[] ToArray() {
			return list.ToArray();
		}

		public void AddRange(IEnumerable<FileNodeViewModel> nodes) {
			InsertRange(this.Count, nodes);
		}

		public void Clear() {
			ThrowOnReentrancy();
			//var oldList = new List<FileNode>(list);
			//list.Clear();
			var oldList = list;
			list = new List<FileNodeViewModel>();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldList, 0));
		}

		public bool Contains(FileNodeViewModel node) {
			return IndexOf(node) >= 0;
		}

		public void CopyTo(FileNodeViewModel[] array, int arrayIndex) {
			list.CopyTo(array, arrayIndex);
		}

		public bool Remove(FileNodeViewModel item) {
			int pos = IndexOf(item);
			if (pos >= 0) {
				RemoveAt(pos);
				return true;
			}
			else {
				return false;
			}
		}

		public bool Remove(FileNodeBase item) {
			int pos = list.FindIndex(n => n.Model == item);
			if (pos >= 0) {
				RemoveAt(pos);
				return true;
			}
			else {
				return false;
			}
		}

		public IEnumerator<FileNodeViewModel> GetEnumerator() {
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return list.GetEnumerator();
		}

		public void RemoveAll(Predicate<FileNodeViewModel> match) {
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
