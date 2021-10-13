using Microsoft.Toolkit.HighPerformance.Buffers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Structures;

#if DOUBLE
using Number = System.Double;
#else
using Number = System.Single;
#endif

namespace WinDirStat.Net.Rendering {
    partial class TreemapRenderer {
        unsafe void WritePixelSpan(MemoryOwner<Rgba32Color> pixels, WriteableBitmap bitmap, Rectangle2I rc) {
            fixed (Rgba32Color* pBuffer = pixels.Span) {
                bitmap.WritePixels((Int32Rect) rc, (IntPtr) pBuffer, rc.Width * rc.Height * sizeof(Rgba32Color), bitmap.BackBufferStride);
            }
        }

        public void HighlightItems(WriteableBitmap bitmap, Rectangle2I rc, Rgba32Color color, IEnumerable<ITreemapItem> items) {
            if (rc.Width <= 0 || rc.Height <= 0)
                return;

            renderArea = rc;

            using var pixels = InitPixels(rc, Rgba32Color.Transparent);

            var span = pixels.Span;
            foreach (FileItemBase item in items) {
                HighlightRectangle(span, item.Rectangle, color);
            }

            ui.Invoke(() => WritePixelSpan(pixels, bitmap, rc));
        }

        public void HighlightExtensions(WriteableBitmap bitmap, Rectangle2I rc, FileItemBase root, Rgba32Color color, string extension) {
            if (rc.Width <= 0 || rc.Height <= 0)
                return;

            renderArea = rc;

            using var pixels = InitPixels(rc, Rgba32Color.Transparent);
            RecurseHighlightExtensions(pixels.Span, root, color, extension);

            ui.Invoke(() => WritePixelSpan(pixels, bitmap, rc));
        }

        private void RecurseHighlightExtensions(Span<Rgba32Color> bitmap, FileItemBase parent, Rgba32Color color, string extension) {
            List<FileItemBase> children = parent.Children;
            int count = children.Count;
            for (int i = 0; i < count; i++) {
                FileItemBase child = children[i];
                Rectangle2S rc = child.Rectangle;
                if (rc.Width > 0 && rc.Height > 0) {
                    if (child.IsLeaf) {
                        if (child.Extension == extension)
                            HighlightRectangle(bitmap, rc, color);
                    }
                    else {
                        RecurseHighlightExtensions(bitmap, child, color, extension);
                    }
                }
            }
        }

        private void HighlightRectangle(Span<Rgba32Color> bitmap, Rectangle2I rc, Rgba32Color color) {
            if (rc.Width >= 7 && rc.Height >= 7) {
                FillRectangle(bitmap, Rectangle2I.FromLTRB(rc.Left, rc.Top, rc.Right, rc.Top + 3), color);
                FillRectangle(bitmap, Rectangle2I.FromLTRB(rc.Left, rc.Bottom - 3, rc.Right, rc.Bottom), color);
                FillRectangle(bitmap, Rectangle2I.FromLTRB(rc.Left, rc.Top + 3, rc.Left + 3, rc.Bottom - 3), color);
                FillRectangle(bitmap, Rectangle2I.FromLTRB(rc.Right - 3, rc.Top + 3, rc.Right, rc.Bottom - 3), color);
            }
            else if (rc.Width == 1 && rc.Height == 1) {
                bitmap[rc.Left + rc.Top * renderArea.Width] = color;
            }
            else if (rc.Width > 0 && rc.Height > 0) {
                FillRectangle(bitmap, rc, color);
            }
        }

        private void FillRectangle(Span<Rgba32Color> bitmap, Rectangle2I rc, Rgba32Color color) {
            int bottom = rc.Bottom;
            int right = rc.Right;
            if (rc.Width >= rc.Height) {
                for (int iy = rc.Top; iy < bottom; iy++) {
                    for (int ix = rc.Left; ix < right; ix++) {
                        bitmap[ix + iy * renderArea.Width] = color;
                    }
                }
            }
            else {
                for (int ix = rc.Left; ix < right; ix++) {
                    for (int iy = rc.Top; iy < bottom; iy++) {
                        bitmap[ix + iy * renderArea.Width] = color;
                    }
                }
            }
        }
    }
}
