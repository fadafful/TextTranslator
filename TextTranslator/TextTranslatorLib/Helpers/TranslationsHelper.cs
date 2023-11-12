using TextTranslator.Models;

namespace TextTranslator.TextTranslatorLib.Helpers
{
    public class TranslationsHelper
    {
        private readonly TextTranslatorDBContext _db;

        public TranslationsHelper(TextTranslatorDBContext db)
        {
            _db = db;
        }

        public string TranslatorUrlMapperById(int translatorId)
        {
            Translator? translator = _db.Translators.Find(translatorId);
            string? url = translator != null ? translator.Url : "";
            return url;
        }
    }
}
