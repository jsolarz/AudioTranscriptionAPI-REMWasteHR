namespace AudioTranscriptionAPI.Models
{
    public class AudioDataRequest
    {
        public byte[] AudioData { get; set; } = Array.Empty<byte>();
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }
}
