using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UnoTest.Client.Data
{
    public class TestDialogInfo
    {
        public long BusinessId { get; set; }

        public long UserId { get; set; }

        [Range(1, 5)]
        [Display(Name = "Overall Rating")]
        public float OverallRating { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Comments { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Comments2 { get; set; }
    }
}
