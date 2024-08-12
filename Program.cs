//*****************************************************************************
//** Image Downloader                                                        **
//** A simple program to download images from a website.              -Dan   **
//*****************************************************************************


using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ImageDownloader
{
    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter the URL of the website to download images from:");
            string url = Console.ReadLine();

            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine("Invalid URL. Please try again.");
                return;
            }

            Console.WriteLine($"Downloading images from {url}...");

            // Create a directory to store downloaded images
            string directoryName = "DownloadedImages";
            Directory.CreateDirectory(directoryName);

            var imageUrls = await GetImageUrls(url);

            if (imageUrls.Count > 0)
            {
                foreach (var imageUrl in imageUrls)
                {
                    await DownloadImage(imageUrl, directoryName);
                }
                Console.WriteLine("Image download completed.");
            }
            else
            {
                Console.WriteLine("No images found.");
            }
        }

        private static async Task<List<string>> GetImageUrls(string url)
        {
            var imageUrls = new List<string>();
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string htmlContent = await response.Content.ReadAsStringAsync();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                // Extract all img tags
                foreach (var img in htmlDocument.DocumentNode.SelectNodes("//img[@src]"))
                {
                    string src = img.GetAttributeValue("src", string.Empty);
                    if (!string.IsNullOrEmpty(src))
                    {
                        imageUrls.Add(src);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching images: {ex.Message}");
            }
            return imageUrls;
        }

        private static async Task DownloadImage(string imageUrl, string directory)
        {
            try
            {
                // Handle relative URLs
                if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                {
                    // Create a new URI based on the base URL
                    Uri baseUri = new Uri("http://example.com"); // Replace with the actual base URL
                    Uri fullUri = new Uri(baseUri, imageUrl);
                    imageUrl = fullUri.ToString();
                }

                var imageResponse = await httpClient.GetAsync(imageUrl);
                imageResponse.EnsureSuccessStatusCode();

                // Extract the filename from the URL
                string fileName = Path.GetFileName(imageUrl);
                string filePath = Path.Combine(directory, fileName);

                // Save the image to the local directory
                using (var stream = await imageResponse.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fileStream);
                }

                Console.WriteLine($"Downloaded: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image: {imageUrl} - {ex.Message}");
            }
        }
    }
}