namespace task_sync_web.Models
{
    public class LoginUserModel
    {
        public string CompanyId {  get; set; }
        public string CompanyName {  get; set; }
        public string CompanyDatabaseName {  get; set; }
        public int AdministratorId {  get; set; }
        public string AdministratorLoginId {  get; set; }
        public string AdministratorName {  get; set; }
        public DateTime LoginDateTime { get; set; }

    }
}
