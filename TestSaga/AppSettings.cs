using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace TestSaga
{
    public class AppSettings
    {
        public virtual TimeSpan BatchTimeout => TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["BatchTimeoutInMinutes"]));

        public virtual DateTime BatchCutOffTime => DateTime.ParseExact(ConfigurationManager.AppSettings["BatchCutOffTime"], "HHmmss", CultureInfo.InvariantCulture);

        public virtual int MaximumBatchSize => int.Parse(ConfigurationManager.AppSettings["MaximumBatchSize"]);

        public virtual DateTime BatchCutOffDateTime => new DateTime(DateTime.Today.Year,
            DateTime.Today.Month,
            DateTime.Today.Day,
            BatchCutOffTime.Hour,
            BatchCutOffTime.Minute,
            BatchCutOffTime.Second);

        public virtual string NServiceBusPersistence => ConfigurationManager.AppSettings["NServiceBusPersistence"];

        public virtual int MaxImmediateRetries => int.Parse(GetStringOrDefault("MaxImmediateRetries",null));

        public virtual int MaxDelayedRetries => int.Parse(GetStringOrDefault("MaxDelayedRetries",null));
        

        public static string LicensePath =>
            GetStringOrDefault(
                "NServiceBusLicensePath",
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "License", "License.xml"));

        private static string GetStringOrDefault(string key,
            string defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];

            return string.IsNullOrEmpty(value) ?
                defaultValue :
                value;
        }
    }
}