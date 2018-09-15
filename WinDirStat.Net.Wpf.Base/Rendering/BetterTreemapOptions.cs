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
	public struct BetterTreemapOptions {

		/// <summary>Squarification method</summary>
		private TreemapStyle style;
		/// <summary>0..1.0   (default = 0.84)</summary>
		private Number brightness;
		/// <summary>0..oo    (default = 0.40)    Factor "H"</summary>
		private Number height;
		/// <summary>0..1.0   (default = 0.90)    Factor "F"</summary>
		private Number scaleFactor;
		/// <summary>0..1.0   (default = 0.15)    Factor "Ia"</summary>
		private Number ambientLight;
		/// <summary>(-4.0,-4.0)..(+4.0,+4.0) (default = (-1.0,-1.0), negative = top-left</summary>
		private Point2F lightSource;

		/// <summary>Squarification method</summary>
		public TreemapStyle Style {
			get => style;
			set {
				if (!Enum.IsDefined(typeof(TreemapStyle), value))
					throw new ArgumentException(nameof(Style));
				style = value;
			}
		}
		/// <summary>Whether or not to draw grid lines</summary>
		public bool Grid { get; set; }
		/// <summary>Color of grid lines</summary>
		public Rgb24Color GridColor { get; set; }

		/// <summary>0..1.0   (default = 0.84)</summary>
		public Number Brightness {
			get => brightness;
			set => brightness = MathUtils.Clamp(value, 0, 1);
		}
		/// <summary>0..oo    (default = 0.40)    Factor "H"</summary>
		public Number Height {
			get => height;
			set => height = Math.Max(value, 0);
		}
		/// <summary>0..1.0   (default = 0.90)    Factor "F"</summary>
		public Number ScaleFactor {
			get => scaleFactor;
			set => scaleFactor = MathUtils.Clamp(value, 0, 1);
		}
		/// <summary>0..1.0   (default = 0.15)    Factor "Ia"</summary>
		public Number AmbientLight {
			get => ambientLight;
			set => ambientLight = MathUtils.Clamp(value, 0, 1);
		}

		/// <summary>(-4.0,-4.0)..(+4.0,+4.0) (default = (-1.0,-1.0), negative = top-left</summary>
		public Point2F LightSource {
			get => lightSource;
			set {
				LightSourceX = value.X;
				LightSourceY = value.Y;
			}
		}
		/// <summary>-4.0..+4.0 (default = -1.0), negative = left</summary>
		public Number LightSourceX {
			get => lightSource.X;
			set => lightSource.X = MathUtils.Clamp(value, -4, 4);
		}
		/// <summary>-4.0..+4.0 (default = -1.0), negative = top</summary>
		public Number LightSourceY {
			get => lightSource.Y;
			set => lightSource.Y = MathUtils.Clamp(value, -4, 4);
		}
	}
}
