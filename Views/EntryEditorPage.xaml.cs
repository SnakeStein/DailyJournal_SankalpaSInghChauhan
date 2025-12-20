using SecureDailyJournal.ViewModels;

namespace SecureDailyJournal.Views
{
    public partial class EntryEditorPage : ContentPage
    {
        public EntryEditorPage(EntryEditorViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
