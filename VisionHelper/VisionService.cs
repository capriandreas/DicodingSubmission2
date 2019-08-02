using BlobHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace VisionHelper
{
    public class VisionService
    {
        string subscriptionKey = string.Empty;
        string endPoint = string.Empty;

        public VisionService()
        {
            this.subscriptionKey = AppConfiguration.GetConfiguration("ApiKey");
            this.endPoint = AppConfiguration.GetConfiguration("EndPoint");
        }

        public async Task<ImageInfoViewModel> MakeAnalysisRequest(string urlImage, Stream streamImage)
        {
            var errors = new List<string>();
            ImageInfoViewModel responeData = new ImageInfoViewModel();
            try
            {
                HttpClient client = new HttpClient();
                // Request headers.    
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                // Request parameters. A third optional parameter is "details".    
                string requestParameters = "visualFeatures=Categories,Description,Color&language=en";
                // Assemble the URI for the REST API Call.    
                string uri = endPoint + "?" + requestParameters;
                HttpResponseMessage response;
                // Request body. Posts a locally stored JPEG image.
                var webClient = new WebClient();
                byte[] imageBytes = webClient.DownloadData(urlImage);
                //byte[] byteData = GetImageAsByteArray(urlImage);
                using (ByteArrayContent content = new ByteArrayContent(imageBytes))
                {
                    // This example uses content type "application/octet-stream".    
                    // The other content types you can use are "application/json"    
                    // and "multipart/form-data".    
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    // Make the REST API call.    
                    response = await client.PostAsync(uri, content);
                }
                // Get the JSON response.    
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    responeData = JsonConvert.DeserializeObject<ImageInfoViewModel>(result, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs earg) {
                            errors.Add(earg.ErrorContext.Member.ToString());
                            earg.ErrorContext.Handled = true;
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
            return responeData;
        }
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(imageFilePath);

            using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}
