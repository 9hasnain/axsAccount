using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AccountCreator
{
    public class HttpHandler
    {


        

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string GetFirstPageHTML(CookieContainer cookies, DataRow proxy, string html)
        {
            string prefixDomain = "www.axs.com";
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                //this.prefixDomain = "ticketing.axs.com";
                string result = "";
                string url = "";
                string reff = "";
                //result = httpHandlerRef.Get(this.TicketUrl, cookies, out response, out request, "", proxy);

                //foreach (Cookie item in response.Cookies)
                //{
                //    cookies.Add(item);
                //}

                url = "https://" + prefixDomain + "/" + GetStringInBetween("src=\"/", "\"", html, false, false)[0];
                reff = url;
                result = Get(url, cookies, out response, out request, "");


                var hasher = new SHA1Managed();
                var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(result));
                var byteString = BitConverter.ToString(hash);
                var theSHA1Hash = byteString.Replace("-", "").ToLower();
                string base64 = Base64Encode(result);

                url = "https://distilnet.works/api/v1/session";
                string body = @"{""JsSha1"":""" + theSHA1Hash + @""",""JsUri"":""" + reff + @""",""JsData"":""" + base64 + @"""}";
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("Auth-Key", "690607149cc7c0a4de2bae9c5aa53e1297effe4f");


                result = Post(url, new NameValueCollection(), cookies, body, out response, out request, "", proxy, "", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8", true, "", "", headers);

                JObject Jobj = JObject.Parse(result);
                WebHeaderCollection header = new WebHeaderCollection();
                header.Add(Jobj["Headers"][0].ToString());
                foreach (var item in Jobj["tasks"])
                {
                    url = "https://" + prefixDomain + item["uri"].ToString();
                    if (item["method"].ToString() == "POST")
                    {

                        result = Post(url, new NameValueCollection(), cookies, item["data"].ToString(), out response, out request, "", proxy, "", "application/x-www-form-urlencoded", false, "", "", header);
                        break;
                    }
                    else
                    {
                        // HttpHandler.HttpClientCall(url, cookies, null, header).GetAwaiter().GetResult();
                        //resetEvent.WaitOne();
                    }
                }
                //check if page comes
                //result = HttpHandler.Get(baseUrl, cookies, out response, out request, "", null, "", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");


                //string cookieString = HTTPHandler.GetAllCookies(response);
                //string httpReferrer = "/event/" + EventID;
                //string uid = StringHandler.GetStringInBetween("D_ZUID=", ";", cookieString, false, false)[0]; //5FF484E6-E2F3-33E4-92C1-057FE26D024D

                //url = "https://" + prefixDomain + "/distil_identify_cookie.html?httpReferrer=" + HttpUtility.UrlEncode(httpReferrer) + "&uid=" + uid;

                // result = httpHandlerRef.Get(url, cookies, out response, out request, "", proxy, true);

                return result;
            }
            catch
            {
            }
            return "";
        }
        public bool Request_api_axs_com_post(out HttpWebResponse response, CookieContainer cookie, string postData)
        {
            response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.axs.com/proxy/v2/flash/migrate/create?access_token=4f2be33d835e7197e245c54ff00e5fb4&locale=en-US&region=1");

                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.ContentType = "application/json";
                request.Headers.Add("Origin", @"https://www.axs.com");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.CookieContainer = cookie;
                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;
                request.AutomaticDecompression = DecompressionMethods.GZip;

                Random rnd = new Random();
                string group = Properties.Settings.Default.UserCloud + "-country-us-session-" + rnd.Next(1111, 9999);
                request.ConnectionGroupName = group;

                try
                {
                    WebProxy myProxy = new WebProxy();

                    myProxy.Address = new Uri("http://servercountry-us.zproxy.luminati.io:22225");
                    myProxy.Credentials = new NetworkCredential(group, Properties.Settings.Default.UserPassword);

                    //myProxy.Address = new Uri("http://127.0.0.1:8888");
                    request.Proxy = myProxy;
                }
                catch (Exception ex)
                {

                }
                request.Referer = "https://www.axs.com/";

                string body = postData;
                byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(body);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);
                stream.Close();

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if (response != null) response.Close();
                return false;
            }

            return true;
        }

        public string Post(string url,
        NameValueCollection keyValues,
        CookieContainer cookieContainer,
        string paramParameters,
        out HttpWebResponse response,
        out HttpWebRequest request,
        string referer,
        DataRow proxyRow,
         string group,
         string contentType,
        bool redirect = false,
        string host = "",
        string headers = "",
        WebHeaderCollection headerCollection = null,
        string accept = "")
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // is the cookie container instanced? no, create one
            if (cookieContainer == null)
                cookieContainer = new CookieContainer();
            // create the http web request
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));

            if (cookieContainer != null)
            {
                httpWebRequest.CookieContainer = cookieContainer;
            }
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";

            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            httpWebRequest.ContentType = "application/json";
            //httpWebRequest.Headers.Add("Cache-Control", "max-age=0");
            //httpWebRequest.Headers.Add("Upgrade-Insecure-Requests", "1");
            //httpWebRequest.Connection = "keep-alive";
            httpWebRequest.Headers.Add("Origin", "https://www.axs.com");
            httpWebRequest.Headers.Add("Accept-Language", "en-US,en;q=0.9");
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            //httpWebRequest.Headers.Add("X-Distil-Ajax", "twzvbatvrxzavsfzbzeyurav");
            httpWebRequest.Method = "POST";
            httpWebRequest.ProtocolVersion = HttpVersion.Version11;
            httpWebRequest.ServicePoint.Expect100Continue = false;
            //httpWebRequest.UserAgent = userAgent;
            httpWebRequest.KeepAlive = true;
            //httpWebRequest.KeepAlive = true;
            httpWebRequest.AllowAutoRedirect = redirect;
            httpWebRequest.Referer = referer;
            //httpWebRequest.Timeout = timeOut;
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
            if (accept != string.Empty)
            {
                httpWebRequest.Accept = accept;
            }
            if (headers != string.Empty)
            {
                httpWebRequest.Headers.Add(headers);
            }
            if (host != string.Empty)
            {
                httpWebRequest.Host = host;
            }
            if (headerCollection != null)
            {
                foreach (string header in headerCollection)
                {
                    httpWebRequest.Headers.Add(header, headerCollection[header]);
                }
            }
            // encode the parameters
            string parameters = "";
            foreach (string key in keyValues.AllKeys)
            {
                if (parameters != "") parameters += "&";
                {
                    parameters += key + "=" + System.Web.HttpUtility.UrlEncode(keyValues[key]);
                }
                parameters.Trim(new char[] { '&' });
            }
            paramParameters = paramParameters.Trim(new char[] { '&' }) + "&" + parameters;
            paramParameters = paramParameters.Trim(new char[] { '&' });
            // convert the parameters to a byte array
            byte[] parameterBytes = System.Text.Encoding.UTF8.GetBytes(paramParameters);
            httpWebRequest.ContentLength = parameterBytes.Length;

            Random rnd = new Random();
            group = Properties.Settings.Default.UserCloud + "-country-us-session-" + rnd.Next(1111, 9999);
            httpWebRequest.ConnectionGroupName = group;

            try
            {
                WebProxy myProxy = new WebProxy();

                myProxy.Address = new Uri("http://servercountry-us.zproxy.luminati.io:22225");
                myProxy.Credentials = new NetworkCredential(group, Properties.Settings.Default.UserPassword);

                //myProxy.Address = new Uri("http://127.0.0.1:8888");
                httpWebRequest.Proxy = myProxy;
            }
            catch (Exception ex)
            {

            }
            httpWebRequest.Referer = referer;

            System.Net.WebResponse webResponse = null;
            // read the response
            try
            {
                // sent the request, write the parameters
                System.IO.Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(parameterBytes, 0, parameterBytes.Length);
                requestStream.Close();
                int counter = 0;
                
                try
                {
                    webResponse = httpWebRequest.GetResponse();
                    counter = 4;
                }
                catch (Exception ex)
                {
                    
                }
                
            }
            catch (Exception ex)
            {
            }

            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            StreamReader streamReader = new System.IO.StreamReader(webResponse.GetResponseStream());
            string result = streamReader.ReadToEnd().Trim();
            HttpWebResponse httpWebResponse = (HttpWebResponse)webResponse;
            response = httpWebResponse;
            request = httpWebRequest;
            // return the result
            try
            {
                webResponse.Close();
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get
        /// </summary>
        /// <param name="url"></param>
        /// <param name="keyValues"></param>
        /// <param name="cookieContainer"></param>
        /// <returns></returns>
        /// 
        // ----------------------------------------------------------------------------------------
        public string[] GetStringInBetween(string strBegin, string strEnd, string strSource, bool includeBegin, bool includeEnd)
        {
            try
            {
                string[] result = { "", "" };
                int iIndexOfBegin = strSource.IndexOf(strBegin);
                if (iIndexOfBegin != -1)
                {
                    // include the Begin string if desired
                    if (includeBegin)
                        iIndexOfBegin -= strBegin.Length;
                    strSource = strSource.Substring(iIndexOfBegin
                        + strBegin.Length);
                    int iEnd = strSource.IndexOf(strEnd);
                    if (iEnd != -1)
                    {
                        // include the End string if desired
                        if (includeEnd)
                            iEnd += strEnd.Length;
                        result[0] = strSource.Substring(0, iEnd);
                        // advance beyond this segment
                        if (iEnd + strEnd.Length < strSource.Length)
                            result[1] = strSource.Substring(iEnd
                                + strEnd.Length);
                    }
                }
                else
                    // stay where we are
                    result[1] = strSource;
                return result;
            }
            catch (Exception)
            {
                string[] result = { "", "" };
                return result;
            }
        }
        public string Get(string url,
                                    CookieContainer cookieContainer,
                                    out HttpWebResponse response,
                                    out HttpWebRequest request,
                                    string referer,
                                    bool autoReDirect = true,
                                    WebHeaderCollection headers = null)
        {
            try
            {
                System.Net.ServicePointManager.Expect100Continue = false;
                // is the cookie container instanced? no, create one
                if (cookieContainer == null)
                    cookieContainer = new CookieContainer();

                // encode the parameters
                string parameters = "";

                // create the http web request
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Method = "GET";
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                httpWebRequest.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                //httpWebRequest.Accept = "*/*";
                httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate"); //sdch");
                httpWebRequest.Headers.Add("Upgrade-Insecure-Requests", @"1");
                httpWebRequest.KeepAlive = true;
                httpWebRequest.AllowAutoRedirect = autoReDirect;
                httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                //httpWebRequest.ConnectionGroupName = connectiongroupname;
                if (headers != null)
                {
                    httpWebRequest.Headers = headers;
                }
                if (referer != string.Empty)
                {
                    httpWebRequest.Referer = referer;
                }
                WebProxy myProxy = new WebProxy();
                //Obtain the Proxy Prperty of the  Default browser. 
                myProxy.Address = new Uri("http://127.0.0.1:8888");
                httpWebRequest.Proxy = myProxy;
                System.Net.WebResponse webResponse = null;
                // read the response
                try
                {
                    int counter = 0;
                    while (counter <= 3)
                    {
                        try
                        {
                            webResponse = httpWebRequest.GetResponse();
                            counter = 4;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(3000);
                            counter += 1;
                            if (counter == 3)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (webResponse != null)
                    {
                        try
                        {
                            //webResponse.Close();
                        }
                        catch (Exception ex1)
                        {
                            // ProgramManager.LogException(ex1);
                        }
                    }
                }

                System.IO.StreamReader streamReader = new System.IO.StreamReader(webResponse.GetResponseStream());
                Stream receiveStream = webResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, encode);

                string result = streamReader.ReadToEnd().Trim();
                HttpWebResponse httpWebResponse = (HttpWebResponse)webResponse;
                response = httpWebResponse;
                request = httpWebRequest;
                if (webResponse != null)
                {
                    try
                    {
                        webResponse.Close();
                        try
                        {
                            webResponse.Close();
                        }
                        catch (Exception ex)
                        {
                            // ProgramManager.LogException(ex);
                        }
                    }
                    catch (Exception ex1)
                    {
                        //ProgramManager.LogException(ex1);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// PostUrlEncoded
        /// </summary>
        /// <param name="url"></param>
        /// <param name="keyValues"></param>
        /// <param name="cookieContainer"></param>
        /// <returns></returns>
        // ----------------------------------------------------------------------------------------
        public string PostUrlEncoded(string url,
            NameValueCollection keyValues,
            CookieContainer cookieContainer,
            string paramParameters,
            out HttpWebResponse response,
            out HttpWebRequest request,
            string referer,
            WebHeaderCollection headers = null)
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            // is the cookie container instanced? no, create one
            if (cookieContainer == null)
                cookieContainer = new CookieContainer();

            // create the http web request
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));

            if (cookieContainer != null)
            {
                httpWebRequest.CookieContainer = cookieContainer;
            }

            httpWebRequest.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");
            httpWebRequest.Headers.Add("Origin", @"http://www.howstat.com");
            httpWebRequest.Headers.Add("Upgrade-Insecure-Requests", @"1");
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36";
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            httpWebRequest.Referer = "http://www.howstat.com/cricket/Statistics/Players/PlayerMenu.asp";
            httpWebRequest.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            httpWebRequest.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            httpWebRequest.Method = "POST";
            httpWebRequest.ServicePoint.Expect100Continue = false;

            WebProxy myProxy = new WebProxy();
            //Obtain the Proxy Prperty of the  Default browser. 
            myProxy.Address = new Uri("http://127.0.0.1:8888");
            httpWebRequest.Proxy = myProxy;
            httpWebRequest.Referer = referer;

            // encode the parameters
            string parameters = "";
            foreach (string key in keyValues.AllKeys)
            {
                if (parameters != "") parameters += "&";
                parameters += key + "=" + System.Web.HttpUtility.UrlEncode(keyValues[key]);
            }

            // convert the parameters to a byte array
            byte[] parameterBytes = System.Text.Encoding.UTF8.GetBytes(paramParameters);
            httpWebRequest.ContentLength = parameterBytes.Length;

            if (headers != null)
            {
                httpWebRequest.Headers = headers;
            }

            //WebProxy myProxy = new WebProxy();
            //// Obtain the Proxy Prperty of the  Default browser. 
            //myProxy.Address = new Uri("http://127.0.0.1:8118");
            //httpWebRequest.Proxy = myProxy;
            System.Net.WebResponse webResponse = null;

            // read the response
            try
            {
                // sent the request, write the parameters
                System.IO.Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(parameterBytes, 0, parameterBytes.Length);
                requestStream.Close();
                int counter = 0;
                while (counter <= 3)
                {
                    try
                    {
                        webResponse = httpWebRequest.GetResponse();
                        counter = 4;
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(3000);
                        counter += 1;
                        if (counter == 3)
                        {
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (webResponse != null)
                {
                    try
                    {
                        //webResponse.Close();
                    }
                    catch (Exception ex1)
                    {
                        // ProgramManager.LogException(ex1);
                    }
                }
            }
            string result = string.Empty;
            Stream responseStream = webResponse.GetResponseStream();
            using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8, true))
            {
                result = reader.ReadToEnd();
            }
            //StreamReader streamReader = new System.IO.StreamReader(webResponse.GetResponseStream());
            //string result = streamReader.ReadToEnd().Trim();
            HttpWebResponse httpWebResponse = (HttpWebResponse)webResponse;
            response = httpWebResponse;
            request = httpWebRequest;
            // return the result
            if (webResponse != null)
            {
                try
                {
                    webResponse.Close();
                    try
                    {
                        webResponse.Close();
                    }
                    catch (Exception ex)
                    {
                        // ProgramManager.LogException(ex);
                    }
                }
                catch (Exception ex1)
                {
                    //  ProgramManager.LogException(ex1);
                }
            }
            return result;
        }

        public bool Get_www_howstat_com(string url, out HttpWebResponse response)
        {
            response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36";
                request.Headers.Add("Upgrade-Insecure-Requests", @"1");
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                //request.Headers.Set(HttpRequestHeader.Cookie, @"__utmz=1903400.1512424960.1.1.utmcsr=google|utmccn=(organic)|utmcmd=organic|utmctr=(not%20provided); ASPSESSIONIDQQTTDSTC=AMGIBFECEHLLCKCGGHJGKPGJ; __utma=1903400.85938762.1512424960.1512741286.1513471129.3; __utmc=1903400; __utmt=1; __utmb=1903400.18.10.1513471129");
                WebProxy myProxy = new WebProxy();
                //Obtain the Proxy Prperty of the  Default browser. 
                myProxy.Address = new Uri("http://127.0.0.1:8888");
                request.Proxy = myProxy;
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if (response != null) response.Close();
                return false;
            }

            return true;
        }

        public bool Post_www_howstat_com(string paramParameters, out HttpWebResponse response)
        {
            response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.howstat.com/cricket/Statistics/Players/PlayerMenu.asp");

                request.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");
                request.Headers.Add("Origin", @"http://www.howstat.com");
                request.Headers.Add("Upgrade-Insecure-Requests", @"1");
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.Referer = "http://www.howstat.com/cricket/Statistics/Players/PlayerMenu.asp";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;

                WebProxy myProxy = new WebProxy();
                //Obtain the Proxy Prperty of the  Default browser. 
                myProxy.Address = new Uri("http://127.0.0.1:8888");
                request.Proxy = myProxy;
                response = (HttpWebResponse)request.GetResponse();

                byte[] parameterBytes = System.Text.Encoding.UTF8.GetBytes(paramParameters);
                request.ContentLength = parameterBytes.Length;
                System.IO.Stream requestStream = request.GetRequestStream();
                requestStream.Write(parameterBytes, 0, parameterBytes.Length);
                requestStream.Close();

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if (response != null) response.Close();
                return false;
            }

            return true;
        }
        public bool Request_analytics05_cricket_net(out HttpWebResponse response)
        {
            response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://analytics05.cricket.net/xquery/espn/player");

                request.Headers.Add("Origin", @"http://www.espncricinfo.com");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";
                request.ContentType = "text/plain";
                request.Accept = "*/*";
                request.Referer = "http://www.espncricinfo.com/ci/content/zones/insights?insights=player&player_id=1934&format=test&stats=bowling";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");

                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;

                WebProxy myProxy = new WebProxy();
                //Obtain the Proxy Prperty of the  Default browser. 
                myProxy.Address = new Uri("http://127.0.0.1:8888");
                request.Proxy = myProxy;

                string body = @"{""type"":""player-activity"",""format"":""test"",""player_id"":""1934"",""ttl"":300,""bowled"":""yes""}";
                byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(body);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);
                stream.Close();

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if (response != null) response.Close();
                return false;
            }

            return true;
        }

        public string PostUrlEncodedJSON(string url,
            NameValueCollection keyValues,
            CookieContainer cookieContainer,
            string paramParameters,
            out HttpWebResponse response,
            out HttpWebRequest request,
            string referer,
            WebHeaderCollection headers = null)
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            // is the cookie container instanced? no, create one
            if (cookieContainer == null)
                cookieContainer = new CookieContainer();

            // create the http web request
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));

            if (cookieContainer != null)
            {
                httpWebRequest.CookieContainer = cookieContainer;
            }

            httpWebRequest.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");
            httpWebRequest.Headers.Add("Origin", @"http://www.espncricinfo.com");
            httpWebRequest.Headers.Add("Upgrade-Insecure-Requests", @"1");
            httpWebRequest.ContentType = "text/plain";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Referer = referer;
            httpWebRequest.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            httpWebRequest.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            httpWebRequest.Method = "POST";
            httpWebRequest.ServicePoint.Expect100Continue = false;

            WebProxy myProxy = new WebProxy();
            //Obtain the Proxy Prperty of the  Default browser. 
            myProxy.Address = new Uri("http://127.0.0.1:8888");
            httpWebRequest.Proxy = myProxy;
            httpWebRequest.Referer = referer;

            // encode the parameters
            string parameters = "";
            foreach (string key in keyValues.AllKeys)
            {
                if (parameters != "") parameters += "&";
                parameters += key + "=" + System.Web.HttpUtility.UrlEncode(keyValues[key]);
            }

            // convert the parameters to a byte array
            byte[] parameterBytes = System.Text.Encoding.UTF8.GetBytes(paramParameters);
            httpWebRequest.ContentLength = parameterBytes.Length;

            if (headers != null)
            {
                httpWebRequest.Headers = headers;
            }

            //WebProxy myProxy = new WebProxy();
            //// Obtain the Proxy Prperty of the  Default browser. 
            //myProxy.Address = new Uri("http://127.0.0.1:8118");
            //httpWebRequest.Proxy = myProxy;
            System.Net.WebResponse webResponse = null;

            // read the response
            try
            {
                // sent the request, write the parameters
                System.IO.Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(parameterBytes, 0, parameterBytes.Length);
                requestStream.Close();
                int counter = 0;
                while (counter <= 3)
                {
                    try
                    {
                        webResponse = httpWebRequest.GetResponse();
                        counter = 4;
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(3000);
                        counter += 1;
                        if (counter == 3)
                        {
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (webResponse != null)
                {
                    try
                    {
                        //webResponse.Close();
                    }
                    catch (Exception ex1)
                    {
                        // ProgramManager.LogException(ex1);
                    }
                }
            }
            string result = string.Empty;
            Stream responseStream = webResponse.GetResponseStream();
            using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8, true))
            {
                result = reader.ReadToEnd();
            }
            //StreamReader streamReader = new System.IO.StreamReader(webResponse.GetResponseStream());
            //string result = streamReader.ReadToEnd().Trim();
            HttpWebResponse httpWebResponse = (HttpWebResponse)webResponse;
            response = httpWebResponse;
            request = httpWebRequest;
            // return the result
            if (webResponse != null)
            {
                try
                {
                    webResponse.Close();
                    try
                    {
                        webResponse.Close();
                    }
                    catch (Exception ex)
                    {
                        // ProgramManager.LogException(ex);
                    }
                }
                catch (Exception ex1)
                {
                    //  ProgramManager.LogException(ex1);
                }
            }
            return result;
        }




    }
}
