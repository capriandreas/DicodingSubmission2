using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlobUpload.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using BlobHelper;
using VisionHelper;
using Newtonsoft.Json;

namespace BlobUpload.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IFormFile files)
        {
            try
            {
                if (files != null)
                {
                    BlobStorageService objBlobService = new BlobStorageService();
                    VisionService visionService = new VisionService();

                    byte[] bufferFile = new byte[files.Length];
                    Stream stream = files.OpenReadStream();
                    stream.Read(bufferFile, 0, Convert.ToInt32(files.Length));
                    string base64Files = Convert.ToBase64String(bufferFile);
                    string SavePathDestination = "uploadsblob";
                    string filename = Path.Combine(Path.GetFileName(files.FileName));
                    var buffer = Convert.FromBase64String(base64Files);
                    MemoryStream ms = new MemoryStream(buffer);

                    BlobReference blobRef = await objBlobService.GetBlobForWriteAsync(Path.Combine(SavePathDestination, filename));
                    using (Stream blobStream = blobRef.BlobStream)
                    {
                        await ms.CopyToAsync(blobStream);
                    }

                    var urlUploadedFile = blobRef.BlobUri;
                    Stream streamFile = blobRef.BlobStream;

                    if (urlUploadedFile != null || urlUploadedFile != "")
                    {
                        ImageInfoViewModel resImage = await visionService.MakeAnalysisRequest(urlUploadedFile, streamFile);
                        string jsonData = JsonConvert.SerializeObject(resImage);
                        ViewBag.ImageDesc = resImage.description.captions[0].text;
                        ViewBag.jsonData = jsonData;
                        ViewBag.urlUploadedFile = urlUploadedFile;
                    }
                    else
                    {
                        ViewBag.response = "File Failed to Upload";
                    }
                }
                else
                {
                    ViewBag.response = "Please Select Image to Upload ...";
                }
                
            }
            catch (Exception ex)
            {
                ViewBag.response = "Exception catch : " + ex;
            }

            return View();
        }

    }
}
