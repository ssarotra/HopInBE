using HopInBE.DAL;
using HopInBE.Database_Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static HopInBE.RequestModel.OTP;
using static HopInBE.RequestModel.Register;

namespace HopInBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        AuthDal _authDal;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Driver> _drivers;
        private readonly IConfiguration _config;
        private readonly Random _random = new Random();

        public AuthController(IConfiguration config, IMongoClient client)
        {
            _config = config;
            var database = client.GetDatabase("RideBookingDB");
            _users = database.GetCollection<User>("Users");
            _drivers = database.GetCollection<Driver>("Drivers");
            _authDal = new AuthDal();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            if (request.Role == "User")
            {
                var existingUser = await _users.Find(u => u.mobileNumber == request.MobileNumber).FirstOrDefaultAsync();
                if (existingUser != null) return BadRequest("User already registered");

                var newUser = new User
                {
                    userName = request.Name,
                    mobileNumber = request.MobileNumber,
                    ridePin = request.RidePin,
                    otp = string.Empty
                };
                await _users.InsertOneAsync(newUser);
                return Ok("User registered successfully");
            }
            else if (request.Role == "Driver")
            {
                var existingDriver = await _drivers.Find(d => d.mobileNumber == request.MobileNumber).FirstOrDefaultAsync();
                if (existingDriver != null) return BadRequest("Driver already registered");

                var newDriver = new Driver
                {
                    driverName = request.Name,
                    mobileNumber = request.MobileNumber,
                    ridePin = request.RidePin,
                    otp = string.Empty,
                    vehicleType = request.VehicleType,
                    vehicleNumber = request.VehicleNumber,
                    upiId = request.UpiId
                };
                await _drivers.InsertOneAsync(newDriver);
                return Ok("Driver registered successfully");
            }
            return BadRequest("Invalid Role");
        }

        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] OtpRequest request)
        {
            var otp = _random.Next(100000, 999999).ToString();
            if (request.Role == "User")
            {
                var user = await _users.Find(u => u.mobileNumber == request.MobileNumber).FirstOrDefaultAsync();
                if (user == null)
                {
                    user = new User { mobileNumber = request.MobileNumber, otp = otp };
                    await _users.InsertOneAsync(user);
                }
                else
                {
                    var update = Builders<User>.Update.Set(u => u.otp, otp);
                    await _users.UpdateOneAsync(u => u.mobileNumber == request.MobileNumber, update);
                }
            }
            else if (request.Role == "Driver")
            {
                var driver = await _drivers.Find(d => d.mobileNumber == request.MobileNumber).FirstOrDefaultAsync();
                if (driver == null)
                {
                    driver = new Driver { mobileNumber = request.MobileNumber, otp = otp };
                    await _drivers.InsertOneAsync(driver);
                }
                else
                {
                    var update = Builders<Driver>.Update.Set(d => d.otp, otp);
                    await _drivers.UpdateOneAsync(d => d.mobileNumber == request.MobileNumber, update);
                }
            }
            Console.WriteLine($"OTP for {request.MobileNumber}: {otp}");
            return Ok("OTP sent successfully");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            if (request.Role == "User")
            {
                var user = await _users.Find(u => u.mobileNumber == request.MobileNumber && u.otp == request.Otp).FirstOrDefaultAsync();
                if (user == null) return Unauthorized("Invalid OTP");
                var token = GenerateJwtToken(user.mobileNumber, "User");
                return Ok(new { Token = token });
            }
            else if (request.Role == "Driver")
            {
                var driver = await _drivers.Find(d => d.mobileNumber == request.MobileNumber && d.otp == request.Otp).FirstOrDefaultAsync();
                if (driver == null) return Unauthorized("Invalid OTP");
                var token = GenerateJwtToken(driver.mobileNumber, "Driver");
                return Ok(new { Token = token });
            }
            return BadRequest("Invalid Role");
        }

        private string GenerateJwtToken(string mobileNumber, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]??""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("MobileNumber", mobileNumber),
                new Claim("Role", role)
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
