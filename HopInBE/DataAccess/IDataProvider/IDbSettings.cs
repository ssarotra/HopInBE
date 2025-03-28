namespace HopInBE.DataAccess.IDataProvider
{
    public interface IDbSettings
    {
        /// <summary>
        /// Database name
        /// </summary>
        string DatabaseName { get; set; }

        /// <summary>
        /// Database connection string
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Gets and sets database type like MongoDB or SQL.
        /// </summary>
        string DatabaseType { get; set; }

        /// <summary>
        /// Email address of the user accessing the database. 
        /// </summary>
        string DatabaseAccessedBy { get; set; }
    }

}
