using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Services {
	/// <summary>A service for image references.</summary>
	public interface IImagesService {

		#region File Icon Fields

		/// <summary>Gets the icon for file collection items.</summary>
		IImage FileCollection { get; }
		/// <summary>Gets the icon for free space items.</summary>
		IImage FreeSpace { get; }
		/// <summary>Gets the icon for unknown space items.</summary>
		IImage UnknownSpace { get; }
		/// <summary>Gets the icon for files that no longer exist.</summary>
		IImage Missing { get; }

		#endregion
		
	}
}
