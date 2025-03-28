using HopInBE.DataAccess.IDataProvider;
using HopInBE.DataAccess.MongoDB;
using MongoDB.Bson.Serialization.Attributes;

namespace HopInBE.Database_Model
{
    [BsonCollection("RideRequest")]
    [BsonIgnoreExtraElements]
    public class RideRequest : Document
    {
        public string UserId { get; set; }
        public string DriverId { get; set; } // Initially null, updated when a driver accepts
        public string AssignedDriverId { get; set; } // The actual driver who takes the ride
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public double DropLatitude { get; set; }
        public double DropLongitude { get; set; }
        public double EstimatedFare { get; set; }
        public string Status { get; set; } // Pending, Accepted, Completed, Cancelled
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
        public string PIN { get; set; }

    }
}
