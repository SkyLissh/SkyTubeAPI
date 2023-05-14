using Microsoft.AspNetCore.Mvc;
using YoutubeExplode;

using SkyTube.Schemas;
using YoutubeExplode.Videos.Streams;

namespace SkyTube.Controllers;

[ApiController]
[Route("search")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
  static private double _getVideoSize
  (
    IVideoStreamInfo videoStream, IEnumerable<IAudioStreamInfo> audioStream
  )
  {
    var audio = audioStream
      .Where(a => a.Bitrate.KiloBitsPerSecond <= videoStream.Bitrate.KiloBitsPerSecond)
      .FirstOrDefault() ?? audioStream.Last();

    return Math.Round(
      (videoStream.Size.MegaBytes + audio.Size.MegaBytes), 2
    );
  }

  [HttpGet("video")]
  [ProducesResponseType(typeof(VideoInfo), 200)]
  [ProducesResponseType(typeof(HttpMessage), 400)]
  public async Task<ActionResult<VideoInfo>> GetVideos([FromQuery] string url)
  {
    try
    {
      new Uri(url.ToString());

      var youtube = new YoutubeClient();
      var video = await youtube.Videos.GetAsync(url.ToString());

      var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);

      var audioStreams = streamManifest.GetAudioStreams()
        .Where(s => s.Container == Container.Mp4)
        .DistinctBy(s => s.Bitrate.KiloBitsPerSecond)
        .OrderByDescending(s => s.Bitrate.KiloBitsPerSecond);

      var audioQualities = audioStreams
        .Select(s => new Media(
          size: Math.Round(s.Size.MegaBytes, 2),
          quality: s.Bitrate.ToString(),
          bitrate: Math.Round(s.Bitrate.KiloBitsPerSecond, 2)
        ))
        .ToList();


      var videoQualities = streamManifest
        .GetVideoStreams()
        .Where(s => s.Container == Container.Mp4)
        .DistinctBy(s => s.VideoQuality)
        .Select(s => new Media(
          size: _getVideoSize(s, audioStreams),
          quality: s.VideoQuality.ToString(),
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
