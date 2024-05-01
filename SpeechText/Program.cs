using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using System.Text.RegularExpressions;


TranslateSpeech().Wait();
//await RecognizeSpeechFromMic();0
await TextToSpeech();
Console.ReadKey();
static async Task RecognizeSpeech()
{
    var config = SpeechConfig.FromSubscription("39cf037a3cdb4ae0ac9e4b3d54ac87a2", "eastus");
    using (var recog = new SpeechRecognizer(config))
    {
        Console.WriteLine("speek something.....");
        var result = await recog.RecognizeOnceAsync();
        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            Console.WriteLine(result.Text);
            if (RemoveSpecialCharacters(result.Text).StartsWith("hi jarvis", StringComparison.CurrentCultureIgnoreCase))
            {
                //RecognizeSpeechFromMic().Wait();
            }
        }
    }
}
static string RemoveSpecialCharacters(string text)
{
    // Replace any character that is not a letter, digit, space, underscore, or hyphen with an empty string
    return Regex.Replace(text, @"[^a-zA-Z0-9\s_-]", "");
}
static async Task RecognizeSpeechFromMic()
{
    var config = SpeechConfig.FromSubscription("39cf037a3cdb4ae0ac9e4b3d54ac87a2", "eastus");
    using (var recog = new SpeechRecognizer(config))
    {
        recog.Recognizing += (sender, eventArgs) =>
        {
            Console.WriteLine($"Recognizing: {eventArgs.Result.Text} ");
        };
        var finalStatement = string.Empty;
        recog.Recognized += (sender, eventArgs) =>
        {
            var result = eventArgs.Result;
            finalStatement = result.Text;
            if (result.Reason == ResultReason.RecognizedSpeech)
                Console.WriteLine($"Final Statement: {result.Text} ");
            //To-Do - Find out the best practice to stop
            //if(RemoveSpecialCharacters( finalStatement).Contains("bixby stop", StringComparison.CurrentCultureIgnoreCase))
            //    recog.StopContinuousRecognitionAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        };

        recog.SessionStarted += (sender, eventArgs) =>
        {
            Console.WriteLine("U can start speeking");
        };
        recog.SessionStopped += (sender, eventArgs) =>
        {
            Console.WriteLine("Session ended");
        };

        await recog.StartContinuousRecognitionAsync().ConfigureAwait(false);

        do
        {
            Console.WriteLine("Press Enter to stop");
        }
        while (Console.ReadKey().Key != ConsoleKey.Enter
        );

        await recog.StopContinuousRecognitionAsync().ConfigureAwait(false);
    }
}

static async Task TextToSpeech()
{
    var config = SpeechTranslationConfig.FromSubscription("39cf037a3cdb4ae0ac9e4b3d54ac87a2", "eastus");
    // Set the desired voice (optional)
    var voice = "te-IN-AriaNeural"; // Choose a voice from available options
    config.VoiceName = voice;
    using var synthesizer = new SpeechSynthesizer(config);

    // Text to be synthesized
    string textToSynthesize = "Hello, this is a sample text to be converted to speech.";

    // Synthesize the text
    using var result = await synthesizer.SpeakTextAsync(textToSynthesize);
    //if (result.Reason == ResultReason.SynthesizingAudioCompleted)
    //{
    //    // Save audio to a file (optional)
    //    var audioStream = AudioDataStream.FromResult(result);
    //    await audioStream.SaveToWaveFileAsync("output.wav");
    //    Console.WriteLine("Audio saved to output.wav");
    //}
    //else
    //{
    //    Console.WriteLine($"Speech synthesis failed: {result.Reason}");
    //}

}
static async Task TranslateSpeech()
{
    string fromLanguage = "en-US";
    var config = SpeechTranslationConfig.FromSubscription("39cf037a3cdb4ae0ac9e4b3d54ac87a2", "eastus");
    config.SpeechRecognitionLanguage = fromLanguage;
    config.AddTargetLanguage("de-DE");

    const string frenchVoice = "de-DE";
    config.VoiceName = frenchVoice;

    using (var recog = new TranslationRecognizer(config))
    {
        recog.Recognized += (sender, eventArgs) =>
        {
            var result = eventArgs.Result;
            if (result.Reason == ResultReason.TranslatedSpeech)
                Console.WriteLine($"Final Statement: {result.Text} ");
            foreach (var element in eventArgs.Result.Translations)
            {
                Console.WriteLine($"Translating into '{element.Key}' - '{element.Value}'");
            }
        };
            

        recog.Synthesizing += (sender, eventArgs) => {
            var audio = eventArgs.Result.GetAudio();

            if(audio.Length > 0)
            {
                Console.WriteLine($"Audio size: {audio.Length} ");
                //var waveFormat = new WaveFormat(44100, 16, 2);
                //var _waveOut = new WaveOutEvent();
                //using (var memoryStream = new MemoryStream(audio))
                //{
                //    var provider = new RawSourceWaveStream(memoryStream, waveFormat);

                //    // Initialize the WaveOut and play the audio
                //    _waveOut.Init(provider);
                //    _waveOut.Play();
                //}
                File.WriteAllBytes("myFrenchSpeech.wav", audio);
            }
        };

        recog.SessionStarted += (sender, eventArgs) =>
        {
            Console.WriteLine("U can start speeking");
        };
        recog.SessionStopped += (sender, eventArgs) =>
        {
            Console.WriteLine("Session ended");
        };

        await recog.StartContinuousRecognitionAsync();

        do
        {
            Console.WriteLine("Press Enter to stop");
        }
        while (Console.ReadKey().Key != ConsoleKey.Enter);

        await recog.StopContinuousRecognitionAsync();
    }
}