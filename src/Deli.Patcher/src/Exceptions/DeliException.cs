using System;

namespace Deli.Patcher.Exceptions
{
	/// <summary>
	///		Top level class for all Deli exceptions
	/// </summary>
	public class DeliException : Exception
	{
		/// <summary>
		///		The mod which caused this exception
		/// </summary>
		public Mod Mod { get; }

		/// <summary>
		///		Constructor for DeliException
		/// </summary>
		/// <param name="mod">The mod which caused this exception</param>
		/// <param name="message">The exception's message</param>
		/// <param name="innerException">The optional inner exception</param>
		public DeliException(Mod mod, string message, Exception? innerException = null) : base(message, innerException)
		{
			Mod = mod;
		}
	}
}
