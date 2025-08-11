using StackExchange.Redis;

namespace SmartPhotoTagger.Core
{
    public class RedisService
    {
        private readonly IDatabase db;
        private readonly IServer server;
        public RedisService(string connectionString, string port, string userName, string password)
        {
            var mux = ConnectionMultiplexer.Connect(
            new ConfigurationOptions
            {
                EndPoints = { { connectionString, Convert.ToInt16(port) } },
                User = userName,
                Password = password
            }
            );

            db = mux.GetDatabase();
            server = mux.GetServer(connectionString, Convert.ToInt16(port));
        }

        public async Task<List<string>> GetImagesByTag(string searchTag)
        {

            var matchingPaths = new List<string>();

            // SCAN all keys matching "photo:*"
            foreach (var key in server.Keys(pattern: "photo:*"))
            {
                if (db.KeyType(key) == RedisType.Hash)
                {
                    // Get 'tags' field from hash
                    string tags = db.HashGet(key, "tags");

                    // Split into tokens by whitespace or commas
                    var tagList = tags
                        .Split(new[] { ' ', ',', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    // Check if the exact tag exists (case-insensitive)
                    bool exactMatch = Array.Exists(tagList,
                        t => string.Equals(t.TrimStart('#'), searchTag, StringComparison.OrdinalIgnoreCase));

                    if (exactMatch)
                    {
                        string path = db.HashGet(key, "path");
                        matchingPaths.Add(path);
                    }
                }
            }
            return matchingPaths;
        }

        public async Task StoreImageTagsAsync(string imagePath, List<string> tags)
        {
            // Index key for quick lookups
            string pathIndexKey = $"photo:path:{imagePath}";

            // Check if photo already exists
            var existingPhotoId = await db.StringGetAsync(pathIndexKey);
            if (!existingPhotoId.IsNullOrEmpty)
            {
                Console.WriteLine($"Photo already exists: {imagePath} — skipping.");
                return;
            }

            // Generate a unique ID
            string id = Guid.NewGuid().ToString();
            string photoKey = $"photo:{id}";

            // Store photo hash and path index atomically
            var tran = db.CreateTransaction();
            _ = tran.HashSetAsync(photoKey, new HashEntry[]
            {
                new HashEntry("path", imagePath),
                new HashEntry("tags", string.Join(",", tags))
            });

            // Set the path index so we can quickly check later
            _ = tran.StringSetAsync(pathIndexKey, id);

            bool committed = await tran.ExecuteAsync();
            if (committed)
            {
                Console.WriteLine($"Added new photo: {imagePath}");
            }
        }
    }
}
