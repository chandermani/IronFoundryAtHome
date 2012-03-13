using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Configuration;

namespace AtHomeWebRole
{
    public static class ApplicationSettings
    {
        private static AppSettingsProvider _settingsProvider;
        static ApplicationSettings()
        {
            if (RoleEnvironment.IsAvailable)
                _settingsProvider = new AzureSettingsProvider();
        }

        public static string DataConnectionString
        {
            get
            {
                return _settingsProvider.GetSetting("DataConnectionString");
            }
        }
        public static int PollingInterval
        {
            get
            {
                return int.Parse(_settingsProvider.GetSetting("PollingInterval"));
            }
        }
        public static string ClientEXE
        {
            get
            {
                return _settingsProvider.GetSetting("ClientEXE");
            }
        }
        public static string PingServer
        {
            get
            {
                return _settingsProvider.GetSetting("PingServer");
            }
        }
    }

    public abstract class AppSettingsProvider
    {
        public virtual string GetSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }

    public class AzureSettingsProvider : AppSettingsProvider
    {
        public override string GetSetting(string name)
        {
            return RoleEnvironment.GetConfigurationSettingValue(name);
        }
    }

    
}