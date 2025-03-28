using HopInBE.DataAccess.IDataProvider;
using HopInBE.DataAccess.MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace HopInBE.Database_Model
{
    [BsonCollection("GpsHistory")]
    [BsonIgnoreExtraElements]
    public class GpsHistory : Document
    {
        public string DriverId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
