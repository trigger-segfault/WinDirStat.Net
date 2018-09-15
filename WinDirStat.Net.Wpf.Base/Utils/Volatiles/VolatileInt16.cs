
namespace WinDirStat.Net.Utils {
	/// <summary>A mutable volatile <see cref="short"/> that supports compound assignments.</summary>
	public class VolatileInt16 : Volatile<short> {
		
		#region Constructors

		/// <summary>Constructs the default <see cref="VolatileInt16"/>.</summary>
		public VolatileInt16() { }
		/// <summary>Constructs the <see cref="VolatileInt16"/> with the specified value.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		public VolatileInt16(short value) : base(value) { }
		/// <summary>Constructs the <see cref="VolatileInt16"/> with the specified lock.</summary>
		/// 
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileInt16(object lockObj) : base(lockObj) { }
		/// <summary>Constructs the <see cref="VolatileInt16"/> with the specified value and lock.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileInt16(short value, object lockObj) : base(value, lockObj) { }

		#endregion
		
		#region Volatile Operators
		
		/// <summary>Adds to the value.</summary>
		public short Add(short i) {
			lock (lockObj)
				return value += i;
		}
		/// <summary>Subtracts from the value.</summary>
		public short Sub(short i) {
			lock (lockObj)
				return value -= i;
		}
		/// <summary>Multiplies the value.</summary>
		public short Mul(short i) {
			lock (lockObj)
				return value *= i;
		}
		/// <summary>Divides the value.</summary>
		public short Div(short i) {
			lock (lockObj)
				return value /= i;
		}
		/// <summary>Negates the value.</summary>
		public short Negate() {
			lock (lockObj)
				return value = (short) -value;
		}

		#endregion

		#region Volatile Bitwise Operators

		/// <summary>Bitwise AND's the value.</summary>
		public short BitwiseAnd(short i) {
			lock (lockObj)
				return value &= i;
		}
		/// <summary>Bitwise OR's the value.</summary>
		public short BitwiseOr(short i) {
			lock (lockObj)
				return value |= i;
		}
		/// <summary>Bitwise XOR's the value.</summary>
		public short BitwiseXor(short i) {
			lock (lockObj)
				return value ^= i;
		}
		/// <summary>Bitwise negates the value.</summary>
		public short BitwiseNegate() {
			lock (lockObj)
				return value = unchecked((short) ~value);
		}

		#endregion
	}
}
