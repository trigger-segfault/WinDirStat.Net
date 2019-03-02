
namespace WinDirStat.Net.Services.Structures {
	/// <summary>An interface for storing the control.</summary>
	public interface IControl {

		#region Properties

		/// <summary>Gets the actual window object.</summary>
		object Control { get; }
		/// <summary>Gets the window that contains this control.</summary>
		IWindow Window { get; }

		#endregion

		#region Equals

		/// <summary>Gets if the two controls are referencing the same control.</summary>
		/// 
		/// <param name="control">The control wrapper to compare.</param>
		/// <returns>True if the interfaces are referencing the same control.</returns>
		bool Equals(IControl control);

		#endregion
	}
}
