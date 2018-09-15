using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.Rendering {
	/// <summary>A factory for treemap renderers.</summary>
	public class TreemapRendererFactory {

		#region Fields

		/// <summary>The UI service.</summary>
		private readonly UIService ui;

		#endregion

		#region Constructors

		/// <summary>Constructs the<see cref="TreemapRendererFactory"/>.</summary>
		public TreemapRendererFactory(UIService ui) {
			this.ui = ui;
		}

		#endregion

		#region TreemapRenderer Factory

		/// <summary>Constructs a new <see cref="TreemapRenderer"/>.</summary>
		public TreemapRenderer Create() {
			return new TreemapRenderer(ui);
		}

		#endregion
	}
}
