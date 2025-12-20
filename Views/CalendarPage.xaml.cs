using SecureDailyJournal.ViewModels;

namespace SecureDailyJournal.Views
{
    public partial class CalendarPage : ContentPage
    {
        public CalendarPage(CalendarViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
