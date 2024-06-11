namespace Tokenizers.DotNet
{
    public static class HuggingFace
    {
        /// <summary>
        /// <para>Download specific file from the hub.</para>
        /// <para>Example: ("skt/kogpt2-base-v2", "tokenizer.json", "deps")</para>
        /// </summary>
        /// <param name="hubName"></param>
        /// <param name="filePath"></param>
        /// <param name="destinationDirectory"></param>
        /// <returns></returns>
        public static async Task<string> GetFileFromHub(string hubName,
            string filePath, string destinationDirectory = "", bool overwrite = true)
        {
            // Example of hubName: skt/kogpt2-base-v2
            // We need to separate the hubName, delimiter is '/'
            // And then concatenate the path using Path.Combine
            string[] sepPath = hubName.Split('/');
            string directory = Path.Combine(sepPath);
            if (!string.IsNullOrEmpty(destinationDirectory))
            {
                directory = Path.Combine(destinationDirectory, directory);
            }
            string fileFullPath = Path.Combine(directory, filePath);
            // Check if the vocab.txt file exists in the directory
            // Return path if it exists
            if (File.Exists(fileFullPath) && !overwrite)
            {
                return fileFullPath;
            }

            // Download the vocab.txt file from the hub
            // Example url: https://huggingface.co/cl-tohoku/bert-large-japanese/resolve/main/vocab.txt?download=true
            var url = $"https://huggingface.co/{hubName}/resolve/main/{filePath}?download=true";
            // Using HttpClient, save into a path {hubName}/vocab.txt, and return the path
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Cannot download {filePath} from {url}");
            }
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(directory);
            using (var fileStream = File.Create(fileFullPath))
            {
                await response.Content.CopyToAsync(fileStream);
            }
            return fileFullPath;
        }
    }
}
