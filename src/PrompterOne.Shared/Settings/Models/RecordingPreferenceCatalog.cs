namespace PrompterOne.Shared.Settings.Models;

public static class RecordingPreferenceCatalog
{
    public static class AudioChannels
    {
        public const string Mono = "Mono";
        public const string Stereo = "Stereo";
    }

    public static class AudioCodecs
    {
        public const string Aac = "AAC";
        public const string Flac = "FLAC";
        public const string Mp3 = "MP3";
        public const string Opus = "Opus";
        public const string PcmUncompressed = "PCM (Uncompressed)";
    }

    public static class Containers
    {
        public const string Mkv = "MKV";
        public const string Mov = "MOV";
        public const string Mp4 = "MP4";
        public const string WebM = "WebM";
    }

    public static class FrameRates
    {
        public const string Fps24 = "24 fps";
        public const string Fps30 = "30 fps";
        public const string Fps60 = "60 fps";
        public const string SameAsSource = "Same as source";
    }

    public static class Resolutions
    {
        public const string FullHd1080 = "1920 × 1080 (FHD)";
        public const string Hd720 = "1280 × 720 (HD)";
        public const string SameAsSource = "Same as source";
        public const string UltraHd2160 = "3840 × 2160 (4K)";
    }

    public static class SampleRates
    {
        public const string Khz44_1 = "44.1 kHz";
        public const string Khz48 = "48 kHz";
        public const string Khz96 = "96 kHz";
    }

    public static class VideoCodecs
    {
        public const string Av1 = "AV1";
        public const string H264Avc = "H.264 (AVC)";
        public const string H265Hevc = "H.265 (HEVC)";
        public const string ProRes422 = "ProRes 422";
        public const string ProRes422Hq = "ProRes 422 HQ";
        public const string Vp9 = "VP9";
    }
}
