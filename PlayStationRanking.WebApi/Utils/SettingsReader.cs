using System;
using Microsoft.Extensions.Configuration;

namespace WebApi.Utils
{
    public static class SettingsReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="createdItem">It is not necessary to create the item previously to method, it is only to set parameters before setting values from config (if required)</param>
        /// <param name="settings"></param>
        /// <param name="key"></param>
        /// <param name="basePath"></param>
        /// <param name="optional"></param>
        /// <param name="reloadOnChange"></param>
        /// <returns></returns>
        public static T Get<T>(T createdItem, string settings, string key, string basePath = null, bool optional = false, bool reloadOnChange = true)
        {
            IConfigurationBuilder configuration = new ConfigurationBuilder();

            if (basePath != null)
                configuration = configuration.SetBasePath(basePath);

            var configRoot = configuration.AddJsonFile(settings, optional, reloadOnChange).Build();

            var config = createdItem != null ? createdItem : Activator.CreateInstance<T>();
            configRoot.Bind(key, config);

            if (reloadOnChange)
            {
                var reload = configRoot.GetReloadToken();
                reload.RegisterChangeCallback((o) =>
                {
                    configRoot.Bind(key, config);
                }, null);
            }

            return config;
        }
    }
}
