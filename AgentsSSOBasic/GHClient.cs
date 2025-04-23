using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AgentsSSOBasic
{

    public class PR
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("number")]
        public int Number { get; set; } = 0;
    }


    public class GHClient
    {
        public static async Task<string> GetPRs(string accessToken)
        {
            string displayName = "";
            string ghpulls = $"https://api.github.com/repos/microsoft/agents/pulls";
            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
                client.DefaultRequestHeaders.Add("User-Agent", "yo");
                HttpResponseMessage response = await client.GetAsync(ghpulls);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var prlist = JsonSerializer.Deserialize<PR[]>(content)!;
                    foreach (var pr in prlist)
                    {
                        displayName += $"**{pr.Number}** {pr.Title} \r\n";
                    }
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
