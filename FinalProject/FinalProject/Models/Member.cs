using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FinalProject.Models
{
    public class Member : IdentityUser<int>
    {
        [Required]
        [StringLength(100)] // Ad və soyad üçün limitsiz ölçü olmasın deyə bazada sərhəd qoyuruq
        public string FullName { get; set; }

        // 💡 QEYD 1: Email, PasswordHash və Role əlaqələri IdentityUser-dən avtomatik gəlir.
        // Onları bura bir daha yazmırıq!

        public bool isActivated { get; set; } = true;

        public bool isDeleted { get; set; } = false;
    }
}