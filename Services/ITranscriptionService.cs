using AudioTranscriptionAPI.Models;

namespace AudioTranscriptionAPI.Services
{
    /// <summary>
    /// 🎯 Service contract for audio transcription functionality
    /// </summary>
    public interface ITranscriptionService
    {
        /// <summary>
        /// Transcribe audio from uploaded file
        /// </summary>
        Task<TranscriptionResult> TranscribeAudio(IFormFile audioFile);

        /// <summary>
        /// Transcribe audio from byte array
        /// </summary>
        Task<TranscriptionResult> TranscribeAudio(byte[] audioData);

        /// <summary>
        /// 🔗 Process video URL and extract audio for transcription
        /// </summary>
        Task<TranscriptionResult> TranscribeFromUrl(string videoUrl);
    }
}
