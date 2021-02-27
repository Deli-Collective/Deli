namespace Deli.VFS
{
	internal static class HPath
	{
		public static string Combine(IDirectoryHandle parent, string child)
		{
			return parent.Path + "/" + child;
		}
	}
}
