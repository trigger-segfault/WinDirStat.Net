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

namespace WinDirStat.Net.ViewModel.Files {
	// This part of SharpTreeNode controls the 'flat list' data structure, which emulates
	// a big flat list containing the whole tree; allowing access by visible index.
	partial class FileItemViewModel {
		/// <summary>The parent in the flat list</summary>
		internal FileItemViewModel listParent;
		/// <summary>Left/right nodes in the flat list</summary>
		private FileItemViewModel left, right;

		internal FileTreeFlattener treeFlattener;

		/// <summary>Subtree height in the flat list tree</summary>
		private byte height = 1;

		/// <summary>Length in the flat list, including children (children within the flat list). -1 = invalidated</summary>
		private int totalListLength = -1;

		private int Balance {
			get { return Height(right) - Height(left); }
		}

		static int Height(FileItemViewModel node) {
			return node != null ? node.height : 0;
		}

		internal FileItemViewModel GetListRoot() {
			FileItemViewModel node = this;
			while (node.listParent != null)
				node = node.listParent;
			return node;
		}

		#region Debugging
		[Conditional("DEBUG")]
		void CheckRootInvariants() {
			GetListRoot().CheckInvariants();
		}

		[Conditional("DATACONSISTENCYCHECK")]
		void CheckInvariants() {
			Debug.Assert(left == null || left.listParent == this);
			Debug.Assert(right == null || right.listParent == this);
			Debug.Assert(height == 1 + Math.Max(Height(left), Height(right)));
			Debug.Assert(Math.Abs(this.Balance) <= 1);
			Debug.Assert(totalListLength == -1 || totalListLength == (left != null ? left.totalListLength : 0) + (isVisible ? 1 : 0) + (right != null ? right.totalListLength : 0));
			if (left != null) left.CheckInvariants();
			if (right != null) right.CheckInvariants();
		}

		[Conditional("DEBUG")]
		static void DumpTree(FileItemViewModel node) {
			node.GetListRoot().DumpTree();
		}

		[Conditional("DEBUG")]
		void DumpTree() {
			Debug.Indent();
			if (left != null)
				left.DumpTree();
			Debug.Unindent();
			Debug.WriteLine("{0}, totalListLength={1}, height={2}, Balance={3}, isVisible={4}", ToString(), totalListLength, height, Balance, isVisible);
			Debug.Indent();
			if (right != null)
				right.DumpTree();
			Debug.Unindent();
		}
		#endregion

		#region GetNodeByVisibleIndex / GetVisibleIndexForNode
		internal static FileItemViewModel GetNodeByVisibleIndex(FileItemViewModel root, int index) {
			root.GetTotalListLength(); // ensure all list lengths are calculated
			Debug.Assert(index >= 0);
			Debug.Assert(index < root.totalListLength);
			FileItemViewModel node = root;
			while (true) {
				if (node.left != null && index < node.left.totalListLength) {
					node = node.left;
				}
				else {
					if (node.left != null) {
						index -= node.left.totalListLength;
					}
					if (node.isVisible) {
						if (index == 0)
							return node;
						index--;
					}
					node = node.right;
				}
			}
		}

		internal static int GetVisibleIndexForNode(FileItemViewModel node) {
			int index = node.left != null ? node.left.GetTotalListLength() : 0;
			while (node.listParent != null) {
				if (node == node.listParent.right) {
					if (node.listParent.left != null)
						index += node.listParent.left.GetTotalListLength();
					if (node.listParent.isVisible)
						index++;
				}
				node = node.listParent;
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
		static FileItemViewModel Rebalance(FileItemViewModel node) {
			Debug.Assert(node.left == null || Math.Abs(node.left.Balance) <= 1);
			Debug.Assert(node.right == null || Math.Abs(node.right.Balance) <= 1);
			// Keep looping until it's balanced. Not sure if this is stricly required; this is based on
			// the Rope code where node merging made this necessary.
			while (Math.Abs(node.Balance) > 1) {
				// AVL balancing
				// note: because we don't care about the identity of concat nodes, this works a little different than usual
				// tree rotations: in our implementation, the "this" node will stay at the top, only its children are rearranged
				if (node.Balance > 1) {
					if (node.right.Balance < 0) {
						node.right = node.right.RotateRight();
					}
					node = node.RotateLeft();
					// If 'node' was unbalanced by more than 2, we've shifted some of the inbalance to the left node; so rebalance that.
					node.left = Rebalance(node.left);
				}
				else if (node.Balance < -1) {
					if (node.left.Balance > 0) {
						node.left = node.left.RotateLeft();
					}
					node = node.RotateRight();
					// If 'node' was unbalanced by more than 2, we've shifted some of the inbalance to the right node; so rebalance that.
					node.right = Rebalance(node.right);
				}
			}
			Debug.Assert(Math.Abs(node.Balance) <= 1);
			node.height = (byte) (1 + Math.Max(Height(node.left), Height(node.right)));
			node.totalListLength = -1; // mark for recalculation
									   // since balancing checks the whole tree up to the root, the whole path will get marked as invalid
			return node;
		}

		internal int GetTotalListLength() {
			if (totalListLength >= 0)
				return totalListLength;
			int length = (isVisible ? 1 : 0);
			if (left != null) {
				length += left.GetTotalListLength();
			}
			if (right != null) {
				length += right.GetTotalListLength();
			}
			return totalListLength = length;
		}

		FileItemViewModel RotateLeft() {
			/* Rotate tree to the left
			 * 
			 *       this               right
			 *       /  \               /  \
			 *      A   right   ===>  this  C
			 *           / \          / \
			 *          B   C        A   B
			 */
			FileItemViewModel b = right.left;
			FileItemViewModel newTop = right;

			if (b != null) b.listParent = this;
			this.right = b;
			newTop.left = this;
			newTop.listParent = this.listParent;
			this.listParent = newTop;
			// rebalance the 'this' node - this is necessary in some bulk insertion cases:
			newTop.left = Rebalance(this);
			return newTop;
		}

		FileItemViewModel RotateRight() {
			/* Rotate tree to the right
			 * 
			 *       this             left
			 *       /  \             /  \
			 *     left  C   ===>    A   this
			 *     / \                   /  \
			 *    A   B                 B    C
			 */
			FileItemViewModel b = left.right;
			FileItemViewModel newTop = left;

			if (b != null) b.listParent = this;
			this.left = b;
			newTop.right = this;
			newTop.listParent = this.listParent;
			this.listParent = newTop;
			newTop.right = Rebalance(this);
			return newTop;
		}

		static void RebalanceUntilRoot(FileItemViewModel pos) {
			while (pos.listParent != null) {
				if (pos == pos.listParent.left) {
					pos = pos.listParent.left = Rebalance(pos);
				}
				else {
					Debug.Assert(pos == pos.listParent.right);
					pos = pos.listParent.right = Rebalance(pos);
				}
				pos = pos.listParent;
			}
			FileItemViewModel newRoot = Rebalance(pos);
			if (newRoot != pos && pos.treeFlattener != null) {
				Debug.Assert(newRoot.treeFlattener == null);
				newRoot.treeFlattener = pos.treeFlattener;
				pos.treeFlattener = null;
				newRoot.treeFlattener.root = newRoot;
			}
			Debug.Assert(newRoot.listParent == null);
			newRoot.CheckInvariants();
		}
		#endregion

		#region Insertion
		static void InsertNodeAfter(FileItemViewModel pos, FileItemViewModel newNode) {
			// newNode might be the model root of a whole subtree, so go to the list root of that subtree:
			newNode = newNode.GetListRoot();
			if (pos.right == null) {
				pos.right = newNode;
				newNode.listParent = pos;
			}
			else {
				// insert before pos.right's leftmost:
				pos = pos.right;
				while (pos.left != null)
					pos = pos.left;
				Debug.Assert(pos.left == null);
				pos.left = newNode;
				newNode.listParent = pos;
			}
			RebalanceUntilRoot(pos);
		}
		#endregion

		#region Removal
		void RemoveNodes(FileItemViewModel start, FileItemViewModel end) {
			// Removes all nodes from start to end (inclusive)
			// All removed nodes will be reorganized in a separate tree, do not delete
			// regions that don't belong together in the tree model!

			List<FileItemViewModel> removedSubtrees = new List<FileItemViewModel>();
			FileItemViewModel oldPos;
			FileItemViewModel pos = start;
			do {
				// recalculate the endAncestors every time, because the tree might have been rebalanced
				HashSet<FileItemViewModel> endAncestors = new HashSet<FileItemViewModel>();
				for (FileItemViewModel tmp = end; tmp != null; tmp = tmp.listParent)
					endAncestors.Add(tmp);

				removedSubtrees.Add(pos);
				if (!endAncestors.Contains(pos)) {
					// we can remove pos' right subtree in a single step:
					if (pos.right != null) {
						removedSubtrees.Add(pos.right);
						pos.right.listParent = null;
						pos.right = null;
					}
				}
				FileItemViewModel succ = pos.Successor();
				DeleteNode(pos); // this will also rebalance out the deletion of the right subtree

				oldPos = pos;
				pos = succ;
			} while (oldPos != end);

			// merge back together the removed subtrees:
			FileItemViewModel removed = removedSubtrees[0];
			for (int i = 1; i < removedSubtrees.Count; i++) {
				removed = ConcatTrees(removed, removedSubtrees[i]);
			}
		}

		static FileItemViewModel ConcatTrees(FileItemViewModel first, FileItemViewModel second) {
			FileItemViewModel tmp = first;
			while (tmp.right != null)
				tmp = tmp.right;
			InsertNodeAfter(tmp, second);
			return tmp.GetListRoot();
		}

		FileItemViewModel Successor() {
			if (right != null) {
				FileItemViewModel node = right;
				while (node.left != null)
					node = node.left;
				return node;
			}
			else {
				FileItemViewModel node = this;
				FileItemViewModel oldNode;
				do {
					oldNode = node;
					node = node.listParent;
					// loop while we are on the way up from the right part
				} while (node != null && node.right == oldNode);
				return node;
			}
		}

		static void DeleteNode(FileItemViewModel node) {
			FileItemViewModel balancingNode;
			if (node.left == null) {
				balancingNode = node.listParent;
				node.ReplaceWith(node.right);
				node.right = null;
			}
			else if (node.right == null) {
				balancingNode = node.listParent;
				node.ReplaceWith(node.left);
				node.left = null;
			}
			else {
				FileItemViewModel tmp = node.right;
				while (tmp.left != null)
					tmp = tmp.left;
				// First replace tmp with tmp.right
				balancingNode = tmp.listParent;
				tmp.ReplaceWith(tmp.right);
				tmp.right = null;
				Debug.Assert(tmp.left == null);
				Debug.Assert(tmp.listParent == null);
				// Now move node's children to tmp:
				tmp.left = node.left; node.left = null;
				tmp.right = node.right; node.right = null;
				if (tmp.left != null) tmp.left.listParent = tmp;
				if (tmp.right != null) tmp.right.listParent = tmp;
				// Then replace node with tmp
				node.ReplaceWith(tmp);
				if (balancingNode == node)
					balancingNode = tmp;
			}
			Debug.Assert(node.listParent == null);
			Debug.Assert(node.left == null);
			Debug.Assert(node.right == null);
			node.height = 1;
			node.totalListLength = -1;
			if (balancingNode != null)
				RebalanceUntilRoot(balancingNode);
		}

		void ReplaceWith(FileItemViewModel node) {
			if (listParent != null) {
				if (listParent.left == this) {
					listParent.left = node;
				}
				else {
					Debug.Assert(listParent.right == this);
					listParent.right = node;
				}
				if (node != null)
					node.listParent = listParent;
				listParent = null;
			}
			else {
				// this was a root node
				Debug.Assert(node != null); // cannot delete the only node in the tree
				node.listParent = null;
				if (treeFlattener != null) {
					Debug.Assert(node.treeFlattener == null);
					node.treeFlattener = this.treeFlattener;
					this.treeFlattener = null;
					node.treeFlattener.root = node;
				}
			}
		}
		#endregion
	}
}
