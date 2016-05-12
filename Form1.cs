using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SASTokenGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtEpochTime.Text = EpochGenerator.GetEpochTime(3).ToString();
           
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtKey.Text)
                || String.IsNullOrEmpty(txtPolicyName.Text)
                || String.IsNullOrEmpty(txtURI.Text))
                return;
            string uri = String.Format("{0}.azure-devices.net", txtURI.Text);
            string key = txtKey.Text;
            string keyName = txtPolicyName.Text;
            string expiry = txtEpochTime.Text;
            txtResult.Text = SASTokenGenerator.GetSASToken(uri, expiry, key, keyName);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var uri = "https://uneidelIOT.azure-devices.net/devices?top=1000&api-version=2016-02-03";
            var uri2 = uri.Substring(0, uri.IndexOf(".net/") + 4);
            var expiry =  EpochGenerator.GetEpochTime(1).ToString();
            var key = "xb/V9xU916krRjPxukmZCRSRoIx+nAuNl9Cw8Y3dWP0=";
            var policyName = "iothubowner";
            var SasToken = txtResult.Text;
            DownloadIdentitiesFromRest(uri, SasToken);
        }
        private string DownloadIdentitiesFromRest(string uri, string sasToken)
        {
            var jsonresult = "";
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", sasToken);
               
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        jsonresult = reader.ReadToEnd();
                        toolStripStatusLabel1.Text = "Success";
                    }
                }
                
            }
            catch(WebException ex)
            {
                toolStripStatusLabel1.Text = "Failure";
            }
            return jsonresult; 
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            string uri = String.Format("{0}.azure-devices.net", txtURI.Text);
            var connstring = SASTokenGenerator.ConnectionString(uri, txtKey.Text, txtPolicyName.Text);
            var storageSAS = SASTokenGenerator.ContainerSaSUri(txt_StorageName.Text,"adf", txt_StorageKey.Text);
            Devices d = new Devices();
            bool result = await d.ExportAllDevices(connstring, storageSAS);
            if (result)
            {
                toolStripStatusLabel1.Text = "Successfully exported";
            }
            else { toolStripStatusLabel1.Text = "Failure while exporting"; }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            string uri = String.Format("{0}.azure-devices.net", txtURI.Text);
            var connstring = SASTokenGenerator.ConnectionString(uri, txtKey.Text, txtPolicyName.Text);
            var storageSAS = SASTokenGenerator.ContainerSaSUri(txt_StorageName.Text, "adf", txt_StorageKey.Text);
            Devices d = new Devices();
            bool result = await d.ImportAllDevices(connstring, storageSAS);
            if (result)
            {
                toolStripStatusLabel1.Text = "Successfully exported";
            }
            else { toolStripStatusLabel1.Text = "Failure while exporting"; }
        }
    }
}
