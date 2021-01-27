using System;

namespace Deli.Patcher.Bootstrap
{
	public class ImplicitContractException : InvalidOperationException
	{
		public ImplicitContractException(string requiredMember) : base($"Implicit contract was broken: {requiredMember} must be called before this member is accessed.")
		{
		}
	}
}
