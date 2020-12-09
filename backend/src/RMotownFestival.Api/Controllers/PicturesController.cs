using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RMotownFestival.Api.Common;
using System;
using System.Linq;

namespace RMotownFestival.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PicturesController : ControllerBase
    {
        public PicturesController(BlobUtility blobUtility)
        {
            BlobUtility = blobUtility;
        }

        private BlobUtility BlobUtility { get; }

        [HttpGet]
        public string[] GetAllPictureUrls()
        {
            BlobContainerClient container = BlobUtility.GetThumbsContainer();

            return container.GetBlobs()
                .Select(blob => BlobUtility.GetSasUri(container, blob.Name))
                .ToArray();
        }

        [HttpPost]
        public void PostPicture(IFormFile file)
        {
            BlobContainerClient container = BlobUtility.GetPicturesContainer();
            container.UploadBlob(file.FileName, file.OpenReadStream());
        }
    }
}
