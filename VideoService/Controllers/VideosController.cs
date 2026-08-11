using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VideoService.Application.Services;
using VideoService.DTOs;

namespace VideoService.Controllers
{
    // Controllers/VideosController.cs
    [ApiController]
    [Route("api/videos")]
    [Authorize]
    public class VideosController : ControllerBase
    {
        private readonly RequestVideoUploadHandler _requestUploadHandler;
        private readonly GetVideoStreamUrlHandler _getStreamUrlHandler;

        public VideosController(
            RequestVideoUploadHandler requestUploadHandler,
            GetVideoStreamUrlHandler getStreamUrlHandler)
        {
            _requestUploadHandler = requestUploadHandler;
            _getStreamUrlHandler = getStreamUrlHandler;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<RequestUploadResponse>> RequestUpload(
            [FromBody] RequestUploadRequest request,
            CancellationToken cancellationToken)
        {
            var ownerId = GetOwnerId();
            var result = await _requestUploadHandler.HandleAsync(ownerId, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{videoId:guid}/stream")]
        public async Task<ActionResult<StreamVideoResponse>> GetStreamUrl(
            Guid videoId,
            CancellationToken cancellationToken)
        {
            var result = await _getStreamUrlHandler.HandleAsync(videoId, cancellationToken);
            return Ok(result);
        }

        private Guid GetOwnerId()
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("Missing user identifier claim");
            return Guid.Parse(sub);
        }
    }
}
