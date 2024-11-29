using Jellyfin.Api;
using Jellyfin.Plugin;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace JavMetadataPlugin
{
    public class JavMetadataPlugin : BasePlugin
    {
        // Plugin metadata
        public override string Name => "JAV Metadata Plugin";
        public override string Description => "A plugin to retrieve metadata for JAV content.";
        public override string Version => "1.0.0";

        // Constructor
        public JavMetadataPlugin()
        {
            // Initialization code, if needed
        }

        // Initialize the plugin
        public override void Initialize()
        {
            // Register events or any other initialization code here
            // For example, you could register metadata fetch on library scan
        }

        // Fetch metadata for a given video ID from JavLibrary
        public async Task<string> FetchMetadata(string videoId)
        {
            string url = $"https://www.javlibrary.com/en/vl_searchbyid.php?keyword={videoId}";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string html = await client.GetStringAsync(url);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    // Example: Extract video title
                    var titleNode = doc.DocumentNode.SelectSingleNode("//div[@id='video_title']");
                    string title = titleNode?.InnerText.Trim();

                    // Similarly, extract other metadata like actors, release date, studio, etc.
                    // This is an example for extracting the title, you can expand this as needed

                    return title;
                }
                catch (Exception ex)
                {
                    // Handle errors (e.g., network issues, parsing failures)
                    Console.WriteLine($"Error fetching metadata for {videoId}: {ex.Message}");
                    return null;
                }
            }
        }

        // Extract video ID from filename using regular expressions
        public string ExtractVideoIdFromFilename(string filename)
        {
            string pattern = @"(\w{3,5}-\d{3,4})"; // Regex for capturing video IDs like FSET-701
            var match = Regex.Match(filename, pattern);
            return match.Success ? match.Value : null;
        }

        // Apply the fetched metadata to the video in Jellyfin
        public void ApplyMetadataToVideo(string videoId, string title)
        {
            var videoItem = ApiClient.GetItemById(videoId);
            if (videoItem != null)
            {
                videoItem.Name = title;
                ApiClient.UpdateItemMetadata(videoItem); // This method would update the metadata in Jellyfin
            }
        }

        // Main method to be called when metadata is needed for a video
        public async Task ProcessVideoMetadata(string filename)
        {
            string videoId = ExtractVideoIdFromFilename(filename);
            if (videoId != null)
            {
                string metadataTitle = await FetchMetadata(videoId);
                if (metadataTitle != null)
                {
                    ApplyMetadataToVideo(videoId, metadataTitle);
                }
                else
                {
                    Console.WriteLine($"No metadata found for {videoId}");
                }
            }
            else
            {
                Console.WriteLine($"No video ID found in the filename {filename}");
            }
        }
    }
}
