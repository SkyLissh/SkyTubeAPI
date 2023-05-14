using Microsoft.AspNetCore.Mvc;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;
using Container = YoutubeExplode.Videos.Streams.Container;

using SkyTube.Extensions;
using SkyTube.Schemas;

namespace SkyTube.Controllers;

[ApiController]
[Route("download")]
[Produces("application/json")]
public class DownloadController : ControllerBase
{
  private YoutubeClient _client = new YoutubeClient();

  [HttpGet("audio")]
  [ProducesResponseType(typeof(HttpMessage), 400)]
  [ProducesResponseType(typeof(FileStreamResult), 200)]
  public async Task<ActionResult<FileStream>> DownloadAudio(
    [FromQuery] string url, [FromQuery] double quality
  )
  {
    try
    {
      new Uri(url.ToString());

      var video = await _client.Videos.GetAsync(url);
      var streamManifest = await _client.Videos.Streams.GetManifestAsync(url);

      var audioInfo = streamManifest
        .GetAudioStreams()
        .Where(s => s.Container == Container.Mp4)
        .Where(s => Math.Round(s.Bitrate.KiloBitsPerSecond, 2) == quality)
        .FirstOrDefault();

      if (audioInfo == null) return BadRequest(new HttpMessage("Invalid quality"));

      var audioPath = $"/tmp/{video.Id.Value}.mp3";

      await _client.Videos.DownloadAsync(
        new IStreamInfo[] { audioInfo },
        new ConversionRequestBuilder(audioPath)
          .SetPreset(ConversionPreset.UltraFast)
          .Build()
      );

      var stream = new FileStream(audioPath, FileMode.Open);

      System.IO.File.Delete(audioPath);

      return File(stream, "audio/mpeg", $"{video.Id.Value}.mp3");
    }
    catch (UriFormatException)
    {
      return BadRequest(new HttpMessage("Invalid url format"));
    }
    catch (ArgumentException)
    {
      return BadRequest(new HttpMessage("Invalid YouTube url"));
    }
  }

  [HttpGet("video")]
  [ProducesResponseType(typeof(HttpMessage), 400)]
  [ProducesResponseType(typeof(FileStreamResult), 200)]
  public async Task<IActionResult> DownloadVideo(
    [FromQuery] string url, [FromQuery] string quality
  )
  {
    Quality? qualityVideo = null;

    foreach (Quality q in Enum.GetValues(typeof(Quality)))
    {
      if (q.GetName() == quality)
      {
        qualityVideo = q;
        break;
      }
    }

    if (qualityVideo == null) return BadRequest(new HttpMessage("Invalid quality"));

    try
    {

      new Uri(url.ToString());

      var video = await _client.Videos.GetAsync(url);

      var streamManifest = await _client.Videos.Streams.GetManifestAsync(url);

      var videoInfo = streamManifest
        .GetVideoStreams()
        .Where(s => s.Container == Container.Mp4)
        .Where(s => s.VideoQuality.Label == qualityVideo.GetName())
        .FirstOrDefault();

      var audioInfo = streamManifest
        .GetAudioStreams()
        .Where(s => s.Container == Container.Mp4)
        .OrderByDescending(s => s.Bitrate.KiloBitsPerSecond)
        .Where(
          s => s.Bitrate.KiloBitsPerSecond <= videoInfo!.Bitrate.KiloBitsPerSecond
        )
        .FirstOrDefault();

      if (videoInfo == null || audioInfo == null)
      {
        return BadRequest(new HttpMessage("Invalid quality"));
      }

      var videoPath = $"/tmp/{video.Title}.mp4";

      await _client.Videos.DownloadAsync(
        new IStreamInfo[] { audioInfo, videoInfo! },
        new ConversionRequestBuilder(videoPath).Build()
      );

      var stream = new FileStream(videoPath, FileMode.Open);

      System.IO.File.Delete(videoPath);

      return File(stream, "video/mp4", $"{video.Title}.mp4");
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
