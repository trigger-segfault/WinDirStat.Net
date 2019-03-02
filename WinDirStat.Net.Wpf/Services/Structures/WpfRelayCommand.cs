using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using WinDirStat.Net.ViewModel;

namespace WinDirStat.Net.Wpf.Services.Structures {
	public class WpfRelayCommand : RelayCommand, IWpfRelayUICommand, IRelayCommand {

		#region Constructors

		/// <summary>Constructs the <see cref="WpfRelayCommand"/>.</summary>
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
		public WpfRelayCommand(Action execute, bool keepTargetAlive = false)
			: base(execute, null, keepTargetAlive)
		{
		}
		
		/// <summary>Constructs the <see cref="WpfRelayCommand"/>.</summary>
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
		public WpfRelayCommand(Action execute, Func<bool> canExecute, bool keepTargetAlive = false)
			: base(execute, canExecute, keepTargetAlive)
		{
		}

		#endregion
	}
}
