using System.ComponentModel.DataAnnotations;

namespace MissingLinks.Models
{
    public class InputModel
    {
        [Required(ErrorMessage = "I need to know who's supposed to get the move!")]
        public string Pokemon { set; get; }

        [Required(ErrorMessage = "What move are you after?")]
        public string Move { set; get; }
    }
}