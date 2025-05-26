using System.ComponentModel.DataAnnotations;

namespace AudioTranscriptionAPI.Models;

/// <summary>
/// 🎯 Request model for video URL processing
/// </summary>
public class VideoUrlRequest
{
    /// <summary>
    /// The video URL to process for accent analysis
    /// </summary>
    [Required(ErrorMessage = "Video URL is required")]
    [Url(ErrorMessage = "Please provide a valid URL")]
    public string VideoUrl { get; set; } = string.Empty;
}
