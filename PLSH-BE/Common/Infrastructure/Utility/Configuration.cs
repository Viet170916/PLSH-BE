using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Common.Infrastructure.Utility
{
    [ExcludeFromCodeCoverage]
    public class Configuration
    {
        private static Configuration _instance;
        private static IConfiguration _config;

        private Configuration()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(environment))
            {
                new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();
            }
            else
            {
                new ConfigurationBuilder().AddJsonFile($"appsettings.{environment}.json", false, true).Build();
            }
        }

        public static Configuration Instance => _instance ?? (_instance = new Configuration());

        public string GetConfig(string key)
        {
            return _config[key];
        }

        public string GetConfig(string section, string key)
        {
            return _config[$"{section}:{key}"];
        }

        public string GetConfig(string section, string subSection, string key)
        {
            return _config[$"{section}:{subSection}:{key}"];
        }

        public T GetConfig<T>(string key)
        {
            return (T)Convert.ChangeType(_config[key], typeof(T));
        }

        public T GetConfig<T>(string section, string key)
        {
            return (T)Convert.ChangeType(_config[$"{section}:{key}"], typeof(T));
        }

        public T GetConfig<T>(string section, string subSection, string key)
        {
            return (T)Convert.ChangeType(_config[$"{section}:{subSection}:{key}"], typeof(T));
        }

        public T GetSection<T>(string section)
        {
            return _config.GetSection(section).Get<T>();
        }

        public T GetSection<T>(string section, string subSection)
        {
            return _config.GetSection(section).GetSection(subSection).Get<T>();
        }
    }
}
