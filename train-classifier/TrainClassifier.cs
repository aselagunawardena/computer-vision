using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;

namespace testClassifier
{
    public static class TestClassifier
    {
        public static async Task Predict(IConfigurationRoot configuration)
        {

            CustomVisionPredictionClient prediction_client;

            try
            {
                // Get Configuration Settings
                string prediction_endpoint = configuration["PredictionEndpoint"];
                string prediction_key = configuration["PredictionKey"];
                Guid project_id = Guid.Parse(configuration["ProjectID"]);
                string model_name = configuration["ModelName"];

                // Authenticate a client for the prediction API
                prediction_client = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(prediction_key))
                {
                    Endpoint = prediction_endpoint
                };

                // Classify test images
                //String[] images = Directory.GetFiles("test-images");

                String[] images = Directory.GetFiles(@"C:\data\src\acemy-Image\test-images");

                 foreach(var image in images)
                {
                    Console.Write(image + ": ");
                    MemoryStream image_data = new MemoryStream(File.ReadAllBytes(image));
                    Console.WriteLine("Reading test images... " + project_id + " - " + model_name);
                    var result = prediction_client.ClassifyImage(project_id, model_name, image_data);
                    Console.WriteLine("Classifying test images...");
                    // Loop over each label prediction and print any with probability > 50%
                    foreach (var prediction in result.Predictions)
                    {
                        if (prediction.Probability > 0.5)
                        {
                            Console.WriteLine($"{prediction.TagName} ({prediction.Probability:P1})");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}