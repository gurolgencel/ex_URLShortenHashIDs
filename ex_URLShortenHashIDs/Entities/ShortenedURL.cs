using System.ComponentModel.DataAnnotations.Schema;

namespace ex_URLShortenHashIDs.Entities
{
    public class ShortenedURL
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string URL { get; set; } =string.Empty;
        public DateTime CreatedOnUTC { get; set; }
    }
}
