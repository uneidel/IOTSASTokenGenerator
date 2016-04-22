using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SASTokenGenerator
{
    public class SASTokenGenerator
    {
        public static string GetSASToken(string uri, string expiry, string key, string policyName)
        {
            string uriStringEncoded = HttpUtility.UrlEncode(uri);
            string stringToSign = uriStringEncoded + "\n" + expiry;
            var signature = Sign(stringToSign, key);
            var sasToken = String.Format(CultureInfo.InvariantCulture, 
                                                "SharedAccessSignature sig={0}&se={1}&skn={2}&sr={3}", 
                                                HttpUtility.UrlEncode(signature), expiry, policyName, uri);
            return sasToken;
        }
        private static string Sign(string requestString, string key)
        {
            string result;
            using (HMACSHA256 hMACSHA = new HMACSHA256(Convert.FromBase64String(key)))
            {
                result = Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(requestString)));
            }
            return result;
        }
    }

    public class EpochGenerator
    {
        public static int GetEpochTime(int AddDays)
        {
            TimeSpan t = (DateTime.UtcNow).AddDays(AddDays) - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            return secondsSinceEpoch;
            
        }
    }
}
