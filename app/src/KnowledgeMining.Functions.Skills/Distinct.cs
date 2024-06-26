// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using KnowledgeMining.Functions.Skills.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace KnowledgeMining.Functions.Skills.Distinct
{
    public class Distinct
    {
        public const string FunctionName = "distinct";

        private readonly ILogger<Distinct> _logger;

        public Distinct(ILogger<Distinct> logger)
        {
            _logger = logger;
        }

        [Function(FunctionName)]
        public IActionResult RunDistinct(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [FromBody] WebApiSkillRequest request,
            [BlobInput("synonyms/thesaurus.json", Connection = "SynonymsStorage")] string synBlob)
        {
            _logger.LogInformation("Distinct Custom Skill: C# HTTP trigger function processed a request.");

            var thesaurus = new Thesaurus();
            try
            {
                thesaurus.ParseThesaurus(synBlob);

                return new OkObjectResult(ProcessRequestRecords(request, thesaurus));
            }
            catch (ArgumentNullException)
            {
                return new UnprocessableEntityObjectResult($"Failed to read and parse thesaurus.json");
            }
        }

        private static WebApiSkillResponse ProcessRequestRecords(WebApiSkillRequest request, Thesaurus thesaurus)
        {
            var response = new WebApiSkillResponse();

            foreach (var inRecord in request.Values)
            {
                var outRecord = new WebApiResponseRecord() { RecordId = inRecord!.RecordId ?? string.Empty };

                if (inRecord!.Data is not null)
                {
                    if (inRecord!.Data.TryGetValue("words", out object? wordsParameterObject) && wordsParameterObject is not null)
                    {
                        var wordsArray = wordsParameterObject as JArray;
                        var words = wordsArray?.Values<string>()?.Where(w => w is not null) ?? [];

                        outRecord.Data["distinct"] = thesaurus.Dedupe(words);
                    }
                    else
                    {
                        outRecord.Errors.Add(new WebApiErrorWarning() { Message = $"{FunctionName} - Error processing the request record: Input data is missing a `words` array of words to de-duplicate." });
                    }

                    response.Values.Add(outRecord);
                }
                else
                {
                    outRecord.Errors.Add(new WebApiErrorWarning() { Message = $"{FunctionName} - Error processing the request record: Input data is missing." });
                    response.Values.Add(outRecord);

                }
            }

            return response;
        }
    }
}