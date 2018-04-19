using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace QnAMaker
{
    public static class Helper
    {
        public static async Task<string> CallBingSpellCheckAsync(string query)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();
            var uri = ConfigurationManager.AppSettings["BingSpellCheckUrl"] + "mkt=en-US&mode=spell";

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["BingSpellCheckAPIKey"]);

            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("text", query)
            };

            using (FormUrlEncodedContent content = new FormUrlEncodedContent(values))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                response = await client.PostAsync(uri, content);
            }

            var contentString = await response.Content.ReadAsStringAsync();
            var spellCheckResponse = JsonConvert.DeserializeObject<SpellCheckResponse>(contentString);

            if (spellCheckResponse.FlaggedTokens.Count > 0 && spellCheckResponse.CorrectionType == "High")
                foreach (var flaggedToken in spellCheckResponse.FlaggedTokens)
                     query = query.Replace(flaggedToken.Token, flaggedToken.Suggestions.First().Suggestion);

            return query;
        }

        public static async Task<string> CallQnAMakerAsync(string question)
        {
            string data = "{ \"question\": \"" + question + "\" }";
            string result = string.Empty;
            byte[] postBytes = Encoding.UTF8.GetBytes(data);

            var request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["QnAMakerUrl"]);
            request.Method = "POST";
            request.ContentType = "application/json; charset=UTF-8";
            request.Accept = "application/json";
            request.ContentLength = data.Length;
            request.Headers.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["QnAMakerAccessKey"]);

            using (var streamWriter = request.GetRequestStream())
                streamWriter.Write(postBytes, 0, postBytes.Length);

            var response = (HttpWebResponse)(await request.GetResponseAsync());

            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                result = await streamReader.ReadToEndAsync();

            var qnaResponse = JsonConvert.DeserializeObject<QnAResponse>(result);

            return qnaResponse.Answers.First().Answer;
        }

        public static async Task<List<SearchResults>> CallBingSearchAsync(string question)
        {
            string json = string.Empty;
            var uriQuery = ConfigurationManager.AppSettings["BingSearchUrl"] + "?q=" + Uri.EscapeDataString(question);

            var request = (HttpWebRequest)WebRequest.Create(uriQuery);
            request.Headers.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["BingSearchAccessKey"]);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());

            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                json = JObject.Parse(await streamReader.ReadToEndAsync()).SelectToken("webPages").ToString();

            var webPages = JsonConvert.DeserializeObject<BingSearchResponse>(json);
            var pages = webPages.Value.Where(x => x.IsFamilyFriendly).ToList();
            var searchResults = new List<SearchResults>();

            foreach (var page in pages)
                searchResults.Add(new SearchResults(page.Name, page.Url));

            return searchResults;
        }

        public static async Task<bool> CallContentModeratorAsync(string text)
        {
            JObject result = null;
            byte[] postBytes = Encoding.UTF8.GetBytes(text);
            bool moderationRequired = false;

            var request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["ContentModeratorUrl"] + "autocorrect=false&PII=false&classify=true&language=eng");
            request.Method = "POST";
            request.ContentType = "text/plain";
            request.ContentLength = text.Length;
            request.Headers.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["ContentModeratorKey"]);

            using (var streamWriter = request.GetRequestStream())
                streamWriter.Write(postBytes, 0, postBytes.Length);

            var resp = (HttpWebResponse)(await request.GetResponseAsync());

            using (StreamReader streamReader = new StreamReader(resp.GetResponseStream()))
                result = JObject.Parse(await streamReader.ReadToEndAsync());

            var classification = JsonConvert.DeserializeObject<ClassificationObject>(result.SelectToken("Classification").ToString());
            var terms = JsonConvert.DeserializeObject<List<TermObject>>(result.SelectToken("Terms")?.ToString());

            if (classification.ReviewRecommended)
                if (classification.Category1.Score >= 0.8 || classification.Category2.Score >= 0.8 || classification.Category3.Score >= 0.8)
                    moderationRequired = true;

            if (terms != null)
                moderationRequired = true;

            return moderationRequired;
        }
    }
}
