using HopInBE.Database_Model;
using MongoDB.Driver;

namespace HopInBE.DAL
{
    public class AuthDal
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Driver> _drivers;
        private readonly IConfiguration _config;
        private readonly Random _random = new Random();

        public AuthDal() { }

        //public register()

    }
}
