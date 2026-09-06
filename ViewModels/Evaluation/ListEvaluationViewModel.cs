namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class ListEvaluationViewModel
    {
        public int EvaluationId { get; set; }
        public string EvaluationTitle { get; set; } = string.Empty;

        public string EventTitle { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }
        public DateTime OpenAt { get; set; }
        public DateTime ClosesAt { get; set; }

        public string Status
        {
            get
            {
                var now = DateTime.Now;

                if (now < OpenAt)
                    return "Upcoming";

                if (now > ClosesAt)
                    return "Closed";

                return "Open";
            }
        }

        public string DateInfo
        {
            get
            {
                var now = DateTime.Now;

                if (now < OpenAt)
                {
                    var days = (OpenAt - now).Days;
                    return $"Opens in {days} days · {OpenAt:MMM dd}";
                }

                if (now <= ClosesAt)
                {
                    var days = (ClosesAt - now).Days;
                    return $"{days} days left · closes {ClosesAt:MMM dd}";
                }

                return $"Closed · {ClosesAt:MMM dd}";
            }
        }
    }

    }

