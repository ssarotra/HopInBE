namespace HopInBE.RequestModel
{
    public class Register
    {
        public class RegistrationRequest
        {
            public string Name { get; set; }
            public string MobileNumber { get; set; }
            public string RidePin { get; set; }
            public string Role { get; set; } // "User" or "Driver"
            public string VehicleType { get; set; } // Only for Driver
            public string VehicleNumber { get; set; } // Only for Driver
            public string UpiId { get; set; } // Only for Driver
        }
    }
}
