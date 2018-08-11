using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WinDirStat.Net.Utils {
	public struct StackInfo {
		public string ID { get; }
		public string Caller { get; }
		public string File { get; }
		public int Line { get; }

		public StackInfo([CallerLineNumber] int line = 0, [CallerFilePath] string file = null, [CallerMemberName] string caller = null) {
			ID = null;
			Caller = caller;
			File = file;
			Line = line;
		}

		public StackInfo(string id, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null, [CallerMemberName] string caller = null) {
			ID = id;
			Caller = caller;
			File = file;
			Line = line;
		}

		public override string ToString() {
			if (!string.IsNullOrWhiteSpace(ID))
				return $"{ID}: {Caller} in {File}:line {Line}";
			return $"{Caller} in {File}:line {Line}";
		}
	}

	public struct TimedLock : IDisposable {

		private static readonly Dictionary<object, Stack<StackInfo>> currentLocks;

		public static ReadOnlyDictionary<object, Stack<StackInfo>> CurrentLocks {
			get => new ReadOnlyDictionary<object, Stack<StackInfo>>(currentLocks);
		}

		public static TimeSpan DefaultTimeout { get; set; }

		static TimedLock() {
			currentLocks = new Dictionary<object, Stack<StackInfo>>();
			DefaultTimeout = TimeSpan.FromSeconds(10);
		}

		public static TimedLock Lock(object o, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null, [CallerMemberName] string caller = null) {
			return Lock(o, DefaultTimeout, null, line, file, caller);
		}

		public static TimedLock Lock(object o, double seconds, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null, [CallerMemberName] string caller = null) {
			return Lock(o, TimeSpan.FromSeconds(seconds), null, line, file, caller);
		}

		public static TimedLock Lock(object o, TimeSpan timeout, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null, [CallerMemberName] string caller = null) {
			return Lock(o, timeout, null, line, file, caller);
		}

		public static TimedLock Lock(object o, string id, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null, [CallerMemberName] string caller = null) {
			return Lock(o, DefaultTimeout, id, line, file, caller);
		}

		public static TimedLock Lock(object o, double seconds, string id, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null, [CallerMemberName] string caller = null) {
			return Lock(o, TimeSpan.FromSeconds(seconds), id, line, file, caller);
		}

		public static TimedLock Lock(object o, TimeSpan timeout, string id, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null, [CallerMemberName] string caller = null) {
			TimedLock tl = new TimedLock(o);
			if (!Monitor.TryEnter(o, timeout)) {
#if DEBUG
				GC.SuppressFinalize(tl.leakDetector);
#endif
				if (id != null)
					Console.WriteLine($"Lock {id} timed out!");
				else
					Console.WriteLine("Lock timed out!");
				Console.WriteLine($"Currently locked by: {Peek(o)}");
				Console.WriteLine();
				Console.WriteLine("Stack Trace:");
				Console.WriteLine(new StackTrace());
				throw new LockTimeoutException();
			}
			Push(o, new StackInfo(id, line, file, caller));

			return tl;
		}

		private static void Push(object o, StackInfo stackInfo) {
			lock (currentLocks) {
				if (!currentLocks.TryGetValue(o, out Stack<StackInfo> stack)) {
					stack = new Stack<StackInfo>();
					currentLocks.Add(o, stack);
				}
				stack.Push(stackInfo);
			}
		}

		private static StackInfo Peek(object o) {
			lock (currentLocks)
				return currentLocks[o].Peek();
		}

		private static void Pop(object o) {
			lock (currentLocks) {
				Stack<StackInfo> stack = currentLocks[o];
				stack.Pop();
				if (stack.Count == 0)
					currentLocks.Remove(o);
			}
		}

		private TimedLock(object o) {
			target = o;
#if DEBUG
			leakDetector = new Sentinel();
#endif
		}

		/// <summary>The locked object.</summary>
		private readonly object target;

		/// <summary>Disposes of and exits the lock.</summary>
		public void Dispose() {
			Monitor.Exit(target);

			Pop(target);

			// It's a bad error if someone forgets to call Dispose,
			// so in Debug builds, we put a finalizer in to detect
			// the error. If Dispose is called, we suppress the
			// finalizer.
#if DEBUG
			GC.SuppressFinalize(leakDetector);
#endif
		}

#if DEBUG
		/// <summary>
		/// (In Debug mode, we make it a class so that we can add a finalizer
		/// in order to detect when the object is not freed.)
		/// </summary>
		private class Sentinel {
			~Sentinel() {
				// If this finalizer runs, someone somewhere failed to
				// call Dispose, which means we've failed to leave
				// a monitor!
				Debug.Fail("Undisposed lock");
			}
		}
		private Sentinel leakDetector;
#endif
	}

	public class LockTimeoutException : ApplicationException {
		public LockTimeoutException() : base("Lock timed out!") {
		}
	}
}
