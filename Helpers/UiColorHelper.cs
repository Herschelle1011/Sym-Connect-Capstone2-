namespace Sym_Connect__Capstone2_.Helpers
{
    public static class UiColorHelper
    {
        private static readonly string[] Palette =
     {
            "bg-success-subtle text-success",
            "bg-warning-subtle text-warning",
            "bg-primary-subtle text-primary",
            "bg-info-subtle text-info",
            "bg-danger-subtle text-danger",
            "bg-secondary-subtle text-secondary"
        };

        public static string GetBadgeClass(string key)
        {
            if (string.IsNullOrEmpty(key)) return Palette[0];

            int hash = 0;
            foreach (var c in key)
                hash = hash * 31 + c;

            return Palette[Math.Abs(hash) % Palette.Length];
        }
    }
}
