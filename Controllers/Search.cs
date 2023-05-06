using Microsoft.AspNetCore.Mvc;
using YoutubeExplode;
using YoutubeExplode.Videos;

using SkyTube.Schemas;
using YoutubeExplode.Videos.Streams;

namespace SkyTube.Controllers;

[ApiController]
[Route("search")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
  [HttpGet("video")]
  [ProducesResponseType(typeof(Video), 200)]
  [ProducesResponseType(typeof(HttpMessage), 400)]
  public async Task<ActionResult<VideoInfo>> GetVideos([FromQuery] string url)
  {
    try
    {
      new Uri(url.ToString());

      var youtube = new YoutubeClient();
      var video = await youtube.Videos.GetAsync(url.ToString());

      var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
      var videoQualities = streamManifest
        .GetVideoStreams()
        .Where(s => s.Container == Container.Mp4)
        .DistinctBy(s => s.VideoQuality)
        .Select(s => new Media(
          size: Math.Round(s.Size.MegaBytes, 2),
          quality: s.VideoQuality.ToString(),
          bitrate: Math.Round(s.Bitrate.KiloBitsPerSecond, 2)
        ))
        .ToList();

      var audioQualities = streamManifest
        .GetAudioStreams()
        .DistinctBy(s => s.Bitrate)
        .Select(s => new Media(
          size: Math.Round(s.Size.MegaBytes, 2),
          quality: s.Bitrate.ToString(),
          bitrate: Math.Round(s.Bitrate.KiloBitsPerSecond, 2)
        ))
        .ToList();

      return new VideoInfo(video, videoQualities, audioQualities);
    }
    catch (ArgumentException)
    {
      return BadRequest(new HttpMessage("Invalid YouTube url"));
    }
    catch (UriFormatException)
    {
      return BadRequest(new HttpMessage("Invalid url format"));
    }
  }
}
