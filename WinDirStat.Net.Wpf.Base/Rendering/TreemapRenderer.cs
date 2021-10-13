using System;
using System.Buffers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using WinDirStat.Net.Services;
using WinDirStat.Net.Structures;

#if DOUBLE
using Number = System.Double;
#else
using Number = System.Single;
#endif

namespace WinDirStat.Net.Rendering {
    /// <summary>A service for rendering WinDirStat treemaps.</summary>
    public unsafe partial class TreemapRenderer {

        #region Fields

        /// <summary>The UI service.</summary>
        private readonly UIService ui;

        /// <summary>The last pixel array used for drawing.</summary>
        private Rgba32Color[] pixels;

        /// <summary>The render options for the treemap.</summary>
        private TreemapOptions options;
        /// <summary>Calculated light position for the treemap.</summary>
        private Number lx;
        /// <summary>Calculated light position for the treemap.</summary>
        private Number ly;
        /// <summary>Calculated light position for the treemap.</summary>
        private Number lz;
        /// <summary>The render area for the current operation.</summary>
        private Rectangle2I renderArea;

        #endregion

        #region Constructors

        /// <summary>Constructs the <see cref="TreemapRenderer"/>.</summary>
        public TreemapRenderer(UIService ui) {
            this.ui = ui;
            Options = TreemapOptions.Default;
        }

        #endregion

        #region Properties

        /// <summary>Gets or sets the treemap options.</summary>
        public TreemapOptions Options {
            get => options;
            set {
                options = value;

                Number lx = options.LightSourceX;
                Number ly = options.LightSourceY;
                const Number lz = 10f;

                Number length = (Number) Math.Sqrt(lx * lx + ly * ly + lz * lz);
                this.lx = lx / length;
                this.ly = ly / length;
                this.lz = lz / length;
            }
        }

        #endregion

        private void InitPixels(Rectangle2I rc, Rgba32Color? background = null) {
            int pixelCount = rc.Width * rc.Height;
            if (pixels == null || pixels.Length != pixelCount) {
                if (pixels != null) {
                    ArrayPool<Rgba32Color>.Shared.Return(pixels);
                }
                pixels = ArrayPool<Rgba32Color>.Shared.Rent(pixelCount);
            }

            if (background.HasValue) {
                pixels.AsSpan(..pixelCount).Fill(background.Value);
            }
        }

        [Conditional("DEBUG")]
        private void RecurseCheckTree(ITreemapItem item) {
            if (item.IsLeaf) {
                Debug.Assert(item.ChildCount == 0);
            }
            else {
                // TODO: check that children are sorted by size.
                long sum = 0;
                for (int i = 0; i < item.ChildCount; i++) {
                    ITreemapItem child = item[i];
                    sum += child.Size;
                    RecurseCheckTree(child);
                }
                Debug.Assert(sum == item.Size);
            }
        }

        public void DrawTreemap(WriteableBitmap bitmap, Rectangle2I rc, ITreemapItem root) {
            RecurseCheckTree(root);

            Rectangle2I fullRc = rc;

            rc.Width--;
            rc.Height--;

            if (rc.Width <= 0 || rc.Height <= 0)
                return;

            renderArea = fullRc;

            if (root.Size == 0)
                InitPixels(fullRc, Rgba32Color.Black);
            else if (options.Grid)
                InitPixels(fullRc, options.GridColor);
            else
                InitPixels(fullRc, new Rgba32Color(160, 160, 160));

            // Recursively draw the tree graph
            if (root.Size > 0) {
                Number[] surface = { 0, 0, 0, 0 };
                Span<Rgba32Color> span = pixels;
                RecurseDrawGraph(span, root, rc, true, surface, options.Height, 0);
            }

            ui.Invoke(() => {
                fixed (Rgba32Color* pBitmapBits = pixels) {
                    bitmap.WritePixels((Int32Rect) fullRc, (IntPtr) pBitmapBits, fullRc.Width * fullRc.Height * sizeof(Rgba32Color), bitmap.BackBufferStride);
                }
            });
        }

        public void DrawColorPreview(WriteableBitmap bitmap, Rectangle2I rc, Rgb24Color color) {
            if (rc.Width <= 0 || rc.Height <= 0)
                return;

            renderArea = rc;

            // That bitmap in turn will be created from this array
            InitPixels(rc);

            Number[] surface = { 0, 0, 0, 0 };

            AddRidge(rc, surface, options.Height * options.ScaleFactor);

            Span<Rgba32Color> span = pixels;
            RenderRectangle(span, rc, surface, color);

            ui.Invoke(() => {
                fixed (Rgba32Color* pBitmapBits = pixels) {
                    bitmap.WritePixels((Int32Rect) rc, (IntPtr) pBitmapBits, rc.Width * rc.Height * sizeof(Rgba32Color), bitmap.BackBufferStride);
                }
            });
        }

        public static ITreemapItem FindItemAtPoint(ITreemapItem item, Point2I p) {
            if (item.IsLeaf) {
                return item;
            }
            else {
                for (int i = 0; i < item.ChildCount; i++) {
                    ITreemapItem child = item[i];
                    Rectangle2I rcChild = child.Rectangle;

                    if (rcChild.Contains(p))
                        return FindItemAtPoint(child, p);
                }
            }

            return null;
        }
    }
}
