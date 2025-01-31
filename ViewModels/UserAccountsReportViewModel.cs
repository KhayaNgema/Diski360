namespace MyField.ViewModels
{
    public class UserAccountsReportViewModel
    {
        public int TotalUserAccountsCount { get; set; }

        public int ActiveUserAccountsCount { get; set; }

        public int InactiveUserAccountsCount { get; set; }

        public int SuspendedUserAccountsCount { get; set; }

        public int DeletedUserAccountsCount { get; set; }
    }
}
