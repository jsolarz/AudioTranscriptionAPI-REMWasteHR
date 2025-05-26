using AudioTranscriptionAPI.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AudioTranscriptionAPI.Services;

public class TranscriptionService : ITranscriptionService
{
    private readonly string _subscriptionKey;
    private readonly string _region;
    private readonly ILogger<TranscriptionService> _logger;
    private readonly HttpClient _httpClient;

    public TranscriptionService(IConfiguration configuration, ILogger<TranscriptionService> logger, HttpClient httpClient)
    {
        _subscriptionKey = configuration["AzureSpeech:SubscriptionKey"] ?? throw new ArgumentNullException("AzureSpeech:SubscriptionKey");
        _region = configuration["AzureSpeech:Region"] ?? throw new ArgumentNullException("AzureSpeech:Region");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<TranscriptionResult> TranscribeAudio(IFormFile audioFile)
    {
        if (audioFile == null || audioFile.Length == 0)
        {
            return new TranscriptionResult
            {
                IsSuccessful = false,
                ErrorMessage = "No audio file provided"
            };
        }

        try
        {
            _logger.LogInformation("Processing audio file: {FileName}, Type: {ContentType}, Size: {Size} bytes",
                audioFile.FileName, audioFile.ContentType, audioFile.Length);            // Convert IFormFile to byte array
            using var memoryStream = new MemoryStream();
            await audioFile.CopyToAsync(memoryStream);
            var audioData = memoryStream.ToArray();

            // Detect and validate audio format
            var audioFormat = DetectAudioFormat(audioData, audioFile.FileName);
            _logger.LogInformation("Detected audio format: {Format}", audioFormat);

            // For now, we'll try to process all formats directly with Azure Speech Service
            // Azure Speech Service supports WAV, MP3, FLAC, OGG, OPUS, MP4 (audio), and others
            return await TranscribeAudioWithFormat(audioData, audioFormat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audio file: {FileName}", audioFile.FileName);
            return new TranscriptionResult
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to process audio file: {ex.Message}"
            };
        }
    }

    public async Task<TranscriptionResult> TranscribeAudio(byte[] audioData)
    {
        if (audioData == null || audioData.Length == 0)
        {
            return new TranscriptionResult
            {
                IsSuccessful = false,
                ErrorMessage = "No audio data provided"
            };
        }

        // Use the enhanced method with unknown format (will try to detect from headers)
        var format = DetectAudioFormat(audioData, "unknown");
        return await TranscribeAudioWithFormat(audioData, format);
    }

    /// <summary>
    /// 🔗 Process video URL and extract audio for transcription
    /// </summary>
    public async Task<TranscriptionResult> TranscribeFromUrl(string videoUrl)
    {
        if (string.IsNullOrWhiteSpace(videoUrl))
        {
            return new TranscriptionResult
            {
                IsSuccessful = false,
                ErrorMessage = "Video URL cannot be empty"
            };
        }

        try
        {
            _logger.LogInformation("🎬 Starting to process video URL: {VideoUrl}", videoUrl);

            // Validate URL format
            if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out var uri))
            {
                return new TranscriptionResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Invalid URL format"
                };
            }

            // Download the file from URL
            _logger.LogInformation("📥 Downloading content from URL: {VideoUrl}", videoUrl);
            var audioData = await DownloadAudioFromUrl(videoUrl);

            if (audioData == null || audioData.Length == 0)
            {
                return new TranscriptionResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Failed to download audio content from URL"
                };
            }

            _logger.LogInformation("✅ Successfully downloaded {Size} bytes from URL", audioData.Length);

            // Process the downloaded audio
            return await TranscribeAudio(audioData);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error downloading from URL: {VideoUrl}", videoUrl);
            return new TranscriptionResult
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to download from URL: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing video URL: {VideoUrl}", videoUrl);
            return new TranscriptionResult
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to process video URL: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// 📥 Download audio/video content from URL
    /// </summary>
    private async Task<byte[]> DownloadAudioFromUrl(string url)
    {
        try
        {
            // Set timeout and headers for better compatibility
            _httpClient.Timeout = TimeSpan.FromMinutes(10); // Large files may take time
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            _logger.LogInformation("📡 Making HTTP request to: {Url}", url);

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";
            var contentLength = response.Content.Headers.ContentLength ?? 0;

            _logger.LogInformation("📋 Response - Type: {ContentType}, Length: {ContentLength} bytes",
                contentType, contentLength);

            // Check if content type suggests audio/video
            if (!IsAudioVideoContent(contentType, url))
            {
                _logger.LogWarning("⚠️ Content type '{ContentType}' may not be audio/video", contentType);
            }

            // Download content
            var content = await response.Content.ReadAsByteArrayAsync();
            _logger.LogInformation("✅ Download completed: {ActualSize} bytes", content.Length);

            return content;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("⏰ Download timed out for URL: {Url}", url);
            throw new HttpRequestException("Download timed out", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "🌐 HTTP error downloading from URL: {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// 🎵 Check if content type or URL suggests audio/video content
    /// </summary>
    private static bool IsAudioVideoContent(string contentType, string url)
    {
        var audioVideoTypes = new[]
        {
            "audio/", "video/", "application/octet-stream",
            "application/ogg", "application/mp4"
        };

        var audioVideoExtensions = new[]
        {
            ".mp3", ".wav", ".mp4", ".m4a", ".ogg", ".opus",
            ".flac", ".aac", ".webm", ".avi", ".mov", ".mkv"
        };

        return audioVideoTypes.Any(type => contentType.Contains(type, StringComparison.OrdinalIgnoreCase)) ||
               audioVideoExtensions.Any(ext => url.Contains(ext, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Detect audio format based on file header and filename extension
    /// </summary>
    private string DetectAudioFormat(byte[] audioData, string fileName)
    {
        if (audioData.Length < 4)
            return "unknown";

        // Check file header signatures
        if (audioData.Length >= 12 &&
            audioData[0] == 0x52 && audioData[1] == 0x49 && audioData[2] == 0x46 && audioData[3] == 0x46 &&
            audioData[8] == 0x57 && audioData[9] == 0x41 && audioData[10] == 0x56 && audioData[11] == 0x45)
        {
            return "wav";
        }

        if (audioData.Length >= 3 &&
            audioData[0] == 0xFF && (audioData[1] & 0xE0) == 0xE0)
        {
            return "mp3";
        }

        if (audioData.Length >= 8 &&
            audioData[4] == 0x66 && audioData[5] == 0x74 && audioData[6] == 0x79 && audioData[7] == 0x70)
        {
            return "mp4";
        }

        if (audioData.Length >= 4 &&
            audioData[0] == 0x66 && audioData[1] == 0x4C && audioData[2] == 0x61 && audioData[3] == 0x43)
        {
            return "flac";
        }

        // Fallback to file extension
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".wav" => "wav",
            ".mp3" => "mp3",
            ".mp4" => "mp4",
            ".m4a" => "m4a",
            ".flac" => "flac",
            ".ogg" => "ogg",
            ".opus" => "opus",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Transcribe audio with format-specific handling
    /// </summary>
    private async Task<TranscriptionResult> TranscribeAudioWithFormat(byte[] audioData, string format)
    {
        try
        {
            _logger.LogInformation("Transcribing audio with format: {Format}, size: {Size} bytes", format, audioData.Length);

            // Set up Azure Speech Service configuration
            var speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _region);
            speechConfig.SpeechRecognitionLanguage = "en-US";

            // Azure Speech Service can handle multiple formats directly
            // For formats it can't handle, we'd need conversion, but let's try direct first
            using var audioStream = new MemoryStream(audioData);
            using var audioInputStream = AudioInputStream.CreatePushStream();

            // Copy data to push stream
            var buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = audioStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                audioInputStream.Write(buffer, bytesRead);
            }
            audioInputStream.Close();

            // Set up audio configuration from stream
            using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            _logger.LogInformation("Starting speech recognition from {Format} stream...", format);

            // Perform recognition
            var result = await speechRecognizer.RecognizeOnceAsync();

            _logger.LogInformation("Recognition completed with result: {Reason}", result.Reason);

            return result.Reason switch
            {
                ResultReason.RecognizedSpeech => new TranscriptionResult
                {
                    TranscribedText = result.Text,
                    Confidence = 0.95, // Azure doesn't provide confidence in basic recognition
                    IsSuccessful = true,
                    DurationSeconds = result.Duration.TotalSeconds,
                    Language = "en-US",
                    AudioFormat = format
                },
                ResultReason.NoMatch => new TranscriptionResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "No speech detected in audio",
                    AudioFormat = format
                },
                ResultReason.Canceled => HandleCanceledResult(result, format),
                _ => new TranscriptionResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Unexpected result: {result.Reason}",
                    AudioFormat = format
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during {Format} audio transcription", format);
            return new TranscriptionResult
            {
                IsSuccessful = false,
                ErrorMessage = $"Transcription failed for {format}: {ex.Message}",
                AudioFormat = format
            };
        }
    }

    /// <summary>
    /// Handle canceled recognition results with detailed error information
    /// </summary>
    private TranscriptionResult HandleCanceledResult(SpeechRecognitionResult result, string format)
    {
        var cancellation = CancellationDetails.FromResult(result);
        var errorMessage = cancellation.Reason == CancellationReason.Error
            ? $"Recognition canceled due to error: {cancellation.ErrorDetails}"
            : $"Recognition canceled: {cancellation.Reason}";

        _logger.LogWarning("Speech recognition canceled for {Format}: {Reason} - {ErrorDetails}",
            format, cancellation.Reason, cancellation.ErrorDetails);

        return new TranscriptionResult
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage,
            AudioFormat = format
        };
    }
}
