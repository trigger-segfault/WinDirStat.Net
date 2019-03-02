using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Structures;

#if DOUBLE
using Number = System.Double;
using Point2N = WinDirStat.Net.Structures.Point2D;
#else
using Number = System.Single;
using Point2N = WinDirStat.Net.Structures.Point2F;
#endif

namespace WinDirStat.Net.Rendering {
	unsafe partial class TreemapRenderer {

		private void KDirStat_DrawChildren(Rgba32Color* bitmap, ITreemapItem parent, Number[] surface, Number h, uint flags) {
			Debug.Assert(parent.ChildCount > 0);

			Rectangle2I rc = parent.Rectangle;
			List<Number> rows = new List<Number>();
			List<int> childrenPerRow = new List<int>();

			Number[] childWidth = new Number[parent.ChildCount];

			bool horizontalRows = KDirStat_ArrangeChildren(parent, childWidth, rows, childrenPerRow);

			int width = horizontalRows ? rc.Width : rc.Height;
			int height = horizontalRows ? rc.Height : rc.Width;
			Debug.Assert(width >= 0);
			Debug.Assert(height >= 0);

			int c = 0;
			Number top = horizontalRows ? rc.Top : rc.Left;
			for (int row = 0; row < rows.Count; row++) {
				Number fBottom = top + rows[row] * height;
				int bottom = (int) fBottom;
				if (row == rows.Count - 1) {
					bottom = horizontalRows ? rc.Bottom : rc.Right;
				}
				Number left = horizontalRows ? rc.Left : rc.Top;
				for (int i = 0; i < childrenPerRow[row]; i++, c++) {
					ITreemapItem child = parent[c];
					Debug.Assert(childWidth[c] >= 0);
					Number fRight = left + childWidth[c] * width;
					int right = (int) fRight;

					bool lastChild = (i == childrenPerRow[row] - 1 || childWidth[c + 1] == 0);

					if (lastChild)
						right = horizontalRows ? rc.Right : rc.Bottom;

					Rectangle2I rcChild;
					if (horizontalRows) {
						rcChild = Rectangle2I.FromLTRB((int) left, (int) top, right, bottom);
					}
					else {
						rcChild = Rectangle2I.FromLTRB((int) top, (int) left, bottom, right);
					}

#if DEBUG
					if (rcChild.Width > 0 && rcChild.Height > 0) {
						//Rectangle2I test;
						//test.IntersectRect(parent->TmiGetRectangle(), rcChild);
						//Debug.Assert(test == rcChild);
					}
#endif
					RecurseDrawGraph(bitmap, child, rcChild, false, surface, h * options.ScaleFactor, 0);

					if (lastChild) {
						i++; c++;

						if (i < childrenPerRow[row]) {
							parent[c].Rectangle = Rectangle2I.FromLTRB(-1, -1, -1, -1);
						}

						c += childrenPerRow[row] - i;
						break;
					}

					left = fRight;
				}
				top = fBottom;
			}
		}

		private bool KDirStat_ArrangeChildren(ITreemapItem parent, Number[] childWidth, List<Number> rows, List<int> childrenPerRow) {
			Debug.Assert(!parent.IsLeaf);
			Debug.Assert(parent.ChildCount > 0);

			if (parent.Size == 0) {
				rows.Add(1);
				childrenPerRow.Add(parent.ChildCount);
				for (int i = 0; i < parent.ChildCount; i++) {
					childWidth[i] = 1 / parent.ChildCount;
				}
				return true;
			}

			bool horizontalRows = parent.Rectangle.Width >= parent.Rectangle.Height;

			Number width = 1;
			if (horizontalRows) {
				if (parent.Rectangle.Height > 0)
					width = (Number) parent.Rectangle.Width / parent.Rectangle.Height;
			}
			else {
				if (parent.Rectangle.Width > 0)
					width = (Number) parent.Rectangle.Height / parent.Rectangle.Width;
			}

			int nextChild = 0;
			while (nextChild < parent.ChildCount) {
				rows.Add(KDirStat_CalculateNextRow(parent, nextChild, width, out int childrenUsed, childWidth));
				childrenPerRow.Add(childrenUsed);
				nextChild += childrenUsed;
			}

			return horizontalRows;
		}

		private Number KDirStat_CalculateNextRow(ITreemapItem parent, int nextChild, Number width, out int childrenUsed, Number[] childWidth) {
			int i = 0;
			const Number minProportion = (Number) 0.4;
			Debug.Assert(minProportion < 1);

			Debug.Assert(nextChild < parent.ChildCount);
			Debug.Assert(width >= 1);

			Number mySize = parent.Size;
			Debug.Assert(mySize > 0);
			long sizeUsed = 0;
			Number rowHeight = 0;

			for (i = nextChild; i < parent.ChildCount; i++) {
				long childSize = parent[i].Size;
				if (childSize == 0) {
					Debug.Assert(i > nextChild);
					break;
				}

				sizeUsed += childSize;
				Number virtualRowHeight = sizeUsed / mySize;
				Debug.Assert(virtualRowHeight > 0);
				Debug.Assert(virtualRowHeight <= 1);

				// Rectangle2I(mySize)    = width * 1.0
				// Rectangle2I(childSize) = childWidth * virtualRowHeight
				// Rectangle2I(childSize) = childSize / mySize * width

				Number childWidth_ = childSize / mySize * width / virtualRowHeight;

				if (childWidth_ / virtualRowHeight < minProportion) {
					Debug.Assert(i > nextChild); // because width >= 1 and _minProportion < 1.
					// For the first child we have:
					// childWidth / rowHeight
					// = childSize / mySize * width / rowHeight / rowHeight
					// = childSize * width / sizeUsed / sizeUsed * mySize
					// > childSize * mySize / sizeUsed / sizeUsed
					// > childSize * childSize / childSize / childSize
					// = 1 > _minProportion.
					break;
				}
				rowHeight = virtualRowHeight;
			}
			Debug.Assert(i > nextChild);

			while (i < parent.ChildCount && parent[i].Size == 0) {
				i++;
			}

			childrenUsed = i - nextChild;

			for (i = 0; i < childrenUsed; i++) {
				// Rectangle2I(1 * 1) = mySize
				Number rowSize = mySize * rowHeight;
				Number childSize = parent[nextChild + i].Size;
				Number cw = childSize / rowSize;
				Debug.Assert(cw >= 0);
				childWidth[nextChild + i] = cw;
			}

			return rowHeight;
		}

	}
}
