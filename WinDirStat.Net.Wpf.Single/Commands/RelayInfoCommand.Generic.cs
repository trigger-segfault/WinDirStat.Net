using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using WinDirStat.Net.ViewModel;
using WinDirStat.Net.ViewModel.Commands;
using WinDirStat.Net.Wpf.Input;

namespace WinDirStat.Net.Wpf.Commands {
	public class RelayInfoCommand<T> : RelayCommand<T>, IRelayInfoCommand, IRelayCommand<T> {

		#region Fields

		/// <summary>The UI specific info for the command.</summary>
		private RelayInfo info;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="RelayInfoCommand"/>.</summary>
		/// 
		/// <param name="execute">
		/// The execution logic. IMPORTANT: If the action causes a closure, you must set <paramref name=
		/// "keepTargetAlive"/> to true to avoid side effects.
		/// </param>
		/// <param name="keepTargetAlive">
		/// If true, the target of the Action will be kept as a hard reference, which might cause a memory
		/// leak. You should only set this parameter to true if the action is causing a closures.<para/>
		/// See http://galasoft.ch/s/mvvmweakaction.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException"><paramref name="execute"/> is null.</exception>
		public RelayInfoCommand(Action<T> execute, bool keepTargetAlive = false)
			: base(execute, null, keepTargetAlive)
		{
		}

		/// <summary>Constructs the <see cref="RelayInfoCommand"/>.</summary>
		/// 
		/// <param name="execute">
		/// The execution logic. IMPORTANT: If the action causes a closure, you must set <paramref name=
		/// "keepTargetAlive"/> to true to avoid side effects.
		/// </param>
		/// <param name="canExecute">
		/// The execution status logic. IMPORTANT: If the func causes a closure, you must set <paramref name=
		/// "keepTargetAlive"/> to true to avoid side effects.
		/// </param>
		/// <param name="keepTargetAlive">
		/// If true, the target of the Action will be kept as a hard reference, which might cause a memory
		/// leak. You should only set this parameter to true if the action is causing a closures.<para/>
		/// See http://galasoft.ch/s/mvvmweakaction.
		/// </param>
		/// 
		/// <exception cref="ArgumentNullException"><paramref name="execute"/> is null.</exception>
		public RelayInfoCommand(Action<T> execute, Func<T, bool> canExecute, bool keepTargetAlive = false)
			: base(execute, canExecute, keepTargetAlive)
		{
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets the UI specific info for the command.</summary>
		public RelayInfo Info {
			get => info;
			set {
				if (info != value) {
					if (info != null)
						info.PropertyChanged -= OnInfoPropertyChanged;
					info = value;
					if (info != null)
						info.PropertyChanged += OnInfoPropertyChanged;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>Gets the display text for the command.</summary>
		public string Text => info?.Text;
		/// <summary>Gets the display icon for the command.</summary>
		public ImageSource Icon => info?.Icon;
		/// <summary>Gets the input gesture for the command.</summary>
		public AnyKeyGesture InputGesture => info?.InputGesture;

		#endregion

		#region Events

		/// <summary>Called when a property has changed.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Execute

		/// <summary>Executes the method with a parameter.</summary>
		public void Execute(T parameter) => Execute((object) parameter);

		#endregion

		#region Private PropertyChanged

		private void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private bool Set<T2>(ref T2 field, T2 newValue, [CallerMemberName] string propertyName = null) {
			if (EqualityComparer<T2>.Default.Equals(field, newValue)) {
				return false;
			}

			field = newValue;
			RaisePropertyChanged(propertyName);
			return true;
		}

		#endregion

		#region Event Handlers
		
		private void OnInfoPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			case nameof(RelayInfo.Text):
			case nameof(RelayInfo.Icon):
			case nameof(RelayInfo.InputGesture):
				RaisePropertyChanged(e.PropertyName);
				break;
			}
		}

		#endregion
	}
}