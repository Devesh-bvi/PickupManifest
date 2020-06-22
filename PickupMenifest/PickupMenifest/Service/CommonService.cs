using System;
using System.IO;
using System.Net;
using System.Text;

namespace PickupMenifest.Service
{
    public class CommonService
    {

        public static string SendApiRequest(string url, string Request)
        {
            string strresponse = "";
            try
            {
                var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "text/json";

                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    if (!string.IsNullOrEmpty(Request))
                        streamWriter.Write(Request);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    strresponse = streamReader.ReadToEnd();
                }
                
            }
            //catch (WebException ex)
            //{
            //    string message = ex.Message;

            //    WebResponse errorResponse = ex.Response;
            //    if (errorResponse != null)
            //    {
            //        using (Stream responseStream = errorResponse.GetResponseStream())
            //        {
            //            StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            //            message = reader.ReadToEnd();
            //        }
            //    }
            //    //using (WebResponse response = e.Response)
            //    //{
            //    //    HttpWebResponse httpResponse = (HttpWebResponse)response;
            //    //    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
            //    //    using (Stream data = response.GetResponseStream())
            //    //    using (var reader = new StreamReader(data))
            //    //    {
            //    //        string text = reader.ReadToEnd();
            //    //        Console.WriteLine(text);
            //    //    }
            //    //}
            //}
            catch (Exception ex)
            {
               
            }

            return strresponse;

        }
    }
}
