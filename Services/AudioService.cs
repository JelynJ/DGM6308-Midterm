using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace WebAssembly.Services
{
    public class AudioService
    {
        // Private field to store the IJSRuntime instance
        private readonly IJSRuntime _jsRuntime;

        // Constructor that takes an IJSRuntime instance as a parameter
        public AudioService(IJSRuntime jsRuntime)
        {
            // Assign the provided IJSRuntime instance to the private field
            _jsRuntime = jsRuntime;
        }

        // Asynchronous method to play background music
        public async Task PlayBGM(string bgmPath)
        {
            // Invoke the JavaScript function "playBGM" with the provided background music path
            await _jsRuntime.InvokeVoidAsync("playBGM", bgmPath);
        }

        // Asynchronous method to pause the currently playing background music
        public async Task PauseBGM()
        {
            // Invoke the JavaScript function "pauseBGM" to pause the background music
            await _jsRuntime.InvokeVoidAsync("pauseBGM");
        }

        // Asynchronous method to stop the currently playing background music
        public async Task StopBGM()
        {
            // Invoke the JavaScript function "stopBGM" to stop the background music
            await _jsRuntime.InvokeVoidAsync("stopBGM");
        }

        // Asynchronous method to play a sound effect
        public async Task PlaySound(string soundPath)
        {
            // Invoke the JavaScript function "playSound" with the provided sound effect path
            await _jsRuntime.InvokeVoidAsync("playSound", soundPath);
        }
    }
}