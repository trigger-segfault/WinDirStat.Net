﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
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
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using WinDirStat.Net.ViewModel.Files;

namespace WinDirStat.Net.Wpf.Controls.FileList {
	public class FileTreeViewItem : ListViewItem {
		static FileTreeViewItem() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FileTreeViewItem),
													 new FrameworkPropertyMetadata(typeof(FileTreeViewItem)));
		}

        public FileItemViewModel Node => DataContext as FileItemViewModel;

        public FileTreeNodeView NodeView { get; internal set; }
		public FileTreeView ParentTreeView { get; internal set; }

		protected override void OnKeyDown(KeyEventArgs e) {
			switch (e.Key) {
			case Key.F2:
				if (Node.IsEditable && ParentTreeView != null && ParentTreeView.SelectedItems.Count == 1 && ParentTreeView.SelectedItems[0] == Node) {
					Node.IsEditing = true;
					e.Handled = true;
				}
				break;
			case Key.Escape:
				if (Node.IsEditing) {
					Node.IsEditing = false;
					e.Handled = true;
				}
				break;
			}
		}

		#region Mouse

		Point startPoint;
		bool wasSelected;
		bool wasDoubleClick;

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			wasSelected = IsSelected;
			if (!IsSelected) {
				base.OnMouseLeftButtonDown(e);
			}

			if (Mouse.LeftButton == MouseButtonState.Pressed) {
				startPoint = e.GetPosition(null);
				CaptureMouse();

				if (e.ClickCount == 2) {
					wasDoubleClick = true;
				}
			}
		}

		/*protected override void OnMouseMove(MouseEventArgs e) {
			if (IsMouseCaptured) {
				var currentPoint = e.GetPosition(null);
				if (Math.Abs(currentPoint.X - startPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs(currentPoint.Y - startPoint.Y) >= SystemParameters.MinimumVerticalDragDistance) {

					var selection = ParentTreeView.GetTopLevelSelection().ToArray();
					Node.StartDrag(this, selection);
				}
			}
		}*/

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            //this can happen when Node is a DisconnectedItem
            if(Node == null) {
                return;
            }

			if (wasDoubleClick) {
				wasDoubleClick = false;
				Node.ActivateItem();
				if (!e.Handled) {
					if (!Node.IsRoot || ParentTreeView.ShowRootExpander) {
						Node.IsExpanded = !Node.IsExpanded;
					}
				}
			}

			ReleaseMouseCapture();
			if (wasSelected) {
				base.OnMouseLeftButtonDown(e);
			}
		}

		protected override void OnContextMenuOpening(ContextMenuEventArgs e) {
			if (Node != null)
				Node.ShowContextMenu(e);
		}

		#endregion

		#region Drag and Drop

		/*protected override void OnDragEnter(DragEventArgs e) {
			ParentTreeView.HandleDragEnter(this, e);
		}

		protected override void OnDragOver(DragEventArgs e) {
			ParentTreeView.HandleDragOver(this, e);
		}

		protected override void OnDrop(DragEventArgs e) {
			ParentTreeView.HandleDrop(this, e);
		}

		protected override void OnDragLeave(DragEventArgs e) {
			ParentTreeView.HandleDragLeave(this, e);
		}*/

		#endregion
	}
}
