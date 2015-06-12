using System;

namespace Artefacts
{
	public static class TypeExtensions
	{
		public static bool IsPredicate(this Type type)
		{
			return typeof(Delegate).IsAssignableFrom(type);
		}
		
		public static bool IsDelegate(this Type type)
		{
			return typeof(Delegate).IsAssignableFrom(type);
		}
	}
}

