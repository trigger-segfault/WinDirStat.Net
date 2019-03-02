using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;
#if WPF
using GalaSoft.MvvmLight.CommandWpf;
#else
using GalaSoft.MvvmLight.Command;
#endif

namespace WinDirStat.Net.ViewModel {
	public class RelayUICommand<T> : RelayCommand<T>, IRelayUICommand {

		#region Fields

		private string text;
		private IImage icon;
		private readonly IShortcut keyGesture;

		#endregion

		#region Constructors
		
		public RelayUICommand(string text, IImage icon, Action<T> execute)
			: this(text, icon, null, execute, null)
		{
		}

		public RelayUICommand(string text, IImage icon, Action<T> execute,
			Func<T, bool> canExecute)
			: this(text, icon, null, execute, canExecute)
		{
		}

		public RelayUICommand(string text, IImage icon, IShortcut keyGesture,
			Action<T> execute)
			: this(text, icon, keyGesture, execute, null)
		{
		}

		public RelayUICommand(string text, IImage icon, IShortcut keyGesture,
			Action<T> execute, Func<T, bool> canExecute)
			: base(execute, canExecute)
		{
			this.text = text;
			this.icon = icon;
			this.keyGesture = keyGesture;
		}

		#endregion

		#region Properties

		public string Text {
			get => text;
			set {
				if (text != value) {
					text = value;
					RaisePropertyChanged();
				}
			}
		}

		public IImage Icon {
			get => icon;
			set {
				if (icon != value) {
					icon = value;
					RaisePropertyChanged();
				}
			}
		}

		public IShortcut Shortcut {
			get => keyGesture;
		}

		#endregion

		#region Events

		/// <summary>Called when the <see cref="Text"/> or <see cref="Icon"/> property has changed.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region RaisePropertyChanged

		/// <summary>Raises the <see cref="PropertyChanged"/> event.</summary>
		protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
