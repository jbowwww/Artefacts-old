using System;

namespace Artefacts.Extensions
{
	public static class QueryableBase_Ext
	{
		public static int Count(this QueryableBase queryable)
		{
			return queryable.Count;
		}
	}
}

