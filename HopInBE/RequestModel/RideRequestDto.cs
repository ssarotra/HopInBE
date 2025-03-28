namespace HopInBE.RequestModel
{
    public class RideRequestDto
    {
        public string UserId { get; set; }
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public double DropLatitude { get; set; }
        public double DropLongitude { get; set; }
        public double DistanceInKm { get; set; } // Required for Fare Calculation
    }
}
