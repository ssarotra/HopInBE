using HopInBE.MongoDb;
using MongoDB.Bson.Serialization.Attributes;

namespace HopInBE.Database_Model
{
    [BsonCollection("User")]
    [BsonIgnoreExtraElements]
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string userName { get; set; }
        public string mobileNumber { get; set; }
        public string ridePin { get; set; }
        public string otp { get; set; }
    }

}
