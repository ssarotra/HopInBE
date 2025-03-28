namespace HopInBE.ResponseModel
{
    public class RideRequestResponse
    {
        public string RideId { get; set; }
        public string AssignedDriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverPhone { get; set; }
        public string VehicleDetails { get; set; }
        public decimal EstimatedFare { get; set; }
    }

}
