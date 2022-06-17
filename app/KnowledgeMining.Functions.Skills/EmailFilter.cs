// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KnowledgeMining.Functions.Skills.EmailFilter
{
    public static class EmailFilter
    {

        [FunctionName("emailfilter")]
        public static async Task<IActionResult> RunEmailFilter(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
           ILogger log,
           ExecutionContext executionContext)
        {
            log.LogInformation("EmailFilter Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;


            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) =>
                {
                    var document = inRecord.Data["text"] as string;

                    if (document is null)
                    {
                        throw new ArgumentException("Input data is missing a `text` array of words to de-duplicate.", "text");
                    }


                    outRecord.Data["filtered"] = RemoveEmails(RemoveEmailHeaders(document));
                    return outRecord;
                });

            return new OkObjectResult(response);

        }


        public static string RemoveHtmlTags(string html)
        {
            string htmlRemoved = Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>|<[^>]+>| ", " ").Trim();
            string normalised = Regex.Replace(htmlRemoved, @"\s{2,}", " ");
            return normalised;
        }

        public static string RemoveEmails(string text)
        {
            string normalised = Regex.Replace(text, @"[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+", " ").Trim();
            return normalised;
        }

        public static string RemoveEmailHeaders(string text)
        {
            string aLine, aNextLine, message = null;
            StringReader strReader = new StringReader(text);
            while (true)
            {
                aLine = strReader.ReadLine();
                if (aLine != null)
                {
                    aLine = aLine.Trim();
                    if (aLine.StartsWith("From", StringComparison.OrdinalIgnoreCase) || aLine.StartsWith("Recipients", StringComparison.OrdinalIgnoreCase)
                        || aLine.StartsWith("CC", StringComparison.OrdinalIgnoreCase) || aLine.StartsWith("Bcc", StringComparison.OrdinalIgnoreCase)
                        || aLine.StartsWith("To", StringComparison.OrdinalIgnoreCase))
                    {

                        aNextLine = strReader.ReadLine();
                        if (aNextLine == null)
                            break;

                    }
                    else
                    {
                        message = message + aLine + "\n";

                    }
                }
                else
                {

                    break;
                }
            }

            return message;
        }

    }
}