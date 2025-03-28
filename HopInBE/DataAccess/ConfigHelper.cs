using System.Reflection;

namespace HopInBE.DataAccess
{
    public class ConfigHelper
    {
        /// <summary>
        /// Gets and sets class object.
        /// </summary>
        private static ConfigHelper _appSettings;

        /// <summary>
        /// gets and sets Appsetting Value
        /// </summary>
        public string appSettingValue { get; set; }

        /// <summary>
        /// static method which accept key and section name and return value from Appsetting
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Section"></param>
        /// <returns></returns>
        public static string AppSetting(string Key, string Section)
        {
            _appSettings = GetCurrentSettings(Key, Section);
            return _appSettings.appSettingValue;
        }

        /// <summary>
        /// this method is contructore used for buliding initial objects.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="Key"></param>
        public ConfigHelper(IConfiguration config, string Key)
        {
            this.appSettingValue = config.GetValue<string>(Key);
        }

        /// <summary>
        /// this method will ready Appsetting file based on environment variable
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Section"></param>
        /// <returns></returns>
        public static ConfigHelper GetCurrentSettings(string Key, string Section)
        {
            try
            {
                ///This is used read appsetting file based on envrionment variable
                var builder = new ConfigurationBuilder()
                                 .SetBasePath(Directory.GetCurrentDirectory())
                                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                 //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false)
                 .AddEnvironmentVariables();
                IConfigurationRoot configuration = builder.Build();
                var settings = new ConfigHelper(configuration.GetSection(Section), Key);
                return settings;
            }
            catch (Exception ex)
            {
                ///This is used read appsetting file based on envrionment variable
                var builder = new ConfigurationBuilder()
                               .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                               //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false)
                               .AddEnvironmentVariables();

                IConfigurationRoot configuration = builder.Build();

                var settings = new ConfigHelper(configuration.GetSection(Section), Key);

                return settings;
            }
        }

        #region Appsetting for Async call

        /// <summary>
        /// This method is used for extract key from appsetting file.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Section"></param>
        /// <returns></returns>
        public static T GetAppSetting<T>(string key, string section)
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();
            return configuration.GetSection(section).GetValue<T>(key);
        }


        /// <summary>
        /// This used to read value from appsetting file.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Section"></param>
        /// <returns></returns>
        public static string GetSettingValue(string Key, string Section)
        {
            var basePath = Directory.GetCurrentDirectory();
            if (File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "appsettings.json")))
            {
                basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                                //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false)
                                .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();
            return configuration.GetSection(Section).GetValue<string>(Key);
        }

        #endregion

        /// <summary>
        /// To get setting from smtp form applicton.json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <returns></returns>
        public static T BindSection<T>(string section) where T : new()
        {

            var location = Assembly.GetEntryAssembly().Location;
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                                // .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false)
                                .AddEnvironmentVariables();
            IConfigurationRoot configuration = builder.Build();
            object result = new T();
            configuration.GetSection(section).Bind(result);
            return (T)result;
        }
    }

}
