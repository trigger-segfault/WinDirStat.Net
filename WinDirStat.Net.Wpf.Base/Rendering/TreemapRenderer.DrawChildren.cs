using System;
using System.Diagnostics;
using WinDirStat.Net.Structures;

#if DOUBLE
using Number = System.Double;
#else
using Number = System.Single;
#endif

namespace WinDirStat.Net.Rendering {
    partial class TreemapRenderer {
		
		private void RecurseDrawGraph(Span<Rgba32Color> bitmap, ITreemapItem item, Rectangle2I rc, bool isroot, Number[] pSurface, Number h, uint flags)
		{
			Debug.Assert(rc.Width >= 0);
			Debug.Assert(rc.Height >= 0);
			Debug.Assert(item.Size > 0);

			item.Rectangle = rc;

			int gridWidth = options.Grid ? 1 : 0;

			if (rc.Width <= gridWidth || rc.Height <= gridWidth)
				return;

			Number[] surface = new Number[] { 0, 0, 0, 0 };
			if (IsCushionShading) {
				Array.Copy(pSurface, surface, pSurface.Length);

				if (!isroot)
					AddRidge(rc, surface, h);
			}

			if (item.IsLeaf) {
				RenderLeaf(bitmap, item, surface);
			}
			else {
				Debug.Assert(item.ChildCount > 0);
				Debug.Assert(item.Size > 0);

				DrawChildren(bitmap, item, surface, h, flags);
			}
		}

		private void DrawChildren(Span<Rgba32Color> bitmap, ITreemapItem parent, Number[] surface, Number h, uint flags) {
			switch (options.Style) {
			case TreemapStyle.KDirStatStyle:
				KDirStat_DrawChildren(bitmap, parent, surface, h, flags);
				break;
			case TreemapStyle.SequoiaViewStyle:
				throw new NotImplementedException("SequoiaViewStyle");
				//SequoiaView_DrawwChildren(bitmap, parent, surface, h, flags);
				//break;
			case TreemapStyle.SimpleStyle:
				throw new NotImplementedException("SimpleStyle");
				//Simple_DrawwChildren(bitmap, parent, surface, h, flags);
				//break;
			}
		}


		private bool IsCushionShading {
			get => options.AmbientLight < 1 && options.Height > 0 && options.ScaleFactor > 0;
		}

		private void RenderLeaf(Span<Rgba32Color> bitmap, ITreemapItem item, Number[] surface) {
			Rectangle2I rc = item.Rectangle;

			if (options.Grid) {
				rc.Y++;
				rc.X++;
				rc.Width--;
				rc.Height--;
				if (rc.Width <= 0 || rc.Height <= 0)
					return;
			}

			RenderRectangle(bitmap, rc, surface, item.Color);
		}

		private void RenderRectangle(Span<Rgba32Color> bitmap, Rectangle2I rc, Number[] surface, Rgba32Color color) {
			Number brightness = options.Brightness;

			//color = ColorSpace.SetBrightness(color, PaletteBrightness);
			//brightness *= (Number) 0.66;

			if (IsCushionShading) {
				DrawCushion(bitmap, rc, surface, color, brightness);
			}
			else {
				DrawSolidRect(bitmap, rc, color, brightness);
			}
		}

		private void DrawSolidRect(Span<Rgba32Color> bitmap, Rectangle2I rc, Rgba32Color color, Number brightness) {
			Number factor = brightness / ColorSpace.PaletteBrightness;

			int red		= (int) (color.R * factor);
			int green	= (int) (color.G * factor);
			int blue	= (int) (color.B * factor);
			color = new Rgba32Color(red, green, blue);

			ColorSpace.NormalizeColor(ref red, ref green, ref blue);

			for (int iy = rc.Top; iy < rc.Bottom; iy++) {
				for (int ix = rc.Left; ix < rc.Right; ix++) {
					bitmap[ix + iy * renderArea.Width] = color;
				}
			}
		}
		private void DrawCushion(Span<Rgba32Color> bitmap, Rectangle2I rc, Number[] surface, Rgba32Color color, Number brightness) {
			Number Ia = options.AmbientLight;

			Number Is = 1 - Ia;

			Number r = color.R;
			Number g = color.G;
			Number b = color.B;

			for (int iy = rc.Top; iy < rc.Bottom; iy++) {
				for (int ix = rc.Left; ix < rc.Right; ix++) {
					Number nx = (-(2 * surface[0] * (ix + (Number) 0.5) + surface[2]));
					Number ny = (-(2 * surface[1] * (iy + (Number) 0.5) + surface[3]));
					Number cosa = Math.Min(1, (Number) ((nx*lx + ny*ly + lz) / Math.Sqrt(nx*nx + ny*ny + 1)));

					Number pixel = Math.Max(0, Is * cosa);

					pixel += Ia;
					Debug.Assert(pixel <= 1);

					// Now, pixel is the brightness of the pixel, 0...1.0.

					// Apply contrast.
					// Not implemented.
					// Costs performance and nearly the same effect can be
					// made width the m_options->ambientLight parameter.
					// pixel= pow(pixel, m_options->contrast);

					// Apply "brightness"
					pixel *= brightness / ColorSpace.PaletteBrightness;

					int red		= (int) (r * pixel);
					int green	= (int) (g * pixel);
					int blue	= (int) (b * pixel);

					ColorSpace.NormalizeColor(ref red, ref green, ref blue);

					bitmap[ix + iy * renderArea.Width] = new Rgba32Color(red, green, blue);
				}
			}
		}

		private void AddRidge(Rectangle2I rc, Number[] surface, Number h) {
			/*
			Unoptimized:

			if(rc.Width() > 0)
			{
				surface[2] += 4 * h * (rc.right + rc.left) / (rc.right - rc.left);
				surface[0] -= 4 * h / (rc.right - rc.left);
			}

			if(rc.Height() > 0)
			{
				surface[3] += 4 * h * (rc.bottom + rc.top) / (rc.bottom - rc.top);
				surface[1] -= 4 * h / (rc.bottom - rc.top);
			}
			*/

			// Optimized (gained 15 ms of 1030):

			int width = rc.Width;
			int height = rc.Height;

			Debug.Assert(width > 0 && height > 0);

			Number h4 = 4 * h;

			Number wf = h4 / width;
			surface[2] += wf * (rc.Right + rc.Left);
			surface[0] -= wf;

			Number hf = h4 / height;
			surface[3] += hf * (rc.Bottom + rc.Top);
			surface[1] -= hf;
		}
	}
}
