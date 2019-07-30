using System;

namespace Salar.Bois
{
	/// <summary>
	/// Provided Type is not valid
	/// </summary>
	public class InvalidTypeException : Exception
	{
		public InvalidTypeException()
			: base()
		{ }

		public InvalidTypeException(string message)
			: base(message)
		{ }

		public InvalidTypeException(string message, Exception innserException)
			: base(message, innserException)
		{ }
	}
}
