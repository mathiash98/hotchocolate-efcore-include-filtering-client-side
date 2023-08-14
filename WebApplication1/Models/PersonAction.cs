using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApplication1.Models
{
    public class PersonAction : ConfigEntityBaseClass
    {
        public DateTime DateTimeUtc { get; set; }
        public PersonActionTypeDto Action { get; set; }
        public string? Location { get; set; }

        [Required]
        public Person Person { get; set; } = null!;
    }

    public enum PersonActionTypeDto
    {
        [EnumMember(Value = "embark")] Embark,
        [EnumMember(Value = "disembark")] Disembark,
        [EnumMember(Value = "dropoff")] Dropoff,
    }
}
