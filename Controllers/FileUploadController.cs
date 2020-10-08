using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WebUpload.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly ILogger<FileUploadController> _logger;
        private IBlobService _blobService;
        readonly string ContainerName = "fileupload";

        public FileUploadController(IBlobService blobService, ILogger<FileUploadController> logger)
        {
            _blobService = blobService;
            _logger = logger;
        }

        [HttpGet]
        public string Get([FromQuery] string fileName)
        {
            return _blobService.DownloadFileBlobAsync(ContainerName, fileName);
        }

        [HttpPost, RequestSizeLimit(50000000)] // Request Size Limit set to 50 MB . Request size can also be set at kestrel level
        public async Task<ActionResult> Post(ICollection<IFormFile> files)
        {
            bool isUploaded = false;

            try
            {
                if (files.Count == 0)
                {
                    return BadRequest("File's are not attached.");
                }

                foreach (var formFile in files)
                {
                    if (Helper.IsFileAllowed(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                isUploaded = await _blobService.UploadFileBlobAsync(ContainerName, stream, formFile.FileName);
                            }
                        }
                    }

                }

                if (isUploaded)
                {
                    return new AcceptedResult();
                }
                else
                {
                    return BadRequest("Data upload has been failed. Please try again.");
                }

            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }

    }
}
