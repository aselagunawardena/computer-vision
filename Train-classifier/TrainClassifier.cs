using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
namespace trainClassifier
{
    public static class TrainClassifier
    {
        static CustomVisionTrainingClient? training_client;
        static Project? custom_vision_project;
        public static async Task Train(IConfigurationRoot configuration)
        {
            await Task.Run(() =>
            {
            // Get Configuration Settings
                string training_endpoint = configuration["TrainingEndpoint"] ?? throw new ArgumentNullException("TrainingEndpoint configuration is missing.");
                string training_key = configuration["TrainingKey"] ?? throw new ArgumentNullException("TrainingKey configuration is missing.");
                string projectIdString = configuration["ProjectID"] ?? throw new ArgumentNullException("ProjectID configuration is missing.");
                Guid project_id = Guid.Parse(projectIdString);

                try
                {
                    // Authenticate a client for the training API
                    training_client = new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(training_key))
                    {
                        Endpoint = training_endpoint
                    };

                    // Get the Custom Vision project
                    custom_vision_project = training_client.GetProject(project_id);

                    // Upload and tag images
                    Upload_Images("more-training-images");
                    
                    // Retrain the model
                    Train_Model();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            });
        }

        
        static void Upload_Images(string root_folder)
        {
            Console.WriteLine("Uploading images...");
            if (training_client == null || custom_vision_project == null)
            {
                throw new InvalidOperationException("Training client or custom vision project is not initialized.");
            }
            IList<Tag> tags = training_client.GetTags(custom_vision_project.Id);
            foreach(var tag in tags)
            {
                Console.Write(tag.Name);
                String[] images = Directory.GetFiles(Path.Combine(root_folder, tag.Name));
                foreach(var image in images)
                {
                    Console.Write(".");
                    using (var stream = new MemoryStream(File.ReadAllBytes(image)))
                    {
                        training_client.CreateImagesFromData(custom_vision_project.Id, stream, new List<Guid>() { tag.Id });
                    }
                }
                Console.WriteLine();

            }
        }

        static void Train_Model()
        {
            // Now there are images with tags start training the project
            Console.Write("Training.");
            if (training_client == null || custom_vision_project == null)
            {
                throw new InvalidOperationException("Training client or custom vision project is not initialized.");
            }
            var iteration = training_client.TrainProject(custom_vision_project.Id);

            // The returned iteration will be in progress, and can be queried periodically to see when it has completed
            while (iteration.Status == "Training")
            {
                Console.Write(".");
                Thread.Sleep(5000);

                // Re-query the iteration to get its updated status
                iteration = training_client.GetIteration(custom_vision_project.Id, iteration.Id);
            }

            Console.WriteLine();
            Console.WriteLine("Model trained");
        }
    }
}