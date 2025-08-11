namespace SmartPhotoTagger.ConsoleChat
{
    public class TagGenerator
    {
        public async Task<List<string>> GenerateTagsAsync(string label)
        {
            var prompt = $"Suggest 5 simple, accurate and concise tags for an image containing: {label}";
            var response = await OllamaApi.CallAsync(prompt);
            return response.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(t => t.Trim().ToLower()).ToList();
        }
    }

}
