using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	[DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public class CompactList<T> : IList<T>, IList, IReadOnlyList<T> {
		private const int _defaultCapacity = 4;
		private const int MaxArrayLength = 0X7FEFFFFF;
		private const int MaxByteArrayLength = 0X7FEFFFC7;

		internal T[] items;
		[ContractPublicPropertyName("Count")]
		private int _size;
		private int _version;
		//[NonSerialized]
		//private Object _syncRoot;
		// 4 bytes SAVED! WOW!

		static readonly T[] _emptyArray = new T[0];

		// Constructs a List. The list is initially empty and has a capacity
		// of zero. Upon adding the first element to the list the capacity is
		// increased to 16, and then increased in multiples of two as required.
		public CompactList() {
			items = _emptyArray;
		}

		// Constructs a List with a given initial capacity. The list is
		// initially empty, but will have room for the given number of elements
		// before any reallocations are required.
		// 
		public CompactList(int capacity) {
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			Contract.EndContractBlock();

			if (capacity == 0)
				items = _emptyArray;
			else
				items = new T[capacity];
		}

		// Constructs a List, copying the contents of the given collection. The
		// size and capacity of the new list will both be equal to the size of the
		// given collection.
		// 
		public CompactList(IEnumerable<T> collection) {
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			Contract.EndContractBlock();

			ICollection<T> c = collection as ICollection<T>;
			if (c != null) {
				int count = c.Count;
				if (count == 0) {
					items = _emptyArray;
				}
				else {
					items = new T[count];
					c.CopyTo(items, 0);
					_size = count;
				}
			}
			else {
				_size = 0;
				items = _emptyArray;
				// This enumerable could be empty.  Let Add allocate a new array, if needed.
				// Note it will also go to _defaultCapacity first, not 1, then 2, etc.

				using (IEnumerator<T> en = collection.GetEnumerator()) {
					while (en.MoveNext()) {
						Add(en.Current);
					}
				}
			}
		}

		// Gets and sets the capacity of this list.  The capacity is the size of
		// the internal array used to hold items.  When set, the internal 
		// array of the list is reallocated to the given capacity.
		// 
		public int Capacity {
			get {
				Contract.Ensures(Contract.Result<int>() >= 0);
				return items.Length;
			}
			set {
				if (value < _size) {
					throw new ArgumentOutOfRangeException(nameof(Capacity));
				}
				Contract.EndContractBlock();

				if (value != items.Length) {
					if (value > 0) {
						T[] newItems = new T[value];
						if (_size > 0) {
							Array.Copy(items, 0, newItems, 0, _size);
						}
						items = newItems;
					}
					else {
						items = _emptyArray;
					}
				}
			}
		}

		// Read-only property describing how many elements are in the List.
		public int Count {
			get {
				Contract.Ensures(Contract.Result<int>() >= 0);
				return _size;
			}
		}

		/*bool System.Collections.IList.IsFixedSize {
			get { return false; }
		}*/


		// Is this List read-only?
		bool ICollection<T>.IsReadOnly {
			get { return false; }
		}

		/*bool System.Collections.IList.IsReadOnly {
			get { return false; }
		}

		// Is this List synchronized (thread-safe)?
		bool System.Collections.ICollection.IsSynchronized {
			get { return false; }
		}*/

		// Synchronization root for this object.
		/*Object System.Collections.ICollection.SyncRoot {
			get {
				if (_syncRoot == null) {
					System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
				}
				return _syncRoot;
			}
		}*/
		// Sets or Gets the element at the given index.
		// 
		public T this[int index] {
			get {
				// Following trick can reduce the range check by one
				if ((uint) index >= (uint) _size) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				Contract.EndContractBlock();
				return items[index];
			}

			set {
				if ((uint) index >= (uint) _size) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				Contract.EndContractBlock();
				items[index] = value;
				_version++;
			}
		}

		private static bool IsCompatibleObject(object value) {
			// Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
			// Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
			return ((value is T) || (value == null && default(T) == null));
		}

		/*Object System.Collections.IList.this[int index] {
			get {
				return this[index];
			}
			set {
				if (value == null && typeof(T).IsValueType)
					throw new ArgumentNullException();

				//try {
					this[index] = (T) value;
				//}
				//catch (InvalidCastException) {
				//	ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
				//}
			}
		}*/

		// Adds the given object to the end of this list. The size of the list is
		// increased by one. If required, the capacity of the list is doubled
		// before adding the new element.
		//
		public void Add(T item) {
			if (_size == items.Length) EnsureCapacity(_size + 1);
			items[_size++] = item;
			_version++;
		}

		/*int System.Collections.IList.Add(Object item) {
			if (item == null && typeof(T).IsValueType)
				throw new ArgumentNullException();

			//try {
				Add((T) item);
			//}
			//catch (InvalidCastException) {
			//	ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
			//}

			return Count - 1;
		}*/


		// Adds the elements of the given collection to the end of this list. If
		// required, the capacity of the list is increased to twice the previous
		// capacity or the new size, whichever is larger.
		//
		public void AddRange(IEnumerable<T> collection) {
			Contract.Ensures(Count >= Contract.OldValue(Count));

			InsertRange(_size, collection);
		}

		public ReadOnlyCollection<T> AsReadOnly() {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<T>>() != null);
			return new ReadOnlyCollection<T>(this);
		}

		// Searches a section of the list for a given element using a binary search
		// algorithm. Elements of the list are compared to the search value using
		// the given IComparer interface. If comparer is null, elements of
		// the list are compared to the search value using the IComparable
		// interface, which in that case must be implemented by all elements of the
		// list and the given search value. This method assumes that the given
		// section of the list is already sorted; if this is not the case, the
		// result will be incorrect.
		//
		// The method returns the index of the given value in the list. If the
		// list does not contain the given value, the method returns a negative
		// integer. The bitwise complement operator (~) can be applied to a
		// negative result to produce the index of the first element (if any) that
		// is larger than the given search value. This is also the index at which
		// the search value should be inserted into the list in order for the list
		// to remain sorted.
		// 
		// The method uses the Array.BinarySearch method to perform the
		// search.
		// 
		public int BinarySearch(int index, int count, T item, IComparer<T> comparer) {
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (_size - index < count)
				throw new ArgumentOutOfRangeException(nameof(index));
			Contract.Ensures(Contract.Result<int>() <= index + count);
			Contract.EndContractBlock();

			return Array.BinarySearch<T>(items, index, count, item, comparer);
		}

		public int BinarySearch(T item) {
			Contract.Ensures(Contract.Result<int>() <= Count);
			return BinarySearch(0, Count, item, null);
		}

		public int BinarySearch(T item, IComparer<T> comparer) {
			Contract.Ensures(Contract.Result<int>() <= Count);
			return BinarySearch(0, Count, item, comparer);
		}


		// Clears the contents of List.
		public void Clear() {
			if (_size > 0) {
				Array.Clear(items, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
				_size = 0;
			}
			_version++;
		}

		// Contains returns true if the specified element is in the List.
		// It does a linear, O(n) search.  Equality is determined by calling
		// item.Equals().
		//
		public bool Contains(T item) {
			if ((Object) item == null) {
				for (int i = 0; i<_size; i++)
					if ((Object) items[i] == null)
						return true;
				return false;
			}
			else {
				EqualityComparer<T> c = EqualityComparer<T>.Default;
				for (int i = 0; i<_size; i++) {
					if (c.Equals(items[i], item)) return true;
				}
				return false;
			}
		}

		/*bool System.Collections.IList.Contains(Object item) {
			if (IsCompatibleObject(item)) {
				return Contains((T) item);
			}
			return false;
		}*/

		public CompactList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) {
			if (converter == null) {
				throw new ArgumentNullException(nameof(converter));
			}
			// @


			Contract.EndContractBlock();

			CompactList<TOutput> list = new CompactList<TOutput>(_size);
			for (int i = 0; i< _size; i++) {
				list.items[i] = converter(items[i]);
			}
			list._size = _size;
			return list;
		}

		internal void Minimize() {
			Capacity = Count;
		}

		// Copies this List into array, which must be of a 
		// compatible array type.  
		//
		public void CopyTo(T[] array) {
			CopyTo(array, 0);
		}

		// Copies this List into array, which must be of a 
		// compatible array type.  
		//
		/*void System.Collections.ICollection.CopyTo(Array array, int arrayIndex) {
			if ((array != null) && (array.Rank != 1)) {
				throw new RankException();
			}
			Contract.EndContractBlock();

			//try {
				// Array.Copy will check for NULL.
				Array.Copy(_items, 0, array, arrayIndex, _size);
			//}
			//catch (ArrayTypeMismatchException) {
			//	ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
			//}
		}*/

		// Copies a section of this list to the given array at the given index.
		// 
		// The method uses the Array.Copy method to copy the elements.
		// 
		public void CopyTo(int index, T[] array, int arrayIndex, int count) {
			if (_size - index < count) {
				throw new ArgumentOutOfRangeException();
			}
			Contract.EndContractBlock();

			// Delegate rest of error checking to Array.Copy.
			Array.Copy(items, index, array, arrayIndex, count);
		}

		public void CopyTo(T[] array, int arrayIndex) {
			// Delegate rest of error checking to Array.Copy.
			Array.Copy(items, 0, array, arrayIndex, _size);
		}

		// Ensures that the capacity of this list is at least the given minimum
		// value. If the currect capacity of the list is less than min, the
		// capacity is increased to twice the current capacity or to min,
		// whichever is larger.
		private void EnsureCapacity(int min) {
			if (items.Length < min) {
				int newCapacity = items.Length == 0 ? _defaultCapacity : items.Length * 2;
				// Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
				// Note that this check works even when _items.Length overflowed thanks to the (uint) cast
				if ((uint) newCapacity > MaxArrayLength) newCapacity = MaxArrayLength;
				if (newCapacity < min) newCapacity = min;
				Capacity = newCapacity;
			}
		}

		public bool Exists(Predicate<T> match) {
			return FindIndex(match) != -1;
		}

		public T Find(Predicate<T> match) {
			if (match == null) {
				throw new ArgumentNullException(nameof(match));
			}
			Contract.EndContractBlock();

			for (int i = 0; i < _size; i++) {
				if (match(items[i])) {
					return items[i];
				}
			}
			return default(T);
		}

		public List<T> FindAll(Predicate<T> match) {
			if (match == null) {
				throw new ArgumentNullException(nameof(match));
			}
			Contract.EndContractBlock();

			List<T> list = new List<T>();
			for (int i = 0; i < _size; i++) {
				if (match(items[i])) {
					list.Add(items[i]);
				}
			}
			return list;
		}

		public int FindIndex(Predicate<T> match) {
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() < Count);
			return FindIndex(0, _size, match);
		}

		public int FindIndex(int startIndex, Predicate<T> match) {
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() < startIndex + Count);
			return FindIndex(startIndex, _size - startIndex, match);
		}

		public int FindIndex(int startIndex, int count, Predicate<T> match) {
			if ((uint) startIndex > (uint) _size) {
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}

			if (count < 0 || startIndex > _size - count) {
				throw new ArgumentOutOfRangeException();
			}

			if (match == null) {
				throw new ArgumentNullException(nameof(match));
			}
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() < startIndex + count);
			Contract.EndContractBlock();

			int endIndex = startIndex + count;
			for (int i = startIndex; i < endIndex; i++) {
				if (match(items[i])) return i;
			}
			return -1;
		}

		public T FindLast(Predicate<T> match) {
			if (match == null) {
				throw new ArgumentNullException(nameof(match));
			}
			Contract.EndContractBlock();

			for (int i = _size - 1; i >= 0; i--) {
				if (match(items[i])) {
					return items[i];
				}
			}
			return default(T);
		}

		public int FindLastIndex(Predicate<T> match) {
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() < Count);
			return FindLastIndex(_size - 1, _size, match);
		}

		public int FindLastIndex(int startIndex, Predicate<T> match) {
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() <= startIndex);
			return FindLastIndex(startIndex, startIndex + 1, match);
		}

		public int FindLastIndex(int startIndex, int count, Predicate<T> match) {
			if (match == null) {
				throw new ArgumentNullException(nameof(match));
			}
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() <= startIndex);
			Contract.EndContractBlock();

			if (_size == 0) {
				// Special case for 0 length List
				if (startIndex != -1) {
					throw new ArgumentOutOfRangeException(nameof(startIndex));
				}
			}
			else {
				// Make sure we're not out of range            
				if ((uint) startIndex >= (uint) _size) {
					throw new ArgumentOutOfRangeException(nameof(startIndex));
				}
			}

			// 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
			if (count < 0 || startIndex - count + 1 < 0) {
				throw new ArgumentOutOfRangeException();
			}

			int endIndex = startIndex - count;
			for (int i = startIndex; i > endIndex; i--) {
				if (match(items[i])) {
					return i;
				}
			}
			return -1;
		}

		public void ForEach(Action<T> action) {
			if (action == null) {
				throw new ArgumentNullException(nameof(action));
			}
			Contract.EndContractBlock();

			int version = _version;

			for (int i = 0; i < _size; i++) {
				if (version != _version) {
					break;
				}
				action(items[i]);
			}

			if (version != _version)
				throw new InvalidOperationException();
		}

		// Returns an enumerator for this list with the given
		// permission for removal of elements. If modifications made to the list 
		// while an enumeration is in progress, the MoveNext and 
		// GetObject methods of the enumerator will throw an exception.
		//
		public Enumerator GetEnumerator() {
			return new Enumerator(this);
		}

		/// <internalonly/>
		IEnumerator<T> IEnumerable<T>.GetEnumerator() {
			return new Enumerator(this);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return new Enumerator(this);
		}

		public CompactList<T> GetRange(int index, int count) {
			if (index < 0) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (count < 0) {
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (_size - index < count) {
				throw new ArgumentOutOfRangeException();
			}
			Contract.Ensures(Contract.Result<List<T>>() != null);
			Contract.EndContractBlock();

			CompactList<T> list = new CompactList<T>(count);
			Array.Copy(items, index, list.items, 0, count);
			list._size = count;
			return list;
		}


		// Returns the index of the first occurrence of a given value in a range of
		// this list. The list is searched forwards from beginning to end.
		// The elements of the list are compared to the given value using the
		// Object.Equals method.
		// 
		// This method uses the Array.IndexOf method to perform the
		// search.
		// 
		public int IndexOf(T item) {
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() < Count);
			return Array.IndexOf(items, item, 0, _size);
		}

		/*int System.Collections.IList.IndexOf(Object item) {
			if (IsCompatibleObject(item)) {
				return IndexOf((T) item);
			}
			return -1;
		}*/

		// Returns the index of the first occurrence of a given value in a range of
		// this list. The list is searched forwards, starting at index
		// index and ending at count number of elements. The
		// elements of the list are compared to the given value using the
		// Object.Equals method.
		// 
		// This method uses the Array.IndexOf method to perform the
		// search.
		// 
		public int IndexOf(T item, int index) {
			if (index > _size)
				throw new ArgumentOutOfRangeException(nameof(index));
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() < Count);
			Contract.EndContractBlock();
			return Array.IndexOf(items, item, index, _size - index);
		}

		// Returns the index of the first occurrence of a given value in a range of
		// this list. The list is searched forwards, starting at index
		// index and upto count number of elements. The
		// elements of the list are compared to the given value using the
		// Object.Equals method.
		// 
		// This method uses the Array.IndexOf method to perform the
		// search.
		// 
		public int IndexOf(T item, int index, int count) {
			if (index > _size)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (count <0 || index > _size - count) throw new ArgumentOutOfRangeException(nameof(count));
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() < Count);
			Contract.EndContractBlock();

			return Array.IndexOf(items, item, index, count);
		}

		// Inserts an element into this list at a given index. The size of the list
		// is increased by one. If required, the capacity of the list is doubled
		// before inserting the new element.
		// 
		public void Insert(int index, T item) {
			// Note that insertions at the end are legal.
			if ((uint) index > (uint) _size) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			Contract.EndContractBlock();
			if (_size == items.Length) EnsureCapacity(_size + 1);
			if (index < _size) {
				Array.Copy(items, index, items, index + 1, _size - index);
			}
			items[index] = item;
			_size++;
			_version++;
		}

		/*void System.Collections.IList.Insert(int index, Object item) {
			if (item == null && typeof(T).IsValueType)
				throw new ArgumentNullException(nameof(item));

			//try {
				Insert(index, (T) item);
			//}
			//catch (InvalidCastException) {
			//	ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
			//}
		}*/

		// Inserts the elements of the given collection at a given index. If
		// required, the capacity of the list is increased to twice the previous
		// capacity or the new size, whichever is larger.  Ranges may be added
		// to the end of the list by setting index to the List's size.
		//
		public void InsertRange(int index, IEnumerable<T> collection) {
			if (collection==null) {
				throw new ArgumentNullException(nameof(collection));
			}

			if ((uint) index > (uint) _size) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			Contract.EndContractBlock();

			ICollection<T> c = collection as ICollection<T>;
			if (c != null) {    // if collection is ICollection<T>
				int count = c.Count;
				if (count > 0) {
					EnsureCapacity(_size + count);
					if (index < _size) {
						Array.Copy(items, index, items, index + count, _size - index);
					}

					// If we're inserting a List into itself, we want to be able to deal with that.
					if (this == c) {
						// Copy first part of _items to insert location
						Array.Copy(items, 0, items, index, index);
						// Copy last part of _items back to inserted location
						Array.Copy(items, index+count, items, index*2, _size-index);
					}
					else {
						T[] itemsToInsert = new T[count];
						c.CopyTo(itemsToInsert, 0);
						itemsToInsert.CopyTo(items, index);
					}
					_size += count;
				}
			}
			else {
				using (IEnumerator<T> en = collection.GetEnumerator()) {
					while (en.MoveNext()) {
						Insert(index++, en.Current);
					}
				}
			}
			_version++;
		}

		// Returns the index of the last occurrence of a given value in a range of
		// this list. The list is searched backwards, starting at the end 
		// and ending at the first element in the list. The elements of the list 
		// are compared to the given value using the Object.Equals method.
		// 
		// This method uses the Array.LastIndexOf method to perform the
		// search.
		// 
		public int LastIndexOf(T item) {
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() < Count);
			if (_size == 0) {  // Special case for empty list
				return -1;
			}
			else {
				return LastIndexOf(item, _size - 1, _size);
			}
		}

		// Returns the index of the last occurrence of a given value in a range of
		// this list. The list is searched backwards, starting at index
		// index and ending at the first element in the list. The 
		// elements of the list are compared to the given value using the 
		// Object.Equals method.
		// 
		// This method uses the Array.LastIndexOf method to perform the
		// search.
		// 
		public int LastIndexOf(T item, int index) {
			if (index >= _size)
				throw new ArgumentOutOfRangeException(nameof(index));
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(((Count == 0) && (Contract.Result<int>() == -1)) || ((Count > 0) && (Contract.Result<int>() <= index)));
			Contract.EndContractBlock();
			return LastIndexOf(item, index, index + 1);
		}

		// Returns the index of the last occurrence of a given value in a range of
		// this list. The list is searched backwards, starting at index
		// index and upto count elements. The elements of
		// the list are compared to the given value using the Object.Equals
		// method.
		// 
		// This method uses the Array.LastIndexOf method to perform the
		// search.
		// 
		public int LastIndexOf(T item, int index, int count) {
			if ((Count != 0) && (index < 0)) {
				throw new ArgumentOutOfRangeException();
			}

			if ((Count !=0) && (count < 0)) {
				throw new ArgumentOutOfRangeException(nameof(count));
			}
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(((Count == 0) && (Contract.Result<int>() == -1)) || ((Count > 0) && (Contract.Result<int>() <= index)));
			Contract.EndContractBlock();

			if (_size == 0) {  // Special case for empty list
				return -1;
			}

			if (index >= _size) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (count > index + 1) {
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			return Array.LastIndexOf(items, item, index, count);
		}

		// Removes the element at the given index. The size of the list is
		// decreased by one.
		// 
		public bool Remove(T item) {
			int index = IndexOf(item);
			if (index >= 0) {
				RemoveAt(index);
				return true;
			}

			return false;
		}

		/*void System.Collections.IList.Remove(Object item) {
			if (IsCompatibleObject(item)) {
				Remove((T) item);
			}
		}*/

		// This method removes all items which matches the predicate.
		// The complexity is O(n).   
		public int RemoveAll(Predicate<T> match) {
			if (match == null) {
				throw new ArgumentNullException(nameof(match));
			}
			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() <= Contract.OldValue(Count));
			Contract.EndContractBlock();

			int freeIndex = 0;   // the first free slot in items array

			// Find the first item which needs to be removed.
			while (freeIndex < _size && !match(items[freeIndex])) freeIndex++;
			if (freeIndex >= _size) return 0;

			int current = freeIndex + 1;
			while (current < _size) {
				// Find the first item which needs to be kept.
				while (current < _size && match(items[current])) current++;

				if (current < _size) {
					// copy item to the free slot.
					items[freeIndex++] = items[current++];
				}
			}

			Array.Clear(items, freeIndex, _size - freeIndex);
			int result = _size - freeIndex;
			_size = freeIndex;
			_version++;
			return result;
		}

		// Removes the element at the given index. The size of the list is
		// decreased by one.
		// 
		public void RemoveAt(int index) {
			if ((uint) index >= (uint) _size) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			Contract.EndContractBlock();
			_size--;
			if (index < _size) {
				Array.Copy(items, index + 1, items, index, _size - index);
			}
			items[_size] = default(T);
			_version++;
		}

		// Removes a range of elements from this list.
		// 
		public void RemoveRange(int index, int count) {
			if (index < 0) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (count < 0) {
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (_size - index < count)
				throw new ArgumentOutOfRangeException(nameof(index));
			Contract.EndContractBlock();

			if (count > 0) {
				int i = _size;
				_size -= count;
				if (index < _size) {
					Array.Copy(items, index + count, items, index, _size - index);
				}
				Array.Clear(items, _size, count);
				_version++;
			}
		}

		// Reverses the elements in this list.
		public void Reverse() {
			Reverse(0, Count);
		}

		// Reverses the elements in a range of this list. Following a call to this
		// method, an element in the range given by index and count
		// which was previously located at index i will now be located at
		// index index + (index + count - i - 1).
		// 
		// This method uses the Array.Reverse method to reverse the
		// elements.
		// 
		public void Reverse(int index, int count) {
			if (index < 0) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (count < 0) {
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (_size - index < count)
				throw new ArgumentOutOfRangeException(nameof(index));
			Contract.EndContractBlock();
			Array.Reverse(items, index, count);
			_version++;
		}

		// Sorts the elements in this list.  Uses the default comparer and 
		// Array.Sort.
		public void Sort() {
			Sort(0, Count, null);
		}

		// Sorts the elements in this list.  Uses Array.Sort with the
		// provided comparer.
		public void Sort(IComparer<T> comparer) {
			Sort(0, Count, comparer);
		}

		// Sorts the elements in a section of this list. The sort compares the
		// elements to each other using the given IComparer interface. If
		// comparer is null, the elements are compared to each other using
		// the IComparable interface, which in that case must be implemented by all
		// elements of the list.
		// 
		// This method uses the Array.Sort method to sort the elements.
		// 
		public void Sort(int index, int count, IComparer<T> comparer) {
			if (index < 0) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (count < 0) {
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (_size - index < count)
				throw new ArgumentOutOfRangeException(nameof(index));
			Contract.EndContractBlock();

			Array.Sort<T>(items, index, count, comparer);
			_version++;
		}

		public void Sort(Comparison<T> comparison) {
			if (comparison == null) {
				throw new ArgumentNullException(nameof(comparison));
			}
			Contract.EndContractBlock();

			if (_size > 0) {
				IComparer<T> comparer = new FunctorComparer<T>(comparison);
				Array.Sort(items, 0, _size, comparer);
			}
		}

		// ToArray returns a new Object array containing the contents of the List.
		// This requires copying the List, which is an O(n) operation.
		public T[] ToArray() {
			Contract.Ensures(Contract.Result<T[]>() != null);
			Contract.Ensures(Contract.Result<T[]>().Length == Count);

			T[] array = new T[_size];
			Array.Copy(items, 0, array, 0, _size);
			return array;
		}

		// Sets the capacity of this list to the size of the list. This method can
		// be used to minimize a list's memory overhead once it is known that no
		// new elements will be added to the list. To completely clear a list and
		// release all memory referenced by the list, execute the following
		// statements:
		// 
		// list.Clear();
		// list.TrimExcess();
		// 
		public void TrimExcess() {
			int threshold = (int) (((double) items.Length) * 0.9);
			if (_size < threshold) {
				Capacity = _size;
			}
		}

		public bool TrueForAll(Predicate<T> match) {
			if (match == null) {
				throw new ArgumentNullException(nameof(match));
			}
			Contract.EndContractBlock();

			for (int i = 0; i < _size; i++) {
				if (!match(items[i])) {
					return false;
				}
			}
			return true;
		}

		[Serializable]
		public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator {
			private CompactList<T> list;
			private int index;
			private int version;
			private T current;

			internal Enumerator(CompactList<T> list) {
				this.list = list;
				index = 0;
				version = list._version;
				current = default(T);
			}

			public void Dispose() {
			}

			public bool MoveNext() {

				CompactList<T> localList = list;

				if (version == localList._version && ((uint) index < (uint) localList._size)) {
					current = localList.items[index];
					index++;
					return true;
				}
				return MoveNextRare();
			}

			private bool MoveNextRare() {
				if (version != list._version) {
					throw new InvalidOperationException();
				}

				index = list._size + 1;
				current = default(T);
				return false;
			}

			public T Current {
				get {
					return current;
				}
			}

			Object System.Collections.IEnumerator.Current {
				get {
					if (index == 0 || index == list._size + 1) {
						throw new InvalidOperationException();
					}
					return Current;
				}
			}

			void System.Collections.IEnumerator.Reset() {
				if (version != list._version) {
					throw new InvalidOperationException();
				}

				index = 0;
				current = default(T);
			}

		}

		private sealed class FunctorComparer<T2> : IComparer<T2> {
			Comparison<T2> comparison;

			public FunctorComparer(Comparison<T2> comparison) {
				this.comparison = comparison;
			}

			public int Compare(T2 x, T2 y) {
				return comparison(x, y);
			}
		}

		public int Add(object value) {
			throw new NotImplementedException();
		}

		public bool Contains(object value) {
			throw new NotImplementedException();
		}

		public int IndexOf(object value) {
			throw new NotImplementedException();
		}

		public void Insert(int index, object value) {
			throw new NotImplementedException();
		}

		public void Remove(object value) {
			throw new NotImplementedException();
		}

		object IList.this[int index] {
			get => items[index];
			set => items[index] = (T) value;
		}

		public bool IsReadOnly {
			get => false;
		}
		public bool IsFixedSize {
			get => false;
		}

		public void CopyTo(Array array, int index) {
			Array.Copy(items, 0, array, index, _size);
		}

		public object SyncRoot {
			get => null;
		}
		public bool IsSynchronized {
			get => false;
		}
	}
}
