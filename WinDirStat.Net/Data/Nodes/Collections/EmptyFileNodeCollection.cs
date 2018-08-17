using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data.Nodes {
	public class EmptyFileNodeCollection : IFileNodeCollection {

		public void Add(FileNodeBase item) {
			throw new NotSupportedException();
		}

		public void AddRange(IEnumerable<FileNodeBase> nodes) {
			throw new NotImplementedException();
		}

		public void Clear() {
			throw new NotSupportedException();
		}

		public bool Contains(FileNodeBase item) {
			return false;
		}

		public void CopyTo(FileNodeBase[] array, int arrayIndex) {
			throw new NotSupportedException();
		}

		public bool Remove(FileNodeBase item) {
			throw new NotSupportedException();
		}

		public int Count {
			get => 0;
		}

		public bool IsReadOnly {
			get => true;
		}

		public IEnumerator<FileNodeBase> GetEnumerator() {
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public int IndexOf(FileNodeBase item) {
			return -1;
		}

		public void Insert(int index, FileNodeBase item) {
			throw new NotSupportedException();
		}

		public void InsertRange(int index, IEnumerable<FileNodeBase> nodes) {
			throw new NotImplementedException();
		}

		public void RemoveRange(int index, int count) {
			throw new NotImplementedException();
		}

		public void RemoveAll(Predicate<FileNodeBase> match) {
			throw new NotImplementedException();
		}

		public void RemoveAt(int index) {
			throw new NotSupportedException();
		}

		public FileNodeBase[] ToArray() {
			return new FileNodeBase[0];
		}

		public FileNodeBase this[int index] {
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
