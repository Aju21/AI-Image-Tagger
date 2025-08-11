using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Newtonsoft.Json;
using System.Drawing;
public class ObjectDetector
{
    private readonly InferenceSession _session;
    private readonly string[] _labels;

    public ObjectDetector(string modelPath, string labelPath)
    {
        _session = new InferenceSession(modelPath);
        _labels = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(labelPath));
    }

    public string DetectMainObject(string imagePath)
    {
        using var bitmap = new Bitmap(Image.FromFile(imagePath));
        using var resized = new Bitmap(bitmap, new Size(224, 224));
        var input = new DenseTensor<float>(new[] { 1, 3, 224, 224 });

        for (int y = 0; y < 224; y++)
            for (int x = 0; x < 224; x++)
            {
                var c = resized.GetPixel(x, y);
                input[0, 0, y, x] = c.R / 255f;
                input[0, 1, y, x] = c.G / 255f;
                input[0, 2, y, x] = c.B / 255f;
            }

        var results = _session.Run(new[] { NamedOnnxValue.CreateFromTensor("data", input) });
        var output = results.First().AsEnumerable<float>().ToArray();
        var index = Array.IndexOf(output, output.Max());
        return _labels[index];
    }
}
