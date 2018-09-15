using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Structures;
using WinDirStat.Net.Utils;

#if DOUBLE
using Number = System.Double;
#else
using Number = System.Single;
#endif

namespace WinDirStat.Net.Rendering {
	/// <summary>A structure containing treemap rendering options.</summary>
	[Serializable]
	public struct TreemapOptions {

		public static readonly TreemapOptions Default = new TreemapOptions {
			Style = TreemapStyle.KDirStatStyle,
			Grid = false,
			GridColor = Rgb24Color.Black,
			Brightness = (Number) 0.88,
			Height = (Number) 0.38,
			ScaleFactor = (Number) 0.91,
			AmbientLight = (Number) 0.13,
			LightSourceX = -1,
			LightSourceY = -1,
		};

		/// <summary>Squarification method</summary>
		public TreemapStyle Style;
		/// <summary>Whether or not to draw grid lines</summary>
		public bool Grid;
		/// <summary>Color of grid lines</summary>
		public Rgb24Color GridColor;
		/// <summary>0..1.0   (default = 0.84)</summary>
		public Number Brightness;
		/// <summary>0..oo    (default = 0.40)    Factor "H"</summary>
		public Number Height;
		/// <summary>0..1.0   (default = 0.90)    Factor "F"</summary>
		public Number ScaleFactor;
		/// <summary>0..1.0   (default = 0.15)    Factor "Ia"</summary>
		public Number AmbientLight;
		/// <summary>-4.0..+4.0 (default = -1.0), negative = left</summary>
		public Number LightSourceX;
		/// <summary>-4.0..+4.0 (default = -1.0), negative = top</summary>
		public Number LightSourceY;

		public int BrightnessPercent {
			get => MathUtils.Round(Brightness * 100);
			set => Brightness = value / (Number) 100;
		}
		public int HeightPercent {
			get => MathUtils.Round(Height * 100);
			set => Height = value / (Number) 100;
		}
		public int ScaleFactorPercent {
			get => MathUtils.Round(ScaleFactor * 100);
			set => ScaleFactor = value / (Number) 100;
		}
		public int AmbientLightPercent {
			get => MathUtils.Round(AmbientLight * 100);
			set => AmbientLight = value / (Number) 100;
		}
		public int LightSourceXPercent {
			get => MathUtils.Round(LightSourceX * 100);
			set => LightSourceX = value / (Number) 100;
		}
		public int LightSourceYPercent {
			get => MathUtils.Round(LightSourceY * 100);
			set => LightSourceY = value / (Number) 100;
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
