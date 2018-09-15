using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace WinDirStat.Net {
	/// <summary>An observable object with extra raise property changed methods.</summary>
	public class ObservableObjectEx : ObservableObject {

		/// <summary>Raises the property as changed if the condition is true.</summary>
		/// 
		/// <param name="condition">The condition for raising the changed event.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <returns>True if the property was changed.</returns>
		protected bool RaisePropertyChangedIf(bool condition, [CallerMemberName] string propertyName = null) {
			RaisePropertyChanged(propertyName);
			return condition;
		}
	}
}
