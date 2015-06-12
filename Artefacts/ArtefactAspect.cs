using System;

namespace Artefacts
{
	public class ArtefactAspect<T> where T : class, new()
	{
		public ArtefactAspect(T aspect)
		{
		}
	}
}

