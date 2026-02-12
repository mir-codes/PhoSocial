using HtmlSanitizer;

namespace PhoSocial.API.Utilities
{
    public interface ISanitizationService
    {
        string SanitizeHtml(string? input);
    }

    public class HtmlSanitizationService : ISanitizationService
    {
        private readonly HtmlSanitizer _sanitizer;

        public HtmlSanitizationService()
        {
            _sanitizer = new HtmlSanitizer();
            // Configure allowed tags and attributes
            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedAttributes.Clear();
            // Only allow basic text formatting, no scripts
            _sanitizer.AllowedTags.Add("b");
            _sanitizer.AllowedTags.Add("i");
            _sanitizer.AllowedTags.Add("u");
            _sanitizer.AllowedTags.Add("p");
            _sanitizer.AllowedTags.Add("br");
            _sanitizer.AllowedTags.Add("strong");
            _sanitizer.AllowedTags.Add("em");
        }

        public string SanitizeHtml(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return _sanitizer.Sanitize(input);
        }
    }
}
