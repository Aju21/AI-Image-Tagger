using System.Net.Http.Json;
using System.Text.Json;

namespace SmartPhotoTagger.ConsoleChat
{
    public static class OllamaApi
    {
        public static async Task<string> CallAsync(string prompt)
        {
            var client = new HttpClient();
            var payload = new { model = "mistral", prompt = prompt, stream = false };
            var res = await client.PostAsJsonAsync("http://localhost:11434/api/generate", payload);
            var json = await res.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("response").GetString() ?? "";
        }
    }
}
