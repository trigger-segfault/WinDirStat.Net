using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	/// <summary>A mutable volatile <see cref="T"/>.</summary>
	public class Volatile<T> : IVolatile {

		#region Fields

		/// <summary>The <see cref="T"/> value.</summary>
		protected T value;
		/// <summary>The lock context.</summary>
		protected readonly object lockObj;

		#endregion

		#region Constructors

		/// <summary>Constructs the default <see cref="Volatile{T}"/>.</summary>
		public Volatile() {
			lockObj = new object();
		}
		/// <summary>Constructs the <see cref="Volatile{T}"/> with the specified value.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		public Volatile(T value) : this() {
			this.value = value;
		}
		/// <summary>Constructs the <see cref="Volatile{T}"/> with the specified lock.</summary>
		/// 
		/// <param name="lockObj">The lock context to use.</param>
		public Volatile(object lockObj) {
			if (lockObj == null)
				throw new ArgumentNullException(nameof(lockObj));
			if (lockObj.GetType().IsValueType)
				throw new ArgumentException("Cannot use a boxed a value type as a lock context!");
			this.lockObj = lockObj;
		}
		/// <summary>Constructs the <see cref="Volatile{T}"/> with the specified value and lock.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		/// <param name="lockObj">The lock context to use.</param>
		public Volatile(T value, object lockObj) : this(lockObj) {
			this.value = value;
		}

		#endregion

		#region Locking

		/// <summary>Locks the value.</summary>
		public void Lock() {
			Monitor.Enter(lockObj);
		}
		/// <summary>Unlocks the value.</summary>
		public void Unlock() {
			Monitor.Exit(lockObj);
		}

		#endregion

		#region Properties

		/// <summary>Gets the <see cref="T"/> value.</summary>
		public T Value {
			get { lock (lockObj) return value; }
			//set { lock (lockObj) this.value = value; }
		}
		/// <summary>Gets the <see cref="object"/> value.</summary>
		object IVolatile.Value => Value;
		/// <summary>Gets the lock context.</summary>
		public object LockObj => lockObj;

		#endregion
		
		#region Volatile Operators

		/// <summary>Sets the value.</summary>
		public T Set(T newValue) {
			lock (lockObj)
				return value = newValue;
		}
		/// <summary>Sets the value.</summary>
		object IVolatile.Set(object newValue) {
			lock (lockObj)
				return value = (T) newValue;
		}

		#endregion

		#region Conversion Operators

		/// <summary>Gets the <see cref="T"/> of the <see cref="Volatile{T}"/>.</summary>
		public static implicit operator T(Volatile<T> value) {
			return value.Value;
		}

		#endregion

		#region Overrides

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		public override bool Equals(object obj) {
			if (obj is IVolatile vol)
				return value.Equals(vol.Value);
			return value.Equals(obj);
		}
		/// <summary>Returns the hash code for this instance.</summary>
		public override int GetHashCode() => value.GetHashCode();
		/// <summary>Converts the value of this instance to its equivalent string representation.</summary>
		public override string ToString() => value.ToString();

		#endregion
	}
}
