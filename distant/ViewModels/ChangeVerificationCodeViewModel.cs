using System.ComponentModel.DataAnnotations;

namespace distant.ViewModels
{
    public class ChangeVerificationCodeViewModel
    {
        [Required]
        public string CurrentVerificationCode { get; set; }

        [Required]
        public string NewVerificationCode { get; set; }

        [Required]
        [Compare("NewVerificationCode", ErrorMessage = "Подтверждение нового кода не совпадает.")]
        public string ConfirmNewVerificationCode { get; set; }
    }

}