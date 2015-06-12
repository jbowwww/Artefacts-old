using System;
using MongoDB.Bson;
using System.Runtime.Serialization;

namespace Artefacts
{
	[DataContract]
	public class GetArtefactById
	{
		[DataMember]
		public ObjectId Id { get; set; }
		
		public GetArtefactById(ObjectId id)
		{
			Id = id;
		}
		
		public GetArtefactById(string id)
		{
			Id = new ObjectId(id);
		}
		
		public GetArtefactById()
		{
		
		}
		
		public static implicit operator ObjectId(GetArtefactById request)
		{
			return request.Id;
		}
	}
}

