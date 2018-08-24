using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.View.Nodes {
	public class EmptyFileNodeCollection : IFileNodeCollection {

		public void Add(FileNodeViewModel item) {
			throw new NotSupportedException();
		}

		public void AddRange(IEnumerable<FileNodeViewModel> nodes) {
			throw new NotImplementedException();
		}

		public void Clear() {
			throw new NotSupportedException();
		}

		public bool Contains(FileNodeViewModel item) {
			return false;
		}

		public void CopyTo(FileNodeViewModel[] array, int arrayIndex) {
			throw new NotSupportedException();
		}

		public bool Remove(FileNodeViewModel item) {
			throw new NotSupportedException();
		}

		public int Count {
			get => 0;
		}

		public bool IsReadOnly {
			get => true;
		}

		public IEnumerator<FileNodeViewModel> GetEnumerator() {
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public int IndexOf(FileNodeViewModel item) {
			return -1;
		}

		public void Insert(int index, FileNodeViewModel item) {
			throw new NotSupportedException();
		}

		public void InsertRange(int index, IEnumerable<FileNodeViewModel> nodes) {
			throw new NotImplementedException();
		}

		public void RemoveRange(int index, int count) {
			throw new NotImplementedException();
		}

		public void RemoveAll(Predicate<FileNodeViewModel> match) {
			throw new NotImplementedException();
		}

		public void RemoveAt(int index) {
			throw new NotSupportedException();
		}

		public FileNodeViewModel[] ToArray() {
			return new FileNodeViewModel[0];
		}

		public FileNodeViewModel this[int index] {
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
