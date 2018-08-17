// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using WinDirStat.Net.TreeView;

namespace WinDirStat.Net.Data.Nodes {
	// This part of SharpTreeNode controls the 'flat list' data structure, which emulates
	// a big flat list containing the whole tree; allowing access by visible index.
	public partial class FileNodeBase {
		/// <summary>The parent in the flat list</summary>
		//internal FileNode listParent;
		/// <summary>Left/right nodes in the flat list</summary>
		//FileNode left, right;

		//internal TreeFlattener treeFlattener;

		/// <summary>Subtree height in the flat list tree</summary>
		//byte height = 1;

		/// <summary>Length in the flat list, including children (children within the flat list). -1 = invalidated</summary>
		//int totalListLength = -1;

		int Balance {
			get { return Height(vi.right) - Height(vi.left); }
		}

		static int Height(FileNodeBase node) {
			return node != null ? node.vi.height : 0;
		}

		internal FileNodeBase GetListRoot() {
			FileNodeBase node = this;
			while (node.vi.listParent != null)
				node = node.vi.listParent;
			return node;
		}

		#region Debugging
		[Conditional("DEBUG")]
		void CheckRootInvariants() {
			GetListRoot().CheckInvariants();
		}

		[Conditional("DATACONSISTENCYCHECK")]
		void CheckInvariants() {
			Debug.Assert(vi.left == null || vi.left.vi.listParent == this);
			Debug.Assert(vi.right == null || vi.right.vi.listParent == this);
			Debug.Assert(vi.height == 1 + Math.Max(Height(vi.left), Height(vi.right)));
			Debug.Assert(Math.Abs(this.Balance) <= 1);
			Debug.Assert(vi.totalListLength == -1 || vi.totalListLength == (vi.left != null ? vi.left.vi.totalListLength : 0) + (vi.isVisible ? 1 : 0) + (vi.right != null ? vi.right.vi.totalListLength : 0));
			if (vi.left != null) vi.left.CheckInvariants();
			if (vi.right != null) vi.right.CheckInvariants();
		}

		[Conditional("DEBUG")]
		static void DumpTree(FileNodeBase node) {
			node.GetListRoot().DumpTree();
		}

		[Conditional("DEBUG")]
		void DumpTree() {
			Debug.Indent();
			if (vi.left != null)
				vi.left.DumpTree();
			Debug.Unindent();
			Debug.WriteLine("{0}, totalListLength={1}, height={2}, Balance={3}, isVisible={4}", ToString(), vi.totalListLength, vi.height, Balance, vi.isVisible);
			Debug.Indent();
			if (vi.right != null)
				vi.right.DumpTree();
			Debug.Unindent();
		}
		#endregion

		#region GetNodeByVisibleIndex / GetVisibleIndexForNode
		internal static FileNodeBase GetNodeByVisibleIndex(FileNodeBase root, int index) {
			root.GetTotalListLength(); // ensure all list lengths are calculated
			Debug.Assert(index >= 0);
			Debug.Assert(index < root.vi.totalListLength);
			FileNodeBase node = root;
			while (true) {
				if (node.vi.left != null && index < node.vi.left.vi.totalListLength) {
					node = node.vi.left;
				}
				else {
					if (node.vi.left != null) {
						index -= node.vi.left.vi.totalListLength;
					}
					if (node.vi.isVisible) {
						if (index == 0)
							return node;
						index--;
					}
					node = node.vi.right;
				}
			}
		}

		internal static int GetVisibleIndexForNode(FileNodeBase node) {
			int index = node.vi.left != null ? node.vi.left.GetTotalListLength() : 0;
			while (node.vi.listParent != null) {
				if (node == node.vi.listParent.vi.right) {
					if (node.vi.listParent.vi.left != null)
						index += node.vi.listParent.vi.left.GetTotalListLength();
					if (node.vi.listParent.vi.isVisible)
						index++;
				}
				node = node.vi.listParent;
			}
			return index;
		}
		#endregion

		#region Balancing
		/// <summary>
		/// Balances the subtree rooted in <paramref name="node"/> and recomputes the 'height' field.
		/// This method assumes that the children of this node are already balanced and have an up-to-date 'height' value.
		/// </summary>
		/// <returns>The new root node</returns>
		static FileNodeBase Rebalance(FileNodeBase node) {
			Debug.Assert(node.vi.left == null || Math.Abs(node.vi.left.Balance) <= 1);
			Debug.Assert(node.vi.right == null || Math.Abs(node.vi.right.Balance) <= 1);
			// Keep looping until it's balanced. Not sure if this is stricly required; this is based on
			// the Rope code where node merging made this necessary.
			while (Math.Abs(node.Balance) > 1) {
				// AVL balancing
				// note: because we don't care about the identity of concat nodes, this works a little different than usual
				// tree rotations: in our implementation, the "this" node will stay at the top, only its children are rearranged
				if (node.Balance > 1) {
					if (node.vi.right.Balance < 0) {
						node.vi.right = node.vi.right.RotateRight();
					}
					node = node.RotateLeft();
					// If 'node' was unbalanced by more than 2, we've shifted some of the inbalance to the left node; so rebalance that.
					node.vi.left = Rebalance(node.vi.left);
				}
				else if (node.Balance < -1) {
					if (node.vi.left.Balance > 0) {
						node.vi.left = node.vi.left.RotateLeft();
					}
					node = node.RotateRight();
					// If 'node' was unbalanced by more than 2, we've shifted some of the inbalance to the right node; so rebalance that.
					node.vi.right = Rebalance(node.vi.right);
				}
			}
			Debug.Assert(Math.Abs(node.Balance) <= 1);
			node.vi.height = (byte) (1 + Math.Max(Height(node.vi.left), Height(node.vi.right)));
			node.vi.totalListLength = -1; // mark for recalculation
									   // since balancing checks the whole tree up to the root, the whole path will get marked as invalid
			return node;
		}

		internal int GetTotalListLength() {
			if (vi.totalListLength >= 0)
				return vi.totalListLength;
			int length = (vi.isVisible ? 1 : 0);
			if (vi.left != null) {
				length += vi.left.GetTotalListLength();
			}
			if (vi.right != null) {
				length += vi.right.GetTotalListLength();
			}
			return vi.totalListLength = length;
		}

		FileNodeBase RotateLeft() {
			/* Rotate tree to the left
			 * 
			 *       this               right
			 *       /  \               /  \
			 *      A   right   ===>  this  C
			 *           / \          / \
			 *          B   C        A   B
			 */
			FileNodeBase b = vi.right.vi.left;
			FileNodeBase newTop = vi.right;

			if (b != null) b.vi.listParent = this;
			this.vi.right = b;
			newTop.vi.left = this;
			newTop.vi.listParent = this.vi.listParent;
			this.vi.listParent = newTop;
			// rebalance the 'this' node - this is necessary in some bulk insertion cases:
			newTop.vi.left = Rebalance(this);
			return newTop;
		}

		FileNodeBase RotateRight() {
			/* Rotate tree to the right
			 * 
			 *       this             left
			 *       /  \             /  \
			 *     left  C   ===>    A   this
			 *     / \                   /  \
			 *    A   B                 B    C
			 */
			FileNodeBase b = vi.left.vi.right;
			FileNodeBase newTop = vi.left;

			if (b != null) b.vi.listParent = this;
			this.vi.left = b;
			newTop.vi.right = this;
			newTop.vi.listParent = this.vi.listParent;
			this.vi.listParent = newTop;
			newTop.vi.right = Rebalance(this);
			return newTop;
		}

		static void RebalanceUntilRoot(FileNodeBase pos) {
			while (pos.vi.listParent != null) {
				if (pos == pos.vi.listParent.vi.left) {
					pos = pos.vi.listParent.vi.left = Rebalance(pos);
				}
				else {
					Debug.Assert(pos == pos.vi.listParent.vi.right);
					pos = pos.vi.listParent.vi.right = Rebalance(pos);
				}
				pos = pos.vi.listParent;
			}
			FileNodeBase newRoot = Rebalance(pos);
			if (newRoot != pos && pos.vi.treeFlattener != null) {
				Debug.Assert(newRoot.vi.treeFlattener == null);
				newRoot.vi.treeFlattener = pos.vi.treeFlattener;
				pos.vi.treeFlattener = null;
				newRoot.vi.treeFlattener.root = newRoot;
			}
			Debug.Assert(newRoot.vi.listParent == null);
			newRoot.CheckInvariants();
		}
		#endregion

		#region Insertion
		static void InsertNodeAfter(FileNodeBase pos, FileNodeBase newNode) {
			// newNode might be the model root of a whole subtree, so go to the list root of that subtree:
			newNode = newNode.GetListRoot();
			if (pos.vi.right == null) {
				pos.vi.right = newNode;
				newNode.vi.listParent = pos;
			}
			else {
				// insert before pos.right's leftmost:
				pos = pos.vi.right;
				while (pos.vi.left != null)
					pos = pos.vi.left;
				Debug.Assert(pos.vi.left == null);
				pos.vi.left = newNode;
				newNode.vi.listParent = pos;
			}
			RebalanceUntilRoot(pos);
		}
		#endregion

		#region Removal
		void RemoveNodes(FileNodeBase start, FileNodeBase end) {
			// Removes all nodes from start to end (inclusive)
			// All removed nodes will be reorganized in a separate tree, do not delete
			// regions that don't belong together in the tree model!

			List<FileNodeBase> removedSubtrees = new List<FileNodeBase>();
			FileNodeBase oldPos;
			FileNodeBase pos = start;
			do {
				// recalculate the endAncestors every time, because the tree might have been rebalanced
				HashSet<FileNodeBase> endAncestors = new HashSet<FileNodeBase>();
				for (FileNodeBase tmp = end; tmp != null; tmp = tmp.vi.listParent)
					endAncestors.Add(tmp);

				removedSubtrees.Add(pos);
				if (!endAncestors.Contains(pos)) {
					// we can remove pos' right subtree in a single step:
					if (pos.vi.right != null) {
						removedSubtrees.Add(pos.vi.right);
						pos.vi.right.vi.listParent = null;
						pos.vi.right = null;
					}
				}
				FileNodeBase succ = pos.Successor();
				DeleteNode(pos); // this will also rebalance out the deletion of the right subtree

				oldPos = pos;
				pos = succ;
			} while (oldPos != end);

			// merge back together the removed subtrees:
			FileNodeBase removed = removedSubtrees[0];
			for (int i = 1; i < removedSubtrees.Count; i++) {
				removed = ConcatTrees(removed, removedSubtrees[i]);
			}
		}

		static FileNodeBase ConcatTrees(FileNodeBase first, FileNodeBase second) {
			FileNodeBase tmp = first;
			while (tmp.vi.right != null)
				tmp = tmp.vi.right;
			InsertNodeAfter(tmp, second);
			return tmp.GetListRoot();
		}

		FileNodeBase Successor() {
			if (vi.right != null) {
				FileNodeBase node = vi.right;
				while (node.vi.left != null)
					node = node.vi.left;
				return node;
			}
			else {
				FileNodeBase node = this;
				FileNodeBase oldNode;
				do {
					oldNode = node;
					node = node.vi.listParent;
					// loop while we are on the way up from the right part
				} while (node != null && node.vi.right == oldNode);
				return node;
			}
		}

		static void DeleteNode(FileNodeBase node) {
			FileNodeBase balancingNode;
			if (node.vi.left == null) {
				balancingNode = node.vi.listParent;
				node.ReplaceWith(node.vi.right);
				node.vi.right = null;
			}
			else if (node.vi.right == null) {
				balancingNode = node.vi.listParent;
				node.ReplaceWith(node.vi.left);
				node.vi.left = null;
			}
			else {
				FileNodeBase tmp = node.vi.right;
				while (tmp.vi.left != null)
					tmp = tmp.vi.left;
				// First replace tmp with tmp.right
				balancingNode = tmp.vi.listParent;
				tmp.ReplaceWith(tmp.vi.right);
				tmp.vi.right = null;
				Debug.Assert(tmp.vi.left == null);
				Debug.Assert(tmp.vi.listParent == null);
				// Now move node's children to tmp:
				tmp.vi.left = node.vi.left; node.vi.left = null;
				tmp.vi.right = node.vi.right; node.vi.right = null;
				if (tmp.vi.left != null) tmp.vi.left.vi.listParent = tmp;
				if (tmp.vi.right != null) tmp.vi.right.vi.listParent = tmp;
				// Then replace node with tmp
				node.ReplaceWith(tmp);
				if (balancingNode == node)
					balancingNode = tmp;
			}
			Debug.Assert(node.vi.listParent == null);
			Debug.Assert(node.vi.left == null);
			Debug.Assert(node.vi.right == null);
			node.vi.height = 1;
			node.vi.totalListLength = -1;
			if (balancingNode != null)
				RebalanceUntilRoot(balancingNode);
		}

		void ReplaceWith(FileNodeBase node) {
			if (vi.listParent != null) {
				if (vi.listParent.vi.left == this) {
					vi.listParent.vi.left = node;
				}
				else {
					Debug.Assert(vi.listParent.vi.right == this);
					vi.listParent.vi.right = node;
				}
				if (node != null)
					node.vi.listParent = vi.listParent;
				vi.listParent = null;
			}
			else {
				// this was a root node
				Debug.Assert(node != null); // cannot delete the only node in the tree
				node.vi.listParent = null;
				if (vi.treeFlattener != null) {
					Debug.Assert(node.vi.treeFlattener == null);
					node.vi.treeFlattener = this.vi.treeFlattener;
					this.vi.treeFlattener = null;
					node.vi.treeFlattener.root = node;
				}
			}
		}
		#endregion
	}
}
