using SecureDailyJournal.ViewModels;

namespace SecureDailyJournal.Views
{
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage(HistoryViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
