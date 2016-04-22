using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Runtime;

using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using SASTokenGenerator;

namespace Iot
{
   

    public class IOTHubActivity : IDotNetActivity
    {
        private IActivityLogger _logger;
        private string uri;
        private string expiry;
        private string key;
        private string policyName;
        private string dataStorageContainer, dataStorageAccountName, dataStorageAccountKey;


        public IDictionary<string, string> Execute(
            IEnumerable<LinkedService> linkedServices,
            IEnumerable<Dataset> datasets,
            Activity activity,
            IActivityLogger logger)
        {

            // to get extended properties (for example: SliceStart)
            DotNetActivity dotNetActivity = (DotNetActivity)activity.TypeProperties;
            uri = dotNetActivity.ExtendedProperties["uri"];
            expiry = EpochGenerator.GetEpochTime(1).ToString();
            key = dotNetActivity.ExtendedProperties["key"];
            policyName = dotNetActivity.ExtendedProperties["policyName"];
            dataStorageAccountName = dotNetActivity.ExtendedProperties["dataStorageAccountName"];
            dataStorageContainer = dotNetActivity.ExtendedProperties["dataStorageContainer"];
            dataStorageAccountKey = dotNetActivity.ExtendedProperties["dataStorageAccountKey"];
            _logger = logger;
            GatherDataFromIotHub();

            _logger.Write("Exit");
            return new Dictionary<string, string>();
        }

       
        /// <summary>
        /// Gather data for each Hour based on Slice Start Time.
        /// </summary>
        /// <param name="sliceStartTime"></param>
        /// <param name="urlFormat"></param>
        private void GatherDataFromIotHub()
        {
            Uri storageAccountUri = new Uri("http://" + dataStorageAccountName + ".blob.core.windows.net/");
            
            // Temporary staging folder
            string dataStagingFolder = string.Format(@"{0}\{1}\{1}-{2}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DateTime.Now.Year,DateTime.Now.Month);
            Directory.CreateDirectory(dataStagingFolder);

            // Temporary staging file
            string hourlyFileName = string.Format("data-{0}{1}{2}-{3}0000.json", DateTime.Now.Year, 
                DateTime.Now.Month, 
                DateTime.Now.Day,
                DateTime.Now.Hour);
            string jsonFile = Path.Combine(dataStagingFolder, hourlyFileName);

            try
            {
                _logger.Write("Gathering hourly data: ..");
                TriggerRequest(uri, SASTokenGenerator.SASTokenGenerator.GetSASToken(uri, expiry, key, policyName),jsonFile);
                _logger.Write("Uploading to Blob: ..");
                CloudBlobClient blobClient = new CloudBlobClient(storageAccountUri, 
                    new StorageCredentials(dataStorageAccountName, dataStorageAccountKey));

                string blobPath = string.Format(CultureInfo.InvariantCulture, "iothubdevices.json");

                CloudBlobContainer container = blobClient.GetContainerReference(dataStorageContainer);
                container.CreateIfNotExists();

                var blob = container.GetBlockBlobReference(blobPath);
                blob.UploadFromFile(jsonFile, FileMode.OpenOrCreate);
            }
            catch (Exception ex)
            {
                _logger.Write("Error occurred : {0}", ex);
                throw;
            }
            finally
            {
                if (File.Exists(jsonFile))
                {
                    File.Delete(jsonFile);
                }
            }
        }

        /// <summary>
        /// Trigger request to the HTTP Endpoint
        /// </summary>
        /// <param name="urlFormat"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="decompressedFile"></param>
        private void TriggerRequest(string uri, string sasToken,string decompressedFile)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", sasToken);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    
                }
            }

        }


        
    }
}
