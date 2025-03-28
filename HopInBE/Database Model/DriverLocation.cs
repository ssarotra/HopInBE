using HopInBE.DataAccess.IDataProvider;
using HopInBE.DataAccess.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HopInBE.Database_Model
{
    [BsonCollection("DriverLocation")]
    [BsonIgnoreExtraElements]
    public class DriverLocation : Document
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string DriverId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
