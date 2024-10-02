using System.ComponentModel.DataAnnotations;

namespace SecureApiTemplate.Models.Core
{
    public class Base : ICloneable
    {
        [Key]
        public long Id { get; set; }

        public string CreatedSource { get; set; }

        public string UpdatedSource { get; set; }

        public long CreatedBy { get; set; }

        public long UpdatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
