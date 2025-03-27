namespace HopInBE.RequestModel
{
    public class OTP
    {
        public class OtpRequest
        {
            public string MobileNumber { get; set; }
            public string Role { get; set; } // "User" or "Driver"

        }

        public class OtpVerificationRequest
        {
            public string MobileNumber { get; set; }
            public string Otp { get; set; }
            public string Role { get; set; } // "User" or "Driver"

        }
    }
}
