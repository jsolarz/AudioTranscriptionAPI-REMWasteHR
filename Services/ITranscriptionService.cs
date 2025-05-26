using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AudioTranscriptionAPI.Models;

namespace AudioTranscriptionAPI.Services
{
    public interface ITranscriptionService
    {
        Task<TranscriptionResult> TranscribeAudio(IFormFile audioFile);
        Task<TranscriptionResult> TranscribeAudio(byte[] audioData);
    }
}
