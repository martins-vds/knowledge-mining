// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using KnowledgeMining.UI.Services.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KnowledgeMining.UI.Controllers
{
    public class StorageController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;

        private readonly ISearchService _docSearch;
        private readonly ILogger<StorageController> _logger;

        private readonly Options.StorageOptions _storageOptions;

        public StorageController(BlobServiceClient blobServiceClient,
                                 ISearchService searchService,
                                 IOptions<Options.StorageOptions> storageOptions,
                                 ILogger<StorageController> logger)
        {
            _blobServiceClient = blobServiceClient;
            _storageOptions = storageOptions.Value;
            _docSearch = searchService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(CancellationToken cancellationToken)
        {
            if (Request.Form.Files.Any())
            {
                var container = GetBlobContainerClient();

                foreach (var formFile in Request.Form.Files)
                {
                    if (formFile.Length > 0)
                    {
                        var blob = container.GetBlobClient(formFile.FileName);
                        var blobHttpHeader = new BlobHttpHeaders();
                        blobHttpHeader.ContentType = formFile.ContentType;
                        using var fileStream = formFile.OpenReadStream();
                        await blob.UploadAsync(fileStream, blobHttpHeader, cancellationToken: cancellationToken);
                    }
                }
            }

            await _docSearch.RunIndexer(cancellationToken);

            return new JsonResult("ok");
        }

        /// <summary>
        ///  Returns the requested document with an 'inline' content disposition header.
        ///  This hints to a browser to show the file instead of downloading it.
        /// </summary>
        /// <param name="storageIndex">The storage connection string index.</param>
        /// <param name="fileName">The storage blob filename.</param>
        /// <param name="mimeType">The expected mime content type.</param>
        /// <returns>The file data with inline disposition header.</returns>
        [HttpGet("preview/{storageIndex}/{fileName}/{mimeType}")]
        public async Task<FileContentResult> GetDocumentInline(string fileName, string mimeType, CancellationToken cancellationToken)
        {
            var decodedFilename = WebUtility.UrlDecode(fileName);
            var container = GetBlobContainerClient();
            var blob = container.GetBlobClient(decodedFilename);
            using (var ms = new MemoryStream())
            {
                var downlaodInfo = await blob.DownloadAsync(cancellationToken);
                await downlaodInfo.Value.Content.CopyToAsync(ms, cancellationToken);
                Response.Headers.Add("Content-Disposition", "inline; filename=" + decodedFilename);
                return File(ms.ToArray(), WebUtility.UrlDecode(mimeType));
            }
        }

        private BlobContainerClient GetBlobContainerClient()
        {
            return _blobServiceClient.GetBlobContainerClient(_storageOptions.ContainerName);
        }
    }
}
