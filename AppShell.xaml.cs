namespace SecureDailyJournal;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute("entry_editor", typeof(Views.EntryEditorPage));
	}
}
