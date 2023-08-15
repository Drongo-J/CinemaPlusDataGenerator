using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDataGenerator.Services
{
    public class CloudinaryService
    {
        private static readonly Cloudinary _cloudinary;

        static CloudinaryService()
        {
            Account account = new Account
            {
                Cloud = "dax9yhk8g",
                ApiKey = "881374158784918",
                ApiSecret = "C0qdG2p4fB8Tu4EFk1XjYKjrHyQ",
            };

            _cloudinary = new Cloudinary(account);
        }

        public static async Task<string> UploadImageFromUrlAsync(string imageUrl)
        {
            // Download the image from the provided URL
            using (var webClient = new WebClient())
            {
                byte[] imageData = webClient.DownloadData(imageUrl);

                // Set the upload parameters for images
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(Path.GetFileName(imageUrl), new MemoryStream(imageData))
                };

                // Upload the image to Cloudinary
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                // Return the secure URL of the uploaded image
                return uploadResult.SecureUrl.ToString();
            }
        }
    }
}