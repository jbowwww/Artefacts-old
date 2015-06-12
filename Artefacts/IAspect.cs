using System;
using MongoDB.Bson;
using System.Runtime.Serialization;

namespace Artefacts
{
	/// <summary>
	/// I aspect.
	/// </summary>
	public interface IAspect
	{
		/// <summary>
		/// Gets or sets the artefact identifier.
		/// </summary>
		ObjectId ArtefactId { get; }

		/// <summary>
		/// Gets or sets the artefact.
		/// </summary>
		Artefact Artefact { get; set; }
	}
}

