// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
function LogSearchAnalytics(docCount = 0) {
    if (docCount != null) {
        var recordedQuery = q;
        if (q == undefined || q == null) {
            var recordedQuery = "*";
        }

        appInsights.trackEvent("Search", {
            SearchServiceName: searchServiceName,
            SearchId: searchId,
            IndexName: indexName,
            QueryTerms: recordedQuery,
            ResultCount: docCount,
            ScoringProfile: scoringProfile
        });
    }
}

function LogClickAnalytics(fileName, index) {
    appInsights.trackEvent("Click", {
        SearchServiceName: searchServiceName,
        SearchId: searchId,
        ClickedDocId: fileName,
        Rank: index
    });
}