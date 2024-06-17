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

// Download openai-community/gpt2 from the hub
hubName = "openai-community/gpt2";
filePath = "tokenizer.json";
fileFullPath = await HuggingFace.GetFileFromHub(hubName, filePath, "deps");

// Create a tokenizer instance
var tokenizer2 = new Tokenizer(vocabPath: fileFullPath);
var tokens2 = new uint[] { 72, 373, 10927, 878, 262, 2814, 11, 290, 1312, 550, 257, 17372, 13 };
var decoded2 = tokenizer2.Decode(tokens2);
Console.WriteLine($"Decoded: {decoded2}");

Console.WriteLine($"Version of Tokenizers.DotNet.runtime.win: {tokenizer2.GetVersion()}");
Console.ReadKey();


