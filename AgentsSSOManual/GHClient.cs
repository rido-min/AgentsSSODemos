using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AgentsSSOManual
{

    public class PR
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    public class PRList
    {
        IList<PR> prs;
    }

    public class GHClient
    {
        public static async Task<string> GetPRs(string accessToken)
        {
            string displayName = "unknown";
            string ghpulls = $"https://api.github.com/repos/microsoft/agents/pulls";
            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.GetAsync(ghpulls);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    PRList prlist = JsonSerializer.Deserialize<PRList>(content)!;
                    //var graphResponse = JsonNode.Parse(content);
                    //displayName = graphResponse!["displayName"]!.GetValue<string>();
                    displayName = string.Join(' ', prlist);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Error getting display name: {ex.Message}");
            }
            return displayName;
        }
    }
}
