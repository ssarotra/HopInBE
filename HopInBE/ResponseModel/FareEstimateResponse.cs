namespace HopInBE.ResponseModel
{
    public class FareEstimateResponse
    {
        public double EstimatedFare { get; set; }
        public double Distance { get; set; }
        public string Currency { get; set; } = "INR";

        public FareEstimateResponse(double estimatedFare, double distance)
        {
            EstimatedFare = estimatedFare;
            Distance = distance;
        }
    }
}
