
namespace WinDirStat.Net.Utils {
	/// <summary>A mutable volatile <see cref="long"/> that supports compound assignments.</summary>
	public class VolatileInt64 : Volatile<long> {
		
		#region Constructors

		/// <summary>Constructs the default <see cref="VolatileInt64"/>.</summary>
		public VolatileInt64() { }
		/// <summary>Constructs the <see cref="VolatileInt64"/> with the specified value.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		public VolatileInt64(long value) : base(value) { }
		/// <summary>Constructs the <see cref="VolatileInt64"/> with the specified lock.</summary>
		/// 
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileInt64(object lockObj) : base(lockObj) { }
		/// <summary>Constructs the <see cref="VolatileInt64"/> with the specified value and lock.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileInt64(long value, object lockObj) : base(value, lockObj) { }

		#endregion
		
		#region Volatile Operators
		
		/// <summary>Adds to the value.</summary>
		public long Add(long i) {
			lock (lockObj)
				return value += i;
		}
		/// <summary>Subtracts from the value.</summary>
		public long Sub(long i) {
			lock (lockObj)
				return value -= i;
		}
		/// <summary>Multiplies the value.</summary>
		public long Mul(long i) {
			lock (lockObj)
				return value *= i;
		}
		/// <summary>Divides the value.</summary>
		public long Div(long i) {
			lock (lockObj)
				return value /= i;
		}
		/// <summary>Negates the value.</summary>
		public long Negate() {
			lock (lockObj)
				return value = -value;
		}

		#endregion

		#region Volatile Bitwise Operators

		/// <summary>Bitwise AND's the value.</summary>
		public long BitwiseAnd(long i) {
			lock (lockObj)
				return value &= i;
		}
		/// <summary>Bitwise OR's the value.</summary>
		public long BitwiseOr(long i) {
			lock (lockObj)
				return value |= i;
		}
		/// <summary>Bitwise XOR's the value.</summary>
		public long BitwiseXor(long i) {
			lock (lockObj)
				return value ^= i;
		}
		/// <summary>Bitwise negates the value.</summary>
		public long BitwiseNegate() {
			lock (lockObj)
				return value = ~value;
		}

		#endregion
	}
}
