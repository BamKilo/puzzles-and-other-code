﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;

namespace FennecFox
{
    public class HtmlHelper
    {
        private static String USER_AGENT = "Mozilla/5.0 (Windows NT 6.1; U; ru; rv:5.0.1.6) Gecko/20110501 Firefox/5.0.1 Firefox/5.0.1";

        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        public static String PostToUrl(ConnectionSettings settings)
        {
            String strResponse = null;
            using (
                Stream stream = PostToUrlStream(settings))
            {
                if (settings.Message == null)
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        strResponse = sr.ReadToEnd();
                    }
                }
            }

            return strResponse;
        }

        public static Stream PostToUrlStream(ConnectionSettings settings)
        {
            settings.Message = null;
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] dataBytes = encoding.GetBytes(settings.Data);
            Stream responseStream = null;
            try
            {
                // Prepare web request...
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(settings.Url);
                myRequest.Proxy = null;

                // DEBUG ONLY
#if DEBUG
                //myRequest.Proxy = new WebProxy("127.0.0.1", 8888);
#endif
                myRequest.CookieContainer = settings.CC;
                myRequest.Method = "POST";
                myRequest.Timeout = 600000;
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.UserAgent = USER_AGENT;
                myRequest.ContentLength = settings.Data.Length;
                myRequest.AllowAutoRedirect = settings.FollowRedirect;
                myRequest.KeepAlive = true;
                myRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip |
                                                   DecompressionMethods.None;

                myRequest.ConnectionGroupName = "normal";
                if (settings.UseUnsafeAuthenticatedConnectionSharing)
                {
                    myRequest.UnsafeAuthenticatedConnectionSharing = true;
                    myRequest.ConnectionGroupName = "auto_parsing";
                }

                /* DEBUG ONLY, to handle HTTPS through fiddler.  also uncomment "proxy" above. */

#if DEBUG
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                ServicePointManager.ServerCertificateValidationCallback +=
                    delegate(object sender, X509Certificate certificate, X509Chain chain,
                    SslPolicyErrors sslPolicyErrors)
                    {
                        return true;
                    };
#endif

                foreach (KeyValuePair<String, String> header in settings.Headers)
                {
                    if (header.Key == "User-Agent")
                    {
                        myRequest.UserAgent = header.Value;
                    }
                    else if (header.Key == "Accept")
                    {
                        myRequest.Accept = header.Value;
                    }
                    else if (header.Key == "Referer")
                    {
                        myRequest.Referer = header.Value;
                    }
                    else if (header.Key == "Connection")
                    {
                        myRequest.Connection = header.Value;
                    }
                    else if (header.Key == "Content-Type")
                    {
                        myRequest.ContentType = header.Value;
                    }
                    else if (header.Key == "Expect100Continue")
                    {
                        myRequest.ServicePoint.Expect100Continue = bool.Parse(header.Value);
                    }
                    else
                    {
                        if (myRequest.Headers[header.Key] != null)
                        {
                            myRequest.Headers.Remove(header.Key);
                        }

                        myRequest.Headers.Add(header.Key, header.Value);
                    }
                }

                if (settings.UseAuthentication)
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(MyCertValidationCb);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                }

                using (Stream newStream = myRequest.GetRequestStream())
                {
                    // Send the data.
                    newStream.Write(dataBytes, 0, dataBytes.Length);
                }

                using (HttpWebResponse objResponse = (HttpWebResponse)myRequest.GetResponse())
                {
                    responseStream = new MemoryStream();
                    CopyStream(objResponse.GetResponseStream(), responseStream);
                    responseStream.Seek(0, SeekOrigin.Begin);
                }
            }
            catch (Exception e)
            {
                if (!settings.IgnoreErrors)
                {
                    Console.WriteLine(e.ToString());
                }

                settings.Message = e.Message;
                return null;
            }

            return responseStream;
        }

        public static String GetUrlResponseString(ConnectionSettings settings)
        {
            /*String strURL, CookieContainer cc, bool useAuthentication,
                                                  NetworkCredential credentials, Int32 timeout, Dictionary<String, String> headers, out String message) */
            String toRet = null;
            using (Stream stream = GetUrlStream(settings))
            {
                if (settings.Message == null)
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        toRet = sr.ReadToEnd();
                    }
                }
            }

            return toRet;
        }

        public static Stream GetUrlStream(ConnectionSettings settings)
        {
            settings.Message = null;
            Stream objStreamReceive = null;

            try
            {
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(settings.Url);

                objRequest.Proxy = null;

                // DEBUG ONLY
#if DEBUG
                //objRequest.Proxy = new WebProxy("127.0.0.1", 8888);
#endif
                objRequest.CookieContainer = settings.CC;
                objRequest.Timeout = 600000;
                objRequest.Method = "GET";
                objRequest.UserAgent = USER_AGENT;
                objRequest.AllowAutoRedirect = true;
                objRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip |
                                                    DecompressionMethods.None;

                foreach (KeyValuePair<String, String> header in settings.Headers)
                {
                    if (header.Key == "User-Agent")
                    {
                        objRequest.UserAgent = header.Value;
                    }
                    else if (header.Key == "Accept")
                    {
                        objRequest.Accept = header.Value;
                    }
                    else if (header.Key == "Referer")
                    {
                        objRequest.Referer = header.Value;
                    }
                    else if (header.Key == "Connection")
                    {
                        objRequest.Connection = header.Value;
                    }
                    else if (header.Key == "Content-Type")
                    {
                        objRequest.ContentType = header.Value;
                    }
                    else if (header.Key == "Expect100Continue")
                    {
                        objRequest.ServicePoint.Expect100Continue = bool.Parse(header.Value);
                    }
                    else
                    {
                        if (objRequest.Headers[header.Key] != null)
                        {
                            objRequest.Headers.Remove(header.Key);
                        }

                        objRequest.Headers.Add(header.Key, header.Value);
                    }
                }

                /* DEBUG ONLY, to handle HTTPS through fiddler.  also uncomment "proxy" above. */

#if DEBUG
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                ServicePointManager.ServerCertificateValidationCallback +=
                    delegate(object sender, X509Certificate certificate, X509Chain chain,
                    SslPolicyErrors sslPolicyErrors)
                    {
                        return true;
                    };
#endif

                if (settings.UseAuthentication)
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(MyCertValidationCb);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                }

                using (var objResponse = (HttpWebResponse)objRequest.GetResponse())
                {
                    objStreamReceive = new MemoryStream();
                    CopyStream(objResponse.GetResponseStream(), objStreamReceive);
                    objStreamReceive.Seek(0, SeekOrigin.Begin);
                }
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());

                settings.Message = excep.Message;
                return null;
            }

            return objStreamReceive;
        }


        public static bool MyCertValidationCb(
           object sender,
           X509Certificate certificate,
           X509Chain chain,
           SslPolicyErrors sslPolicyErrors)
        {
            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors)
                == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                return false;
            }
            else if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch)
                     == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                Zone z;
                z = Zone.CreateFromUrl(((HttpWebRequest)sender).RequestUri.ToString());
                if (z.SecurityZone == SecurityZone.Intranet
                    || z.SecurityZone == SecurityZone.MyComputer)
                {
                    return true;
                }
                return false;
            }

            return true;
        }
    }
}