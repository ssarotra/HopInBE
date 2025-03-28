using HopInBE.DataAccess.IDataProvider;

namespace HopInBE.DataAccess.DBCommon
{
    public class Factory<T> where T : IDocument
    {
        /// <summary>
        /// This method is used to create instance of MongoBD based on IBaseRepository.
        /// </summary>
        /// <param name="dbSettings"></param>
        /// <returns></returns>
        public static IBaseRepository<T> getInstance(IDbSettings dbSettings)
        {
            IBaseRepository<T> data;
            switch (dbSettings.DatabaseType?.ToUpper())
            {
                case "MONGODB":
                    data = new MongoDB.MongoDbBaseRepository<T>(dbSettings);
                    return data;
                default:
                    data = new MongoDB.MongoDbBaseRepository<T>(dbSettings);
                    return data;

            }
        }

        /// <summary>
        /// This method is used to create instance of MongoBD based on IBaseRepository.
        /// </summary>
        /// <param name="dbSettings"></param>
        /// <returns></returns>
        public static IBaseRepository<T> getInstance(IDbSettings dbSettings, bool useSingleton)
        {
            IBaseRepository<T> data;
            switch (dbSettings.DatabaseType?.ToUpper())
            {
                case "MONGODB":
                    data = new MongoDB.MongoDbBaseRepository<T>(dbSettings, useSingleton);
                    return data;
                default:
                    data = new MongoDB.MongoDbBaseRepository<T>(dbSettings, useSingleton);
                    return data;

            }
        }
    }

}
