# AI-Image-Tagger
Simple Console Application used to search images on local machine through  conversation with AI Chatbot. This can be further extended to private or public cloud without relying on any third party for handling personal photos.

**Why ?**
Since we have moved to digital images, there are tons of images on your computer/laptop from various devices such as internet, mobile phone, action camera, drone, gopro etc. So in order to search and sort images can be a challenge. There are cloud tools available but they can come at a cost of privacy. So the current functionality works from local without sending image data to the cloud. Further it can be extended to personal or public cloud setup.

**Technologies used :**

1. .Net Core 8 for base application
2. Onnx model for object detection.
3. Mistral 7B running locally through Ollama - for adding more natural tags and also handling user interaction
4. Redis Cloud to store the tags, file path and for easy retrieval

**How to Use :**
1. This project uses onnx model, since the file size is above 100mb, it was not possible to commit it directly.
   Use following link to download latest onnx models : https://github.com/onnx/models/tree/main/validated/vision/classification/resnet/model
2. This project also uses local model. Incase using some web ai model, you can use the link directly it in api file.
Else install Mistral 7B or equivalent using ollama
3. This project also uses Redis Cloud - you can use your own free tier account in order to run the application, please update the configuration in the appsettings file.

**Youtube Demo Video Link**
[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/_ZUtN0Ow_ws/0.jpg)](https://www.youtube.com/watch?v=_ZUtN0Ow_ws)

**Future Improvements :**

1. Add GUI- making web app UI in order to show images
2. Add OCR and face tag support
