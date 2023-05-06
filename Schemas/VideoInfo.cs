using YoutubeExplode.Videos;

namespace SkyTube.Schemas;

public record VideoInfo(
  Video Video,
  List<Media> VideoQualities,
  List<Media> AudioQualities
);
