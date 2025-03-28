using HopInBE.DataAccess.IDataProvider;
using HopInBE.DataAccess.MongoDB;
using MongoDB.Bson.Serialization.Attributes;

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
    }
}
