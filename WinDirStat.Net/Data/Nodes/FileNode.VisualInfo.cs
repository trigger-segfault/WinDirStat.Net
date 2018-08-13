using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WinDirStat.Net.TreeView;

namespace WinDirStat.Net.Data.Nodes {
	public partial class FileNode : INotifyPropertyChanged {
#pragma warning disable 0649, IDE1006
		/// <summary>
		/// A storage class for all visual information to reduce memory usage. This class will only be
		/// created if the node has been added to the WPF collection.
		/// </summary>
		internal class VisualInfo {

#if COMPRESSED_DATA
			private const ushort VisibleFlag	= 0x0100;
			private const ushort HiddenFlag		= 0x0200;
			private const ushort ExpandedFlag	= 0x0400;
			private const ushort SelectedFlag	= 0x0800;
			private const ushort EditingFlag	= 0x1000;

			/// <summary>Compressed flags and height data.</summary>
			private ushort heightAndFlags = 0x0101; // height = 1, isVisible = true

			/// <summary>Gets one of the <see cref="heightAndFlags"/> flags.</summary>
			private bool GetFlag(ushort flag) {
				return (heightAndFlags & flag) != 0;
			}

			/// <summary>Sets one of the <see cref="heightAndFlags"/> flags.</summary>
			private void SetFlag(ushort flag, bool enabled) {
				if (enabled)
					heightAndFlags |= flag;
				else
					heightAndFlags &= unchecked((ushort) ~flag);
			}
#endif

			// ======= FileNode.FlatList.cs ========
			
			/// <summary>The parent in the flat list</summary>
			internal FileNode listParent;
			/// <summary>Left/right nodes in the flat list</summary>
			internal FileNode left, right;

			/// <summary>The helper class for flattening the recursive tree into a flat list.</summary>
			internal TreeFlattener treeFlattener;

			//internal MyTreeFlattener myTreeFlattener;

#if !COMPRESSED_DATA
			/// <summary>Subtree height in the flat list tree</summary>
			//internal byte height = 1;
#else
			/// <summary>Subtree height in the flat list tree</summary>
			internal byte height {
				get => unchecked((byte) heightAndFlags);
				set => heightAndFlags = unchecked((ushort) ((heightAndFlags & 0xFF00) | value));
			}
#endif

			/// <summary>
			/// Length in the flat list, including children (children within the flat list). -1 = invalidated
			/// </summary>
			internal int totalListLength = -1;

			/// <summary>The start index in the parent node.</summary>
			//internal int startIndex;

			/// <summary>The end index in the parent node (exclusive).</summary>
			//internal int endIndex;

			//internal int listLength;

			/// <summary>The length of the node and its children.</summary>
			//internal int length => endIndex - startIndex;

			/*internal FileNode above;
			internal FileNode below;

			internal int totalLength = -1;

			internal int childLength = -1;

			internal int totalParentIndex = -1;

			internal int childParentIndex = -1;*/

			// ======= FileNode.TreeNode.cs ========

			/// <summary>The event raised when a property has been changed.</summary>
			//internal event PropertyChangedEventHandler PropertyChanged;

			/*internal void RaisePropertyChanged(FileNode sender, PropertyChangedEventArgs e) {
				PropertyChanged?.Invoke(sender, e);
			}*/

			internal bool IsWatched {
				get => false;//(PropertyChanged != null);// || (children?.IsWatched ?? false));
			}

			/// <summary>The visual children contained by this node.</summary>
			internal FileNodeCollection children;

			// TODO: See if this can be eliminated
			/// <summary>The visual parent for this node.</summary>
			internal FolderNode parent;

#if !COMPRESSED_DATA
			// TODO: Combine these into flags?
			/// <summary>True if the node is visible.</summary>
			//internal bool isVisible = true;
			/// <summary>True if the node is hidden.</summary>
			//internal bool isHidden;
			/// <summary>True if the node is expanded.</summary>
			//internal bool isExpanded;
			/// <summary>True if the node is selected.</summary>
			//internal bool isSelected;
			/// <summary>True if the node is being edited.</summary>
			//internal bool isEditing;
#else
			/// <summary>True if the node is visible.</summary>
			internal bool isVisible {
				get => GetFlag(VisibleFlag);
				set => SetFlag(VisibleFlag, value);
			}
			/// <summary>True if the node is hidden.</summary>
			internal bool isHidden {
				get => GetFlag(HiddenFlag);
				set => SetFlag(HiddenFlag, value);
			}
			/// <summary>True if the node is expanded.</summary>
			internal bool isExpanded {
				get => GetFlag(ExpandedFlag);
				set => SetFlag(ExpandedFlag, value);
			}
			/// <summary>True if the node is selected.</summary>
			internal bool isSelected {
				get => GetFlag(SelectedFlag);
				set => SetFlag(SelectedFlag, value);
			}
			/// <summary>True if the node is being edited.</summary>
			internal bool isEditing {
				get => GetFlag(EditingFlag);
				set => SetFlag(EditingFlag, value);
			}

			// ======= FileNode.cs ========

			/// <summary>The icon used by the node.</summary>
			internal ImageSource icon;

			public VisualInfo Clone() {
				return (VisualInfo) MemberwiseClone();
			}
#endif
		}

		/// <summary>The actual link to the visual information.</summary>
		internal VisualInfo visualInfo;

		/// <summary>Optional visual information that isn't allocated until it's needed.</summary>
		internal VisualInfo vi {
			get {
				if (visualInfo == null) {
					Debug.WriteLine("New VisualInfo: " + name);
					visualInfo = new VisualInfo();
					if (!(this is FolderNode)) {
						//visualInfo.totalListLength = 1;
						//visualInfo.myTreeFlattener
					}
				}
				return visualInfo;
			}
		}
		
		internal void UnloadVisuals() {
			if (visualInfo != null) {
				//Debug.Assert(!visualInfo.IsWatched);
				/*if (visualInfo.children != null) {
					FileNode[] removedChildren = visualInfo.children.ToArray();
					visualInfo.children.Clear();
					foreach (FileNode child in removedChildren) {
						child.UnloadVisuals();
					}
				}*/
				Debug.WriteLine("Remove VisualInfo: " + name);
				visualInfo = null;
			}
		}

#pragma warning restore 0649, IDE1006
	}
}
