using AutoMapper;
using HopInBE.DAL;
using HopInBE.DataAccess.IDataProvider;
using HopInBE.Database_Model;
using HopInBE.ResponseModel;
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

        public AuthController(IDbSettings dbSettings, IMapper mapperObj, IHttpContextAccessor httpContextAccessor)
        {
            _authDal = new AuthDal(dbSettings, mapperObj, httpContextAccessor);
        }

        [HttpPost("register")]
        public async Task<ServiceResponse<string>> Register(RegistrationRequest request)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();

            serviceResponse.Message = await _authDal.register(request);
            if (serviceResponse.Message == "User registered successfully" || serviceResponse.Message == "Driver registered successfully")
                serviceResponse.Status = System.Net.HttpStatusCode.OK;
            else if(serviceResponse.Message== "User already registered"|| serviceResponse.Message == "Driver already registered")
                serviceResponse.Status = System.Net.HttpStatusCode.BadRequest;
            else
                serviceResponse.Status = System.Net.HttpStatusCode.BadRequest;
            return serviceResponse;
        }

        [HttpPost("requestotp")]
        public async Task<ServiceResponse<string>> RequestOtp( OtpRequest request)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();

            serviceResponse.Message = await _authDal.requestotp(request);
            if (serviceResponse.Message == "OTP sent successfully")
                serviceResponse.Status = System.Net.HttpStatusCode.OK;
            else
                serviceResponse.Status = System.Net.HttpStatusCode.BadRequest;
            return serviceResponse;
        }

        [HttpPost("verifyotp")]
        public async Task<ServiceResponse<string>> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();

            serviceResponse = await _authDal.verifyotp(request);
            return serviceResponse;

        }

    }
}
