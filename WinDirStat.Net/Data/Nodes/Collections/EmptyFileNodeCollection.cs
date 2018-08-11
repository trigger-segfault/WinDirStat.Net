using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data.Nodes {
	public class EmptyFileNodeCollection : IFileNodeCollection {

		public void Add(FileNode item) {
			throw new NotSupportedException();
		}

		public void AddRange(IEnumerable<FileNode> nodes) {
			throw new NotImplementedException();
		}

		public void Clear() {
			throw new NotSupportedException();
		}

		public bool Contains(FileNode item) {
			return false;
		}

		public void CopyTo(FileNode[] array, int arrayIndex) {
			throw new NotSupportedException();
		}

		public bool Remove(FileNode item) {
			throw new NotSupportedException();
		}

		public int Count {
			get => 0;
		}

		public bool IsReadOnly {
			get => true;
		}

		public IEnumerator<FileNode> GetEnumerator() {
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public int IndexOf(FileNode item) {
			return -1;
		}

		public void Insert(int index, FileNode item) {
			throw new NotSupportedException();
		}

		public void InsertRange(int index, IEnumerable<FileNode> nodes) {
			throw new NotImplementedException();
		}

		public void RemoveRange(int index, int count) {
			throw new NotImplementedException();
		}

		public void RemoveAll(Predicate<FileNode> match) {
			throw new NotImplementedException();
		}

		public void RemoveAt(int index) {
			throw new NotSupportedException();
		}

		public FileNode[] ToArray() {
			return new FileNode[0];
		}

		public FileNode this[int index] {
			get => throw new ArgumentOutOfRangeException();
			set => throw new NotSupportedException();
		}

		/// <summary>This event will never fire because the list will never change.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged {
			add { }
			remove { }
		}
	}
}
