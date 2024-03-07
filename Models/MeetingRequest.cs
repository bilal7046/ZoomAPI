using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ZoomAPI.Models
{
    public class MeetingRequest
    {
        [Required]
        [JsonProperty("topic")]
        public string Topic { get; set; }
        [JsonProperty("agenda")]
        public string Agenda { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; } = 2;

        [JsonProperty("start_time")]
        [Required]
        public DateTime StartTime { get; set; }
      
        [JsonProperty("duration")]
        public int Duration { get; set; } = 40;

        [JsonProperty("settings")]
        public Settings Settings { get; set; }
    }
    public class Settings
    {
        [JsonProperty("host_video")]
        public bool HostVideo { get; set; } = false;

        [JsonProperty("participant_video")]
        public bool ParticipantVideo { get; set; } = false;

        [JsonProperty("join_before_host")]
        public bool JoinBeforeHost { get; set; } = false;

        [JsonProperty("mute_upon_entry")]
        public bool MuteUponEntry { get; set; } = false;

        [JsonProperty("watermark")]
        public bool Watermark { get; set; } = false;

        [JsonProperty("audio")]
        public string Audio { get; set; } = "voip";
    }
}
