using HopInBE.DataAccess.IDataProvider;
using HopInBE.DataAccess.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace HopInBE.Database_Model
{
    [BsonCollection("OTPStorage")]
    [BsonIgnoreExtraElements]
    public class OTPStorage : Document
    {
        public string MobileNumber { get; set; }
        public string OTP { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
