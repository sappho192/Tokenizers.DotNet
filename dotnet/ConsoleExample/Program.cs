using Tokenizers.DotNet;

// Download skt/kogpt2-base-v2/tokenizer.json from the hub
var hubName = "skt/kogpt2-base-v2";
var filePath = "tokenizer.json";
var fileFullPath = await HuggingFace.GetFileFromHub(hubName, filePath, "deps");
Console.WriteLine($"Downloaded {fileFullPath}");

// Create a tokenizer instance
Tokenizer tokenizer;
try
{
    tokenizer = new Tokenizer(vocabPath: fileFullPath);
}
catch (TokenizerException e)
{
	Console.WriteLine(e.Message);
    return;
}
try
{
    var text = "음, 이제 식사도 해볼까요";
    Console.WriteLine($"Input text: {text}");
    var tokens = tokenizer.Encode(text);
    Console.WriteLine($"Encoded: {string.Join(", ", tokens)}");
    var decoded = tokenizer.Decode(tokens);
    Console.WriteLine($"Decoded: {decoded}");
}
catch (TokenizerException e)
{
    Console.WriteLine(e.Message);
    return;
}
Console.WriteLine($"Version of Tokenizers.DotNet.runtime.win: {tokenizer.GetVersion()}");
Console.WriteLine("--------------------------------------------------");

//// Download openai-community/gpt2 from the hub
hubName = "openai-community/gpt2";
filePath = "tokenizer.json";
fileFullPath = await HuggingFace.GetFileFromHub(hubName, filePath, "deps");

// Create a tokenizer instance
Tokenizer tokenizer2;
try
{
    tokenizer2 = new Tokenizer(vocabPath: fileFullPath);
}
catch (TokenizerException e)
{
    Console.WriteLine(e.Message);
    return;
}
try
{
    var text2 = "i was nervous before the exam, and i had a fever.";
    Console.WriteLine($"Input text: {text2}");
    var tokens2 = tokenizer2.Encode(text2);
    Console.WriteLine($"Encoded: {string.Join(", ", tokens2)}");
    var decoded2 = tokenizer2.Decode(tokens2);
    Console.WriteLine($"Decoded: {decoded2}");
}
catch (TokenizerException e)
{
    Console.WriteLine(e.Message);
    return;
}

Console.WriteLine($"Version of Tokenizers.DotNet.runtime.win: {tokenizer2.GetVersion()}");
Console.ReadKey();
