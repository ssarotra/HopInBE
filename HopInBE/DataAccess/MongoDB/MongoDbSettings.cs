using HopInBE.DataAccess.IDataProvider;

namespace HopInBE.DataAccess.MongoDB
{
    public class MongoDbSettings : IDbSettings
    {
        /// <summary>
        /// Database name
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Database connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// gets and sets Database Type
        /// </summary>
        public string DatabaseType { get; set; } = null;

        /// <summary>
        /// gets and sets DatabaseAccessedBy
        /// </summary>
        public string DatabaseAccessedBy { get; set; } = "";
    }

}
