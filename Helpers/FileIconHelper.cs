namespace Sym_Connect__Capstone2_.Helpers
{
    public static class FileIconHelper
    {
        public static (string Icon, string ColorClass) GetFileIcon(string? ext)
        {
            return ext?.ToLowerInvariant() switch
            {
                ".pdf" => ("bi-file-earmark-pdf-fill", "file-icon-pdf"),

                ".doc" or ".docx"
                    => ("bi-file-earmark-word-fill", "file-icon-docx"),

                ".xls" or ".xlsx"
                    => ("bi-file-earmark-excel-fill", "file-icon-xlsx"),

                ".ppt" or ".pptx"
                    => ("bi-file-earmark-ppt-fill", "file-icon-ppt"),

                ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp"
                    => ("bi-file-earmark-image-fill", "file-icon-image"),

                _ => ("bi-file-earmark-fill", "file-icon-default")
            };
        }
    }
}
