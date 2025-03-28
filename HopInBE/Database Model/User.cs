using HopInBE.DataAccess.IDataProvider;
using HopInBE.DataAccess.MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.InteropServices;

namespace HopInBE.Database_Model
{
    [BsonCollection("User")]
    [BsonIgnoreExtraElements]
    public class User : Document
    {
        public string userName { get; set; }
        public string mobileNumber { get; set; }
        public string ridePin { get; set; }
        public string otp { get; set; }
    }

}
