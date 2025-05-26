using AudioTranscriptionAPI.Models;
using AudioTranscriptionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AudioTranscriptionAPI.Controllers
{
    /// <summary>
    /// 🎯 Controller for audio transcription and accent analysis
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TranscriptionController : ControllerBase
    {
        private readonly ITranscriptionService _transcriptionService;
        private readonly ILogger<TranscriptionController> _logger;

        public TranscriptionController(
            ITranscriptionService transcriptionService,
            ILogger<TranscriptionController> logger)
        {
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 🎬 Process video URL for accent analysis
        /// </summary>
        [HttpPost("transcribe")]
        public async Task<ActionResult<TranscriptionResult>> PostTranscription([FromBody] VideoUrlRequest request)
        {
            if (request?.VideoUrl is null)
            {
                return BadRequest("Video URL is required.");
            }

            try
            {
                _logger.LogInformation("Processing video URL: {VideoUrl}", request.VideoUrl);
                var transcriptionResult = await _transcriptionService.TranscribeFromUrl(request.VideoUrl);
                return Ok(transcriptionResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing video URL: {VideoUrl}", request.VideoUrl);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error processing video URL", details = ex.Message });
            }
        }

        /// <summary>
        /// 📁 Legacy file upload endpoint (keeping for backward compatibility)
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<TranscriptionResult>> PostTranscriptionFile(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file uploaded.");
            }

            try
            {
                _logger.LogInformation("Processing audio file: {FileName}, size: {Size} bytes", audioFile.FileName, audioFile.Length);
                var transcriptionResult = await _transcriptionService.TranscribeAudio(audioFile);
                return Ok(transcriptionResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audio file");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing audio file");
            }
        }
    }
}
