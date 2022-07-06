// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using KnowledgeMining.Functions.Skills.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace KnowledgeMining.Functions.Skills.Distinct
{
    public static class Distinct
    {
        public const string FunctionName = "distinct";

        [FunctionName(FunctionName)]
        [StorageAccount("SynonymsStorage")]
        public static async ValueTask<IActionResult> RunDistinct(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] WebApiSkillRequest request,
            [Blob("synonyms/thesaurus.json", FileAccess.Read)] string synBlob,
            ILogger logger)
        {
            logger.LogInformation("Distinct Custom Skill: C# HTTP trigger function processed a request.");

            Thesaurus thesaurus;
            try
            {
                thesaurus = new Thesaurus(synBlob);
            }
            catch (ArgumentNullException)
            {
                return new UnprocessableEntityObjectResult($"Failed to read and parse thesaurus.json");
            }

            return new OkObjectResult(ProcessRequestRecords(request, thesaurus));
        }

        private static string ProcessRequestRecords(WebApiSkillRequest request, Thesaurus thesaurus)
        {
            var response = new WebApiSkillResponse();

            foreach (var inRecord in request.Values)
            {
                var outRecord = new WebApiResponseRecord() { RecordId = inRecord.RecordId };

                JsonElement? wordsParameter = inRecord.Data.TryGetValue("words", out object? wordsParameterObject) ?
                        wordsParameterObject as JsonElement? : null;
                if (wordsParameter is not null && wordsParameter.Value.ValueKind is JsonValueKind.Array)
                {
                    var words = wordsParameter.Value.Deserialize<string[]>();
                    outRecord.Data["distinct"] = thesaurus.Dedupe(words);
                }
                else
                {
                    outRecord.Errors.Add(new WebApiErrorWarning() { Message = $"{FunctionName} - Error processing the request record: Input data is missing a `words` array of words to de-duplicate." });
                }

                response.Values.Add(outRecord);
            }

            return response.ToString();
        }
    }
}