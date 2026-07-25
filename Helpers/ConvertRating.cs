namespace Sym_Connect__Capstone2_.Helpers
{
    public class ConvertRating
    {
        public static string Convert(string? answer)
        {
            return answer switch
            {
                "1" => "Very Poor",
                "2" => "Poor",
                "3" => "Neutral",
                "4" => "Excellent",
                "5" => "Very Excellent",
                _ => answer ?? ""
            };
        }
    }
}
