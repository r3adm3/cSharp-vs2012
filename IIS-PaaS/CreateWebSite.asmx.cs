using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.Web.Administration;
using System.Text;
using System.Net;
using System.IO;
using System.Configuration;
using System.Net.Mail;
using System.Management;

namespace IIS_PaaS
{

    

    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService
    {
        private string DNSSuffix = ConfigurationManager.AppSettings.Get("DNSsuffix");
        private string inetpubroot = ConfigurationManager.AppSettings.Get("inetpubroot");

        [WebMethod]
        public string HelloWorld()
        {   
            return DNSSuffix ;
        }

        [WebMethod]
        public int CreateWebSite(string SiteName)
        {

            //check if the array has space
            if (WebArrayHasSpace())
            {
                //ok to proceed....
            }
            else
            {
                //return an error which we'll handle on the client side. 
                return 2;
            }

            //check if the SiteName is Unique in the array
            if (IsSiteNameUnique(SiteName))
            {
                //ok to proceed....
            }
            else
            {
                //return an error which we'll handle on the client side.
                return 3;
            }

            //check if the DNSName already exists or not
            //here we're going to strip out any spaces then check if the fqdn of the resultant 
            //url (eg. defaultwebsite.nwtraders.msft) exists.
            if (CheckforDNSEntry(SiteName))
            {
                //url exists, so return an error we can handle
                return 4;
            }
            else
            {
                //dns looks good. 
            }

            //create the iis site
            if (CreateTheSite(SiteName))
            {
                //Success
            }
            else
            {
                return 5;
            }

            //create the dns name 


            //check the site is up. 
            

            //send connectivity creds to user. 

            return 0;
        }

        private bool WebArrayHasSpace()
        {
            return true;
        }



        private bool IsSiteNameUnique(string SiteName)
        {
            bool returnValue = true;
            ServerManager ServerManager = new ServerManager() ;
            
                foreach (Site mySite in ServerManager.Sites)
                {
                    if (SiteName.ToLower() == mySite.Name.ToLower())
                    {
                        //site name exists
                        returnValue = false;
                    }

                }
            
            return returnValue;
        }


        private bool CreateTheSite(string SiteName)
        {

            //does path on filesystem exist?
            if (!File.Exists(inetpubroot + "\\wwwroot-" + SiteName))
            {
                //if not, create path on the filesystem.
                try
                {
                    Directory.CreateDirectory(inetpubroot + "\\inetpub\\wwwroot-" + SiteName);
                }
                catch
                {
                    return false;
                }

            }

            //create user? - do this another day. 

            //permissions filesystem -and this.  

            try
            {
                using (ServerManager m = new ServerManager())
                {

                    //Site instance creation
                    Site mySite = m.Sites.Add(SiteName, "http", "*:80:" + SiteName + DNSSuffix , inetpubroot + "\\wwwroot-" + SiteName);
                  
                    //add an app pool using ServerManager instance
                    m.ApplicationPools.Add(SiteName + "-Pool");

                    //set the default app pool of the new site to the pool we just created. 
                    mySite.ApplicationDefaults.ApplicationPoolName= SiteName + "-Pool";

                    //commit.
                    m.CommitChanges();

              }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool CreateTheDNSEntry(string SiteName)
        {
            return true;
        }

        [WebMethod]
        public void SendMail()
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("r3adm3@gmail.com", "31n5t31n"), EnableSsl = true};
            client.Send("PaaS@nofishhere.com", "r3adm3@gmail.com", "test subject", "test body");
        }

        private bool CheckforDNSEntry(string SiteName)
        {
        
            //add in the domain lookup here.
            SiteName = SiteName + DNSSuffix;

            try
            {
                //dns name already exists
                IPHostEntry ipEntry;

                ipEntry = Dns.GetHostEntry(SiteName);

                return true;

            }
            catch
            {
                //dns doesn't exist
                return false;
            }
            
            
        }

        [WebMethod]
        public void AddDNSRecord(string hostname)
        {
            AddARecord(hostname, "nofishhere.local", "192.168.1.103", "boston.nwtraders.msft");
        }

        public void AddARecord(string hostName, string zone, string iPAddress, string dnsServerName)
        {
            /*ConnectionOptions connOptions = new ConnectionOptions();
            connOptions.Impersonation = ImpersonationLevel.Impersonate;
            connOptions.EnablePrivileges = true;
            connOptions.Username = "nwtraders\administrator";
            connOptions.Password = "Password01";
            */
            ManagementScope scope =
               new ManagementScope(@"\\" + dnsServerName + "\\root\\MicrosoftDNS"); //,connOptions);

            scope.Connect();

            ManagementClass wmiClass =
               new ManagementClass(scope,
                                   new ManagementPath("MicrosoftDNS_AType"),
                                   null);

            ManagementBaseObject inParams =
                wmiClass.GetMethodParameters("CreateInstanceFromPropertyData");

            inParams["DnsServerName"] = dnsServerName;
            inParams["ContainerName"] = zone;
            inParams["OwnerName"] = hostName + "." + zone;
            inParams["IPAddress"] = iPAddress;

            wmiClass.InvokeMethod("CreateInstanceFromPropertyData", inParams, null);
        }

        [WebMethod]
        public string AllSites()
        {
            StringBuilder sb = new StringBuilder();
            ServerManager ServerManager = new ServerManager() ;
            
                foreach (Site MySite in ServerManager.Sites)
                {
                    sb.Append(MySite.Name.ToString() + "\r\n");
                }
            
            return sb.ToString();

        }

    }
}