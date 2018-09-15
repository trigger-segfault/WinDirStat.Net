using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net {
	/// <summary>An observable object with volatile get and set.</summary>
	public class ObservableVolatileObject : ObservableObjectEx {

		/// <summary>The lock for volatile properties.</summary>
		protected readonly object volatileLock = new object();

		/// <summary>Gets the property and ensures its under lock while accessing it.</summary>
		/// 
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="field">The field to access.</param>
		/// <returns>The locked value of the field.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected T VolatileGet<T>(ref T field) {
			lock (volatileLock)
				return field;
		}

		/// <summary>Sets the property and ensures the property is not changed during the process.</summary>
		/// 
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="field">The field of the property.</param>
		/// <param name="newValue">The new value of the property.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <returns>True if the property was changed.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool VolatileSet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null) {
			lock (volatileLock) {
				if (EqualityComparer<T>.Default.Equals(field, newValue))
					return false;
				field = newValue;
			}
			RaisePropertyChanged(propertyName);
			return true;
		}

	}
}
