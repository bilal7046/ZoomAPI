namespace ZoomAPI.Models
{
   public class Meeting
    {
        public string id { get; set; }
        public string topic { get; set; }
        public string agenda { get; set; }
        public string type { get; set; }
        public int duration { get; set; }
        public string timezone { get; set; }
        public string start_time { get; set; }
        public string join_url { get; set; }
        
    }
    public class Root
    {
        public List<Meeting> meetings { get; set; }
    }
}
