using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AudioTranscriptionAPI.Models;
using AudioTranscriptionAPI.Services;

namespace AudioTranscriptionAPI.Controllers
{
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

        [HttpPost("transcribe")]
        public async Task<ActionResult<TranscriptionResult>> PostTranscription(IFormFile audioFile)
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
