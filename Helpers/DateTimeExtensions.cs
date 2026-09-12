namespace Sym_Connect__Capstone2_.Helpers
{
    public static class DateTimeExtensions
    {
        public static string ToTimeAgo(this DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;

            if (span.TotalMinutes < 1)
                return "just now";

            if (span.TotalHours < 1)
                return $"{(int)span.TotalMinutes} min ago";

            if (span.TotalDays < 1)
                return $"{(int)span.TotalHours} hr ago";

            if (span.TotalDays < 2)
                return "1 day ago";

            return $"{(int)span.TotalDays} days ago";
        }
    }
}