using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	public static class WpfUtils {
		
		/// <summary>Creates a pack Uri for loading resource images.</summary>
		public static Uri MakePackUri(string resourcePath, Assembly assembly = null) {
			assembly = assembly ?? Assembly.GetCallingAssembly();
			// Pull out the short name.
			string assemblyShortName = assembly.ToString().Split(',')[0];
			string uriString = $"pack://application:,,,/{assemblyShortName};component/{resourcePath}";
			return new Uri(uriString);
		}
	}
}
