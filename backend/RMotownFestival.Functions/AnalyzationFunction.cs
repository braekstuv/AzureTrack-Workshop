using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RMotownFestival.Functions
{
    public class AnalyzationFunction
    {
        public AnalyzationFunction(ComputerVisionClient visionClient)
        {
            VisionClient = visionClient;
        }

        public static readonly List<VisualFeatureTypes?> Features =
            new List<VisualFeatureTypes?> { VisualFeatureTypes.Adult };
        private ComputerVisionClient VisionClient { get; }

        [FunctionName("AnalyzationFunction")]
        public async Task RunAsync([BlobTrigger("festivalpics/{name}", Connection = "BlobStorageConnection")] byte[] myBlob, string name, ILogger log, Binder binder)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            ImageAnalysis analysis = await VisionClient.AnalyzeImageInStreamAsync(new MemoryStream(myBlob), Features);

            Attribute[] attributes;
            if(analysis.Adult.IsAdultContent || analysis.Adult.IsGoryContent || analysis.Adult.IsRacyContent)
            {
                attributes = new Attribute[]
                {
                    new BlobAttribute($"picsrejected/{name}", FileAccess.Write),
                    new StorageAccountAttribute("BlobStorageConnection")
                };
            }
            else
            {
                attributes = new Attribute[]
                {
                    new BlobAttribute($"festivalpics/{name}", FileAccess.Write),
                    new StorageAccountAttribute("BlobStorageConnection")
                };
            }

            using Stream fileOutputStream = await binder.BindAsync<Stream>(attributes);
            fileOutputStream.Write(myBlob);
        }
    }
}
