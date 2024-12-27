namespace distant.ViewModels
{
    public class UserRoleViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public IList<string> Roles { get; set; }
        public string NewRole { get; set; } // Для выбора новой роли
        public int? GroupId { get; set; } // Для выбора группы
    }
}
