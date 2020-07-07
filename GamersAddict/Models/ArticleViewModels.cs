using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GamersAddict.Models
{
    public class Articles
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Titre")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Description (150 caractères max, 100 minimum, espace compris)")]
        public string Description { get; set; }

        public string AuthorId { get; set; }

        [Required]
        [Display(Name = "Contenu")]
        public string Text { get; set; }

        [Display(Name = "Tags (Phrases et mots-clefs séparer par une virgule, exemple : Xbox, Xbox One, XBX)")]
        public string Tags { get; set; }

        public DateTime Date { get; set; }
        public DateTime? EditDate { get; set; }
        public int Views { get; set; }
        public int PublishState { get; set; }
    }

    public class Comments
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ArticleId { get; set; }
        public int CommentId { get; set; }
        public DateTime Date { get; set; }

        [Display(Name = "Ajouter un commentaire :")]
        public string Text { get; set; }
    }
}