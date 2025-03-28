namespace HopInBE.RequestModel
{
    public class DriverLoginLogout
    {
        public class DriverLoginRequest
        {
            public string MobileNumber { get; set; }
            public string Otp { get; set; }
        }

        public class DriverLogoutRequest
        {
            public string DriverId { get; set; }
        }
    }
}
