using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GamersAddict.Models
{
    /*** Redaction ***/
    public class ItemsToDoList
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Titre")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public int State { get; set; }
    }

    /** Articles **/
    public class ArticlesViewModel
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int Views { get; set; }
        public int PublishState { get; set; }
    }

        /*** Members ***/
        public class MemberViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime LastLoginDate { get; set; }
    }

    public class MemberRoleViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }
}