using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace PnP.Core.Test
{
    [TestClass]
    public class TestSetup
    {

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // Executes once before this test run
            LaunchSettingsFixture();
        }

        /// <summary>
        /// Copy the environment variables defined in the launchsettings.json file. 
        /// This approach was taken over from https://stackoverflow.com/a/43951218
        /// </summary>
        public static void LaunchSettingsFixture()
        {
            string launchSettingsFile = "..\\..\\..\\Properties\\launchSettings.json";
            if (File.Exists(launchSettingsFile))
            {
                using (var file = File.OpenText(launchSettingsFile))
                {
                    var reader = new JsonTextReader(file);
                    var jObject = JObject.Load(reader);

                    var variables = jObject
                        .GetValue("profiles")
                        //select a proper profile here
                        .SelectMany(profiles => profiles.Children())
                        .SelectMany(profile => profile.Children<JProperty>())
                        .Where(prop => prop.Name == "environmentVariables")
                        .SelectMany(prop => prop.Value.Children<JProperty>())
                        .ToList();

                    foreach (var variable in variables)
                    {
                        Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                    }
                }
            }
        }
    }
}
