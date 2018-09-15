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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace WinDirStat.Net.Wpf.Controls.FileList {
	class LinesRenderer : FrameworkElement {
		static LinesRenderer() {
			pen = new Pen(Brushes.LightGray, 1);
			pen.Freeze();
		}

		static Pen pen;

		FileTreeNodeView NodeView {
			get { return TemplatedParent as FileTreeNodeView; }
		}

		protected override void OnRender(DrawingContext dc) {
			double indent = NodeView.CalculateIndent();
			Point p = new Point(indent + 4.5, 0);
			double endY = Math.Floor(ActualHeight / 2) + 0.5;

			if (!NodeView.Node.IsRoot || NodeView.ParentTreeView.ShowRootExpander) {
				dc.DrawLine(pen, new Point(p.X, endY), new Point(p.X + 10.5, endY));
			}

			if (NodeView.Node.IsRoot) return;

			if (NodeView.Node.IsLast) {
				dc.DrawLine(pen, p, new Point(p.X, endY));
			}
			else {
				dc.DrawLine(pen, p, new Point(p.X, ActualHeight));
			}

			var current = NodeView.Node;
			while (true) {
				p.X -= 18;
				current = current.Parent;
				if (p.X < 0) break;
				if (!current.IsLast) {
					dc.DrawLine(pen, p, new Point(p.X, ActualHeight));
				}
			}
		}
	}
}
