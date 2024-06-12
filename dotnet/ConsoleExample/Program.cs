using Tokenizers.DotNet;

// Download skt/kogpt2-base-v2/tokenizer.json from the hub
var hubName = "skt/kogpt2-base-v2";
var filePath = "tokenizer.json";
var fileFullPath = await HuggingFace.GetFileFromHub(hubName, filePath, "deps");
Console.WriteLine($"Downloaded {fileFullPath}");

// Create a tokenizer instance
var tokenizer = new Tokenizer(vocabPath: fileFullPath);
var tokens = new uint[] { 9330, 387, 12857, 9376, 18649, 9098, 7656, 6969, 8084, 1 };
var decoded = tokenizer.Decode(tokens);
Console.WriteLine($"Decoded: {decoded}");

Console.WriteLine($"Version of Tokenizers.DotNet.runtime.win: {tokenizer.GetVersion()}");

