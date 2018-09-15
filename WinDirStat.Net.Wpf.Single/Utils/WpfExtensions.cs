using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Reflection;
using System.IO.Packaging;
using System.Windows.Markup;
using System.IO;
using System.ComponentModel;

namespace WinDirStat.Net.Wpf.Utils {
	public static class WpfExtensions {
		/*/// <summary>Fix the VS designer being a steaming pile of garbage.</summary>
		/// 
		/// <param name="userControl"></param>
		/// <param name="baseUri"></param>
		/// 
		/// <remarks>
		/// <a href="https://stackoverflow.com/questions/7646331/the-component-does-not-have-a-resource-identified-by-the-uri">Source</a>
		/// </remarks>
		public static void LoadViewFromUri(this UserControl userControl, string baseUri) {
			Uri resourceLocater = new Uri(baseUri, UriKind.Relative);
			PackagePart exprCa = (PackagePart) typeof(Application).GetMethod("GetResourceOrContentPart", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { resourceLocater });
			Stream stream = exprCa.GetStream();
			Uri uri = new Uri((Uri) typeof(BaseUriHelper).GetProperty("PackAppBaseUri", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null), resourceLocater);
			ParserContext parserContext = new ParserContext {
				BaseUri = uri
			};
			typeof(XamlReader).GetMethod("LoadBaml", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { stream, parserContext, userControl, true });
		}*/

		/// <summary>Fix the VS designer being a steaming pile of garbage.</summary>
		/// 
		/// <param name="userControl"></param>
		/// <param name="baseUri"></param>
		/// <returns>True if the design uri was loaded</returns>
		/// 
		/// <remarks>
		/// Improved From:<para/>
		/// <a href="https://stackoverflow.com/questions/7646331/the-component-does-not-have-a-resource-identified-by-the-uri">Source</a>
		/// </remarks>
		public static bool DesignerInitializeComponent(this UserControl userControl, string path) {
			if (DesignerProperties.GetIsInDesignMode(userControl)) {
				Type type = userControl.GetType();
				Assembly assembly = type.Assembly;
				string baseUri = $"/{assembly.GetName().Name};component/{path}/{type.Name}.xaml";

				Uri resourceLocater = new Uri(baseUri, UriKind.Relative);
				PackagePart exprCa = (PackagePart) typeof(Application).GetMethod("GetResourceOrContentPart", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { resourceLocater });
				Stream stream = exprCa.GetStream();
				Uri uri = new Uri((Uri) typeof(BaseUriHelper).GetProperty("PackAppBaseUri", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null), resourceLocater);
				ParserContext parserContext = new ParserContext {
					BaseUri = uri,
				};
				typeof(XamlReader).GetMethod("LoadBaml", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { stream, parserContext, userControl, true });
				return true;
			}
			return false;
		}
	}
}
