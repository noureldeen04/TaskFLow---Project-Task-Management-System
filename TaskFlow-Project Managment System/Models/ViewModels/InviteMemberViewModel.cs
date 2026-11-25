using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Project_Managment_System.Models.ViewModels
{
    public class InviteMemberViewModel
    {
        public int WorkspaceId { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(250)]
        public string Email { get; set; } = string.Empty;
    }
}