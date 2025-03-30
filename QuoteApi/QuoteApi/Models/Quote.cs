using System.Collections.Generic;

namespace QuoteApi.Models
{
    public class Quote
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Author { get; set; }
        public int Likes { get; set; } = 0;

        public List<QuoteTag> QuoteTags { get; set; } = new();
    }
}
