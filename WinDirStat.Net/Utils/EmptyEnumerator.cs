using System;
using System.Collections;
using System.Collections.Generic;

namespace WinDirStat.Net.Utils {
	public static class Enumerator {
		private class EmptyEnumerator : IEnumerator {
			
			public EmptyEnumerator() { }

			public void Reset() { }

			public bool MoveNext() {
				return false;
			}

			public object Current {
				get => throw new InvalidOperationException();
			}
		}

		private class EmptyEnumerator<T> : IEnumerator<T>, IEnumerator {
			
			public EmptyEnumerator() { }

			public void Reset() { }

			public void Dispose() { }

			public bool MoveNext() {
				return false;
			}

			public T Current {
				get => throw new InvalidOperationException();
			}

			object IEnumerator.Current {
				get => throw new InvalidOperationException();
			}
		}

		public static IEnumerator Empty() {
			return new EmptyEnumerator();
		}

		public static IEnumerator<T> Empty<T>() {
			return new EmptyEnumerator<T>();
		}
	}
}
