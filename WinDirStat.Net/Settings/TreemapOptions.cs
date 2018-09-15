using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Settings.Geometry;
using WinDirStat.Net.Utils;

#if FLOAT
using Number = System.Single;
#else
using Number = System.Double;
#endif

namespace WinDirStat.Net.Settings {
	/// <summary>A structure containing treemap rendering options.</summary>
	[Serializable]
	public struct TreemapOptions {

		public static readonly TreemapOptions Default = new TreemapOptions {
			Style = TreemapStyle.KDirStatStyle,
			Grid = false,
			GridColor = Rgb24Color.Black,
			Brightness = 0.88f,
			Height = 0.38f,
			ScaleFactor = 0.91f,
			AmbientLight = 0.13f,
			LightSourceX = -1f,
			LightSourceY = -1f,
		};

		/// <summary>Squarification method</summary>
		public TreemapStyle Style;
		/// <summary>Whether or not to draw grid lines</summary>
		public bool Grid;
		/// <summary>Color of grid lines</summary>
		public Rgb24Color GridColor;
		/// <summary>0..1.0   (default = 0.84)</summary>
		public float Brightness;
		/// <summary>0..oo    (default = 0.40)    Factor "H"</summary>
		public float Height;
		/// <summary>0..1.0   (default = 0.90)    Factor "F"</summary>
		public float ScaleFactor;
		/// <summary>0..1.0   (default = 0.15)    Factor "Ia"</summary>
		public float AmbientLight;
		/// <summary>-4.0..+4.0 (default = -1.0), negative = left</summary>
		public float LightSourceX;
		/// <summary>-4.0..+4.0 (default = -1.0), negative = top</summary>
		public float LightSourceY;

		public int BrightnessPercent {
			get => MathUtils.Round(Brightness * 100);
			set => Brightness = value / 100f;
		}
		public int HeightPercent {
			get => MathUtils.Round(Height * 100);
			set => Height = value / 100f;
		}
		public int ScaleFactorPercent {
			get => MathUtils.Round(ScaleFactor * 100);
			set => ScaleFactor = value / 100f;
		}
		public int AmbientLightPercent {
			get => MathUtils.Round(AmbientLight * 100);
			set => AmbientLight = value / 100f;
		}
		public int LightSourceXPercent {
			get => MathUtils.Round(LightSourceX * 100);
			set => LightSourceX = value / 100f;
		}
		public int LightSourceYPercent {
			get => MathUtils.Round(LightSourceY * 100);
			set => LightSourceY = value / 100f;
		}
		public Point2I LightSourcePoint {
			get => new Point2I(LightSourceXPercent, LightSourceYPercent);
			set {
				LightSourceXPercent = value.X;
				LightSourceYPercent = value.Y;
			}
		}
	}
}
