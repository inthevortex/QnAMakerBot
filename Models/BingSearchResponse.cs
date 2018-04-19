using System;
using System.Collections.Generic;

namespace QnAMaker
{
    public class DeepLink
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Snippet { get; set; }
    }

    public class About
    {
        public string Name { get; set; }
    }

    public class License
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class SnippetAttribution
    {
        public License License { get; set; }
        public string LicenseNotice { get; set; }
    }

    public class Value
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsFamilyFriendly { get; set; }
        public string DisplayUrl { get; set; }
        public string Snippet { get; set; }
        public List<DeepLink> DeepLinks { get; set; }
        public DateTime DateLastCrawled { get; set; }
        public string Language { get; set; }
        public List<About> About { get; set; }
        public SnippetAttribution SnippetAttribution { get; set; }
    }

    public class BingSearchResponse
    {
        public string WebSearchUrl { get; set; }
        public int TotalEstimatedMatches { get; set; }
        public List<Value> Value { get; set; }
    }

    public class SearchResults
    {
        public SearchResults(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public string Name { get; set; }
        public string Url { get; set; }
    }
}
