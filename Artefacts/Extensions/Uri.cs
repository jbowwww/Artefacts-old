using System;

namespace Artefacts
{
	public static class Uri_Ext
	{
		public implicit operator Uri(string str) { return new Uri(str); }
		
	}
}

