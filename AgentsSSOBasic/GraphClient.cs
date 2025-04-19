using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace AgentsSSO;

public class GraphClient
{
    public static async Task<string> GetDisplayName(string accessToken)
    {
        string displayName = "unknown";
        string graphApiUrl = $"https://graph.microsoft.com/v1.0/me";
        try
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await client.GetAsync(graphApiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var graphResponse = JsonNode.Parse(content);
                displayName = graphResponse!["displayName"]!.GetValue<string>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"Error getting display name: {ex.Message}");
        }
        return displayName;
    }
}
