using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GamersAddict.Models
{
    public class ContactViewModel
    {
        [Required]
        [Display(Name = "Pseudo / Prénom")]
        public string Pseudo { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Titre du message")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Text { get; set; }
    }
}