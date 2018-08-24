using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Data.Nodes {
	public enum FileNodeAction {
		/// <summary>The node state has been marked is invalidated.</summary>
		Invalidated,
		/// <summary>The node state has been validated to reflect its children.</summary>
		Validated,
		/// <summary>Properties that require a visual resort have changed during validation.</summary>
		ValidatedSortOrder,
		/// <summary>The node and all children have been fully scanned.</summary>
		Done,
		/// <summary>The node was refreshed, and non-standard sort properties may have changed.</summary>
		Refreshed,
		/// <summary>Children have been added to the node.</summary>
		ChildrenAdded,
		/// <summary>Children have been removed from the node.</summary>
		ChildrenRemoved,
		/// <summary>Children have been cleared.</summary>
		ChildrenCleared,
		/// <summary>Requests the view watching this node to show itself.</summary>
		GetView,

		IndexChanged,
	}

	public class FileNodeEventArgs {
		public FileNodeAction Action { get; }
		public List<FileNodeBase> Children { get; }
		public int Index { get; }
		public object View { get; set; }

		public FileNodeEventArgs(FileNodeAction action) {
			Action = action;
			Index = -1;
		}

		public FileNodeEventArgs(FileNodeAction action, int index) {
			Action = action;
			Index = index;
		}

		public FileNodeEventArgs(FileNodeAction action, List<FileNodeBase> children, int index = -1) {
			Action = action;
			Children = children;
			Index = index;
		}

		public FileNodeEventArgs(FileNodeAction action, FileNodeBase child, int index = -1) {
			Action = action;
			Children = new List<FileNodeBase> { child };
			Index = index;
		}
	}

	public delegate void FileNodeEventHandler(FileNodeBase sender, FileNodeEventArgs e);
}
