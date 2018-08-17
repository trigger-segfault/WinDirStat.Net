using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data.Nodes {
	public partial class FolderNode {

		private static readonly object stateLock = new object();

		public bool IsDone {
			get {
				lock (stateLock)
					return state.HasFlag(FileNodeStateFlags.Done);
			}
			internal set {
				lock (stateLock)
					state = state.SetFlag(FileNodeStateFlags.Done, value);
			}
		}
		public bool IsInvalidated {
			get {
				lock (stateLock)
					return state.HasFlag(FileNodeStateFlags.Invalidated);
			}
			protected set {
				lock (stateLock)
					state = state.SetFlag(FileNodeStateFlags.Invalidated, value);
			}
		}
		public bool IsValidating {
			get {
				lock (stateLock)
					return state.HasFlag(FileNodeStateFlags.Validating);
			}
			protected set {
				lock (stateLock)
					state = state.SetFlag(FileNodeStateFlags.Validating, value);
			}
		}
		public bool IsPopulated {
			get {
				lock (stateLock)
					return state.HasFlag(FileNodeStateFlags.Populated);
			}
			protected set {
				lock (stateLock)
					state = state.SetFlag(FileNodeStateFlags.Populated, value);
			}
		}
		/*public bool IsExpanded {
			get {
				lock (virtualChildren)
					return state.HasFlag(FileNodeStateFlags.Expanded);
			}
			protected set {
				lock (virtualChildren)
					state = state.SetFlag(FileNodeStateFlags.Expanded, value);
			}
		}*/
	}
}
