using System;
using System.Collections;
using System.Collections.Generic;

namespace WinDirStat.Net.Utils {
	/// <summary>A static class for creating basic <see cref="IEnumerator"/>s.</summary>
	public static class Enumerator {

		#region EmptyEnumerator

		/// <summary>An empty non-generic enumerator.</summary>
		private class EmptyEnumerator : IEnumerator {
			
			/// <summary>Constructs an empty enumerator.</summary>
			public EmptyEnumerator() { }

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the
			/// collection.
			/// </summary>
			public void Reset() { }

			/// <summary>Advances the enumerator to the next element of the collection.</summary>
			/// 
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; false if the enumerator
			/// has passed the end of the collection.
			/// </returns>
			public bool MoveNext() {
				return false;
			}

			/// <summary>Gets the current element in the collection.</summary>
			public object Current {
				get => throw new InvalidOperationException();
			}
		}

		#endregion

		#region EmptyEnumerator<T>

		/// <summary>An empty generic enumerator.</summary>
		private class EmptyEnumerator<T> : IEnumerator<T>, IEnumerator {

			/// <summary>Constructs an empty generic enumerator.</summary>
			public EmptyEnumerator() { }

			/// <summary>Disposes of the enumerator.</summary>
			public void Dispose() { }

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the
			/// collection.
			/// </summary>
			public void Reset() { }

			/// <summary>Advances the enumerator to the next element of the collection.</summary>
			/// 
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; false if the enumerator
			/// has passed the end of the collection.
			/// </returns>
			public bool MoveNext() {
				return false;
			}

			/// <summary>Gets the current element in the collection.</summary>
			public T Current {
				get => throw new InvalidOperationException();
			}

			/// <summary>Gets the current element in the collection.</summary>
			object IEnumerator.Current {
				get => throw new InvalidOperationException();
			}
		}

		#endregion

		#region EmptyEnumerator Static Constructors

		/// <summary>Construct an empty <see cref="IEnumerator"/>.</summary>
		/// 
		/// <returns>The new non-generic enumerator.</returns>
		public static IEnumerator Empty() {
			return new EmptyEnumerator();
		}

		/// <summary>Construct an empty <see cref="IEnumerator{T}"/>.</summary>
		/// 
		/// <typeparam name="T">The generic type of the enumerator.</typeparam>
		/// <returns>The new generic enumerator.</returns>
		public static IEnumerator<T> Empty<T>() {
			return new EmptyEnumerator<T>();
		}

		#endregion
	}
}
