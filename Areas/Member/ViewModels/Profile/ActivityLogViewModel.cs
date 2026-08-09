using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Profile
{
    public class ActivityLogViewModel
    {

        public ActivityType Type { get; set; }
        public string Description { get; set; } = string.Empty; // "Check-in at Main GYM - Youth Organization Anniversary 2026"
        public DateTime Timestamp { get; set; }

        public string TimeAgo => GetTimeAgo(Timestamp);

        private static string GetTimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalDays >= 365) return $"{(int)(span.TotalDays / 365)} Year{(span.TotalDays / 365 >= 2 ? "s" : "")} ago";
            if (span.TotalDays >= 30) return $"{(int)(span.TotalDays / 30)} Month{(span.TotalDays / 30 >= 2 ? "s" : "")} ago";
            if (span.TotalDays >= 1) return $"{(int)span.TotalDays} Day{(span.TotalDays >= 2 ? "s" : "")} ago";
            if (span.TotalHours >= 1) return $"{(int)span.TotalHours} Hour{(span.TotalHours >= 2 ? "s" : "")} ago";
            return "Just now";
        }
    }
}
