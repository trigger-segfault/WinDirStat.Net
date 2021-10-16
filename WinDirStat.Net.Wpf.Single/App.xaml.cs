using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using WinDirStat.Net.Wpf.Windows;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]

namespace WinDirStat.Net.Wpf {
    /// <summary>
    ///  Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        #region Constants

        private static readonly string[] assemblyExtensions = { ".dll", ".exe" };

        #endregion

        #region Constructors

        /// <summary>
        ///  Constructs the app and sets up embedded assembly resolving.
        /// </summary>
        public App() {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssemblies;
            // Call this to avoid referencing assemblies before the assembly resolver can be added.
            Initialize();
        }

        /// <summary>
        ///  Called to avoid referencing assemblies before the assembly resolver can be added.
        /// </summary>
        private void Initialize() {
            ErrorMessageBox.ProgramName = "WinDirStat.Net";
            ErrorMessageBox.HyperlinkName = "GitHub Page";
            ErrorMessageBox.HyperlinkUri = new Uri(@"https://github.com/trigger-death/WinDirStat.Net");
            ErrorMessageBox.ErrorIcon = new BitmapImage(new Uri("pack://application:,,,/Resources/App.ico"));
            ErrorMessageBox.GlobalHook(this);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        ///  Resolves assemblies that may be embedded in the executable.
        /// </summary>
        private Assembly OnResolveAssemblies(object sender, ResolveEventArgs args) {
            AssemblyName assemblyName = new AssemblyName(args.Name);
            string assemblyPath;

            if (TryResolveAssembly(assemblyName, out Assembly assembly))
                return assembly;
            assemblyPath = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            if (TryResolveAssembly(assemblyPath, assemblyName, out assembly))
                return assembly;
            assemblyPath = CultureInfo.CurrentCulture.ToString();
            if (TryResolveAssembly(assemblyPath, assemblyName, out assembly))
                return assembly;

            return null;
        }

        #endregion

        #region TryResolveAssembly

        private bool TryResolveAssembly(AssemblyName assemblyName, out Assembly assembly) {
            return TryResolveAssembly(null, assemblyName, out assembly);
        }
        private bool TryResolveAssembly(string path, AssemblyName assemblyName, out Assembly assembly) {
            foreach (string ext in assemblyExtensions) {
                string startPath = Path.Combine(AppContext.BaseDirectory, "bin");
                if (path != null && !Path.IsPathRooted(path))
                    startPath = Path.Combine(startPath, path);

                path = Path.Combine(startPath, assemblyName.Name + ext);
                if (File.Exists(path)) {
                    assembly = Assembly.LoadFile(path);
                    return true;
                }
            }
            assembly = null;
            return false;
        }

        #endregion
    }
}
