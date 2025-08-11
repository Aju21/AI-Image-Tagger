using Microsoft.Extensions.Configuration;
using SmartPhotoTagger.Core;
using System.Diagnostics;

namespace SmartPhotoTagger.ConsoleChat
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

            //Connect to Redis
            string connectionString = config["RedisConnection:ConnectionString"];
            string port = config["RedisConnection:Port"];
            string userName = config["RedisConnection:UserName"];
            string password = config["RedisConnection:Password"];

            var redis = new RedisService(connectionString, port, userName, password);

            Console.WriteLine("=== Smart Photo Tagger ===");
            Console.WriteLine("Note : In case of a new folder, please scan and process images first.");
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Scan images");
            Console.WriteLine("2. Search for images by tags");

            Console.Write("Enter your choice (1 or 2): ");
            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await ProcessImagesAsync(redis);
                    break;

                case "2":
                    await SearchImagesAsync(redis);
                    break;

                default:
                    Console.WriteLine("Invalid choice. Please run again and enter 1 or 2.");
                    break;
            }

        }

        private static async Task ProcessImagesAsync(RedisService redis)
        {
            Console.Clear();
            Console.WriteLine("=== Image Processing Menu ===");
            Console.WriteLine("1. Use default sample images folder");
            Console.WriteLine("2. Specify custom folder");
            Console.WriteLine("3. Return to main menu");
            Console.Write("Select an option (1-3): ");

            var choice = Console.ReadLine();

            string folderPath = string.Empty;
            switch (choice)
            {
                case "1":
                    folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.ImagesPath);
                    break;
                case "2":
                    Console.Write("Enter the full path to the folder: ");
                    folderPath = Console.ReadLine()?.Trim('"');
                    break;
                case "3":
                    return; // go back to main menu
                default:
                    Console.WriteLine("Invalid choice. Press any key to try again...");
                    break;
            }

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"Folder not found: {folderPath}");
                Console.WriteLine("Press any key to try again...");
                Console.ReadKey();
            }

            Console.WriteLine($"Processing images from: {folderPath}");

            var imageFiles = Directory.GetFiles(folderPath, "*.*")
                                      .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                  f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                  f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                  f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                                      .ToList();

            var onnxModelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.ModelPath);
            var sampleLabelsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.LabelPath);

            var detector = new ObjectDetector(onnxModelPath, sampleLabelsPath);
            var tagger = new TagGenerator();

            foreach (var file in imageFiles)
            {
                var obj = detector.DetectMainObject(file);
                var tags = await tagger.GenerateTagsAsync(obj);
                await redis.StoreImageTagsAsync(file, tags.Append(obj).ToList());
            }
            Console.Clear();
            Console.WriteLine("Image processing completed.");
            
            await SearchImagesAsync(redis);
        }


        private static async Task SearchImagesAsync(RedisService redis)
        {
            Console.WriteLine("\nEnter chat queries (e.g., \"show me wine\"):");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                var labelinput = await OllamaApi.CallAsync($"return 5 basic and concise tags separated by commas from the query based on the context without explaination : \"{input}\"");

                var filePathList = new List<string>();
                foreach (var tag in labelinput.Split(","))
                {
                    var imagePaths = await redis.GetImagesByTag(tag.Trim());
                    filePathList.AddRange(imagePaths);
                }


                foreach (var path in filePathList.Distinct())
                {
                    Console.WriteLine($"Found: {path}");
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
            }
        }
    }
}
