using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.ML.Runtime;
using NAudio;
using OllamaSharp;
using OllamaSharp.Models;
using System;
using System.Runtime.CompilerServices;
using System.Speech.Recognition;
using System.Threading.Tasks;
using System.IO;

//using A2A;

namespace Agent_Llama;

#pragma warning disable CA1416 // Avoid constant arrays as arguments

public class NestEd : IOllamaBase
{

    public string txt { get; set; }

    public NestEd(string modelEndpoint, string modelName)
    {
        ModelEndpoint = new Uri(modelEndpoint);
        ModelName = modelName;
    }

    public NestEd(Uri modelEndpoint, string modelName)
    {
        ModelEndpoint = modelEndpoint;
        ModelName = modelName;
    }

    public Uri ModelEndpoint { get; set; }
    public string ModelName { get; set; }

    static void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
        Console.WriteLine("Recognized text: " + e.Result.Text);
        //txt += e.Result.Text ?? "";
    }


    public async Task CreateAgent(string input1, string input2)
    {
        
        var http = new System.Net.Http.HttpClient();
        http.Timeout = TimeSpan.FromSeconds(1000);

        var mname = "gemma3:4b";

        var chatClient = new OllamaChatClient(
            ModelEndpoint,
            mname, 
            http
        );

        // 1. Initialize the client
        var uri = new Uri("http://localhost:11434");
        var ollamaApiClient = new OllamaApiClient(ModelEndpoint, mname);
        //ollama.SelectedModel = ModelName;
        var chat = new Chat(ollamaApiClient);

        
        var songfilepath = @"C:\Users\risto\OneDrive\Documents\Hello.wav";
        //var songfilepath = @"C:\Users\risto\Music\bee-gees-until.mp3";
               
        

        string imagePath1 = @"C:\Users\risto\OneDrive\Documents\henri.jpg";
        string imagePath2 = @"C:\Users\risto\OneDrive\Documents\who.jpg";
        byte[] imageBytes1 = File.ReadAllBytes(imagePath1);
        byte[] imageBytes2 = File.ReadAllBytes(imagePath2);

        //Uri uriphoto = new Uri("https://photos.google.com/photo/AF1QipPmeaXZ9Mxy4mK76sZZEauNCyluaJLM4STvNXZO");
        // uriphoto.AbsoluteUri

        // 3. Create the prompt with the image content
        var prompt = new ChatMessage(ChatRole.User, "Here is image of Henri:" + imagePath1 + " is the same person in this image too:" + imagePath2);
        prompt.Contents.Add(new DataContent(imageBytes1, "image/jpeg"));
        prompt.Contents.Add(new DataContent(imageBytes2, "image/jpeg"));

        // 4. Get the analysis from the model
        var response = await chatClient.GetResponseAsync(new[] { prompt });
        Console.WriteLine($"Analysis: {response.Messages[0].Text}");
        
         
        
        string mp3FilePath = songfilepath;
        if(System.IO.File.Exists(songfilepath))
        {

            using (
                SpeechRecognitionEngine recognizer =
                    new SpeechRecognitionEngine(
            new System.Globalization.CultureInfo("en-US")))
            {

                // Create and load a dictation grammar.
                recognizer.LoadGrammar(new DictationGrammar());

                // Add a handler for the speech recognized event.
                recognizer.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

                // Configure input to the speech recognizer.
                recognizer.SetInputToDefaultAudioDevice();

                // Start asynchronous, continuous speech recognition.
                recognizer.RecognizeAsync(RecognizeMode.Multiple);

                // Keep the console window open.
                //while (true)
                //{
               //     Console.ReadLine();
               // }
            }

/*
            // Initialize the recognition engine
            using (SpeechRecognitionEngine recognizer = new())
            {
                // Load a basic dictation grammar
                recognizer.LoadGrammar(new DictationGrammar());

                // Point the recognizer to your WAV file
                recognizer.SetInputToWaveFile(songfilepath);

                // Handle the speech recognized event
                recognizer.SpeechRecognized += (s, e) =>
                {
                    Console.WriteLine("Transcribed Text: " + e.Result.Text);
                    txt += e.Result.Text;
                };

                // Start recognition
                recognizer.Recognize();
            }

*/


            using (var audioFile = new NAudio.Wave.WaveFileReader(mp3FilePath))
            {
                // 4. Extract Audio Data (Simple Example - Using NAudio's WaveFormatReader)
                var waveFormat = audioFile.WaveFormat;
                byte[] audioBuffer = new byte[audioFile.Length];
                audioFile?.Read(audioBuffer); //, 0, audioBuffer.Length);

                // 5. Send Audio Data to Ollama for Analysis
                string prompt2 = $"Analyze this audio for what is said. Audio Data: {Convert.ToBase64String(audioBuffer)}";

                await foreach (var token in chat.SendAsync(prompt2, [audioBuffer]))
                    Console.Write(token);

                //var response = await ollamaApiClient.ChatAsync?(prompt);
                //CreateCompletion

                // 6. Process Response
                //Console.WriteLine($"Ollama Response: {response.Choices[0].Text}");
            }
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();

        /*

            var audioBytes = await System.IO.File.ReadAllBytesAsync("song_sample.wav");
            var audioBase64 = Convert.ToBase64String(audioBytes);

            var prompt = "Analyze the following audio and describe its genre, mood, and notable features.";

            var response = await ollama.GenerateAsync(new GenerateRequest
            {
                Model = "mistral-7b-audio-v1.5",
                Prompt = prompt,
                Images = null, // Not needed here
                Audio = audioBase64 // Custom field if supported by your Ollama build
            });

            Console.WriteLine("Music Analysis:");
            Console.WriteLine(response.Response);

        */

        /*
            var parser = new M3UParser();
            var songs = parser.Parse("my_playlist.m3u");

            foreach (var song in songs)
            {
                Console.WriteLine($"{song.Title} by {song.Artist} lasts [{song.DurationInSeconds}s]");
                // Example logic: Check if the file exists locally
                //if (!File.Exists(song.Path))
                //{
                //    Console.WriteLine($"--> Warning: File not found at {song.Path}");
                //}
            }
        */


    }
}
