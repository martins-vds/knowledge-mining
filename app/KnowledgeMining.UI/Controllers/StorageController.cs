﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using KnowledgeMining.UI.Services.Search;
using Microsoft.Extensions.Logging;

namespace KnowledgeMining.UI.Controllers
{
    public class StorageController : Controller
    {
        private readonly ISearchService _docSearch;
        private readonly ILogger<StorageController> _logger;

        public StorageController(ISearchService searchService, ILogger<StorageController> logger)
        {
            _docSearch = searchService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload()
        {
            if (Request.Form.Files.Any())
            {
                var container = GetStorageContainer(0);

                foreach (var formFile in Request.Form.Files)
                {
                    if (formFile.Length > 0)
                    {
                        var blob = container.GetBlobClient(formFile.FileName);
                        var blobHttpHeader = new BlobHttpHeaders();
                        blobHttpHeader.ContentType = formFile.ContentType;
                        await blob.UploadAsync(formFile.OpenReadStream(), blobHttpHeader);
                    }
                }
            }

            await _docSearch.RunIndexer();

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
        public async Task<FileContentResult> GetDocumentInline(int storageIndex, string fileName, string mimeType)
        {
            var decodedFilename = WebUtility.UrlDecode(fileName);
            var container = GetStorageContainer(storageIndex);
            var blob = container.GetBlobClient(decodedFilename);
            using (var ms = new MemoryStream())
            {
                var downlaodInfo = await blob.DownloadAsync();
                await downlaodInfo.Value.Content.CopyToAsync(ms);
                Response.Headers.Add("Content-Disposition", "inline; filename=" + decodedFilename);
                return File(ms.ToArray(), WebUtility.UrlDecode(mimeType));
            }
        }

        private BlobContainerClient GetStorageContainer(int storageIndex)
        {
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;

            var containerKey = "StorageContainerAddress";
            if (storageIndex > 0)
                containerKey += (storageIndex + 1).ToString();
            var containerAddress = _configuration.GetSection(containerKey)?.Value.ToLower();

            var container = new BlobContainerClient(new Uri(containerAddress), new StorageSharedKeyCredential(accountName, accountKey));
            return container;
        }
    }
}