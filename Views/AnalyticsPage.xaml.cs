using SecureDailyJournal.ViewModels;

namespace SecureDailyJournal.Views
{
    public partial class AnalyticsPage : ContentPage
    {
        public AnalyticsPage(AnalyticsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
