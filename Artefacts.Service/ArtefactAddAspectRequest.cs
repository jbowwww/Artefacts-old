using System;
using ServiceStack;
using System.Collections.Generic;

namespace Artefacts
{
	[Serializable]
	public class FileDescription : IAspectUri
	{
		public string Path;
		public Uri URI { get { return new Uri(Path); } }
		
		public override string ToString()
		{
			return this.FormatString();
		}
	}

	[Serializable]
	public class ArtefactAddAspectRequest : IReturn<ArtefactAddAspectResponse>
	{
		private static IDictionary<Type, Func<object, string>> UriExtractors;

		public static void Register<T>(Func<object, string> uriExtractor)
		{
			if (UriExtractors == null)
				UriExtractors = new Dictionary<Type, Func<object, string>>();
			UriExtractors.Add(typeof(T), uriExtractor);
		}

		/// <summary>
		/// Specify artefact by integer identifier. Must use this OR <see cref="URI"/> 
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// Specify artefact by URI. Must use this OR <see cref="Id"/>
		/// </summary>
		public Uri URI { get; private set; }

		/// <summary>
		/// The aspect to be added to the artefact
		/// </summary>
		public object Aspect { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtefactService.ArtefactAddAspectRequest"/> class,
		/// specifying artefact using URI
		/// </summary>
		/// <param name="id">Id of artefact aspect is being added to</param>
		/// <param name="aspect">Aspect to add to artefact</param>
		public ArtefactAddAspectRequest(object aspect)
		{
			Id = -1;
			if (aspect is IAspectUri)
				URI = (aspect as IAspectUri).URI;
			else
			{
				for (Type T = aspect.GetType(); T != null; T = T.BaseType)
					if (UriExtractors.ContainsKey(T))
				{
						URI = new Uri(UriExtractors[T].Invoke(aspect));
						break;
				}

			}
			if (URI == default(Uri))
				throw new ArgumentOutOfRangeException("aspect", aspect, "Does not implement IAspectUri or have a register UriExtractor");
			Aspect = aspect;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtefactService.ArtefactAddAspectRequest"/> class,
		/// specifying artefact using URI
		/// </summary>
		/// <param name="uri">URI of artefact aspect is being added to</param>
		/// <param name="aspect">Aspect to add to artefact</param>
		public ArtefactAddAspectRequest(Uri uri, object aspect)
		{
			Id = -1;
			URI = uri;
			Aspect = aspect;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtefactService.ArtefactAddAspectRequest"/> class,
		/// specifying artefact using URI
		/// </summary>
		/// <param name="id">Id of artefact aspect is being added to</param>
		/// <param name="aspect">Aspect to add to artefact</param>
		public ArtefactAddAspectRequest(int id, object aspect)
		{
			Id = id;
			URI = new Uri(string.Empty);
			Aspect = aspect;
		}
		
		public override string ToString()
		{
			return this.FormatString();
		}
	}
}

