using Microsoft.EntityFrameworkCore;

namespace TextTranslator.Models
{
    [Keyless]
    public class TopTranslator
    {
        public int TranslatorId { get; set; }
        public string TranslatorName { get; set; }
        public string TranslatorUrl { get; set; }
        public int UsageCount { get; set; }
    }
}
