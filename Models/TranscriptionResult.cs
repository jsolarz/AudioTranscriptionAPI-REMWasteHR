namespace AudioTranscriptionAPI.Models;

/// <summary>
/// Result of audio transcription using Azure Speech Services
/// </summary>
public class TranscriptionResult
{
    /// <summary>
    /// The transcribed text from the audio
    /// </summary>
    public string TranscribedText { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score of the transcription (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Duration of the audio in seconds
    /// </summary>
    public double DurationSeconds { get; set; }

    /// <summary>
    /// Detected language of the audio
    /// </summary>
    public string Language { get; set; } = "en-US";

    /// <summary>
    /// Audio format (e.g., wav, mp3, mp4)
    /// </summary>
    public string? AudioFormat { get; set; }

    /// <summary>
    /// Whether the transcription was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message if transcription failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when transcription was completed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
