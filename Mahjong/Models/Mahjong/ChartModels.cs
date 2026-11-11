using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mahjong.Models.Mahjong
{
    public class SplinePointDto
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class GameSplineSeriesDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("pointStart")]
        public int PointStart { get; set; }

        [JsonPropertyName("data")]
        public List<SplinePointDto> Data { get; set; } = [];
    }

    public class LeaderBoardSplineSeriesDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("pointStart")]
        public int PointStart { get; set; }

        [JsonPropertyName("data")]
        public List<double> Data { get; set; } = [];
    }

    public class DependencyWheelPointDto
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("to")]
        public string To { get; set; } = string.Empty;

        [JsonPropertyName("weight")]
        public double Weight { get; set; }
    }
}
