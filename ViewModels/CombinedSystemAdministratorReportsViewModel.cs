namespace MyField.ViewModels
{
    public class CombinedSystemAdministratorReportsViewModel
    {
        public TransactionsReportsViewModel TransactionsReportsViewModel { get; set; }

        public SystemPerformanceReportViewModel SystemPerformanceReportViewModel { get; set; }

        public UserAccountsReportViewModel UserAccountsReportViewModel { get; set; }

        public OnboardingRequestReportViewModel OnboardingRequestReportViewModel { get;  set; }
    }
}
