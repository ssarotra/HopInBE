using HopInBE.DataAccess.IDataProvider;
using HopInBE.DataAccess.MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace HopInBE.Database_Model
{
    [BsonCollection("Driver")]
    [BsonIgnoreExtraElements]
    public class Driver : Document
    {
        public string driverName { get; set; }
        public string mobileNumber { get; set; }
        public string ridePin { get; set; }
        public string otp { get; set; }
        public string vehicleType { get; set; }
        public string vehicleNumber { get; set; }
        public string upiId { get; set; }
        public bool IsAvailable { get; set; } 
        public bool IsOnline { get; set; } 
        [BsonElement("Location")]
        public GeoJsonPoint<GeoJson2DCoordinates> Location { get; set; }
        public string AssignedRideId { get; set; }

    }
}
