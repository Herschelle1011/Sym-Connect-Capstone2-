namespace Sym_Connect__Capstone2_.Helper
{
    public class EvaluationStatusHelper
    {
        public static string GetStatus(DateTime openAt, DateTime closedAt)
        {
            var today = DateTime.Today;
            if (today < openAt.Date) return "Upcoming";
            if (today > closedAt.Date) return "Closed";
            return "Open";
        }
    }
}
