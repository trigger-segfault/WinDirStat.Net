/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:WinDirStat.Net.Wpf.Single"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using WinDirStat.Net.Rendering;
using WinDirStat.Net.Services;
using WinDirStat.Net.ViewModel;
using WinDirStat.Net.Wpf.Services;

namespace WinDirStat.Net.Wpf.ViewModel {
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator {

		//public SimpleIoc Ioc { get; private set; }

		/// <summary>Initializes the ViewModelLocator class.</summary>
		static ViewModelLocator() {
			/*ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

			////if (ViewModelBase.IsInDesignModeStatic)
			////{
			////    // Create design time view services and models
			////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
			////}
			////else
			////{
			////    // Create run time view services and models
			////    SimpleIoc.Default.Register<IDataService, DataService>();
			////}

			SimpleIoc.Default.Register<SettingsService>();
			SimpleIoc.Default.Register<ScanningService>();
			SimpleIoc.Default.Register<UIService>();
			SimpleIoc.Default.Register<BitmapFactory>();
			SimpleIoc.Default.Register<IconCacheService>();
			SimpleIoc.Default.Register<ClipboardService>();
			SimpleIoc.Default.Register<OSService>();
			SimpleIoc.Default.Register<IMyDialogService, DialogService>();
			SimpleIoc.Default.Register<ImagesServiceBase, ResourceImagesService>();
			SimpleIoc.Default.Register<TreemapRendererFactory>();
			SimpleIoc.Default.Register<RelayCommandFactory, RelayInfoCommandFactory>();

			SimpleIoc.Default.Register<MainViewModel>();
			SimpleIoc.Default.Register<DriveSelectViewModel>();
			SimpleIoc.Default.Register<ConfigureViewModel>();*/
		}

		/// <summary>Initializes a new instance of the ViewModelLocator class.</summary>
		public ViewModelLocator() {
            RegisterServices();
		}

        private void RegisterServices() {
            var collection = new ServiceCollection()
                .AddSingleton<SettingsService>()
                .AddSingleton<ScanningService>()
                .AddSingleton<UIService>()
                .AddSingleton<BitmapFactory>()
                .AddSingleton<IconCacheService>()
                .AddSingleton<ClipboardService>()
                .AddSingleton<OSService>()
                .AddSingleton<IMyDialogService, DialogService>()
                .AddSingleton<ImagesServiceBase, ResourceImagesService>()
                .AddSingleton<TreemapRendererFactory>()
                .AddSingleton<RelayCommandFactory, RelayInfoCommandFactory>()

                .AddSingleton<MainViewModel>()
                .AddSingleton<DriveSelectViewModel>()
                .AddSingleton<ConfigureViewModel>()

                .BuildServiceProvider();

            Ioc.Default.ConfigureServices(collection);
        }

        public MainViewModel Main => Ioc.Default.GetService<MainViewModel>();
		public DriveSelectViewModel DriveSelect => Ioc.Default.GetService<DriveSelectViewModel>();
		public ConfigureViewModel Configure => Ioc.Default.GetService<ConfigureViewModel>();
		/*public TreemapRendererFactory TreemapFactory => ServiceLocator.Current.GetInstance<TreemapRendererFactory>();

		public TreemapRenderer CreateTreemap() {
			return TreemapFactory.Create();
		}*/
	}
}