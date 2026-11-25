using System.ComponentModel.DataAnnotations;

namespace TaskFlow_Project_Managment_System.Models.ViewModels
{
    public class AddTeamMemberViewModel
    {
        [Range(1, int.MaxValue)]
        public int WorkspaceId { get; set; }

        [Range(1, int.MaxValue)]
        public int TeamId { get; set; }

        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
    }
}