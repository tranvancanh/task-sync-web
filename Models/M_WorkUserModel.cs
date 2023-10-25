namespace task_sync_web.Models
{
    public class M_WorkUserModel
    {
        public int WorkUserID { get; set; }
        public string WorkUserCode { get; set; }
        public string WorkUserPassword { get; set; }
        public int WorkUserCategoryID { get; set; }
        public string Salt { get; set; }
        public string WorkUserName { get; set; }
        public string WorkUserNameKana { get; set; }
        public int DepoID { get; set; }
        public bool PasswordMode { get; set; }
        public bool AdministratorFlag { get; set; }
        public bool NotUseFlag { get; set; }
    }
}
