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
            if (RunningOnAzure)
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

        private static bool? _runningOnAzure;
        public static bool RunningOnAzure
        {
            get
            {
                try
                {
                    if (_runningOnAzure.HasValue)
                        return _runningOnAzure.Value;
                    _runningOnAzure = RoleEnvironment.IsAvailable;
                }
                catch
                {
                    _runningOnAzure = false;
                }
                return _runningOnAzure.Value;
            }
        }

        private static string _instanceId;

        public static string InstanceId
        {
            get
            {
                if (RunningOnAzure)
                {
                    return RoleEnvironment.CurrentRoleInstance.Id;
                }
                else
                {
                    return Environment.MachineName;
                }
                return _instanceId;
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