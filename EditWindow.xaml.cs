
using System.Windows;


namespace StockCalculator
{
	/// <summary>
	/// Interaction logic for PriceEditWindow.xaml
	/// </summary>
	
	public enum EditWindowIntent { currentPrice, buyPrice, courtage, minCourtage  }

	public partial class EditWindow : Window
	{
		MainWindow MW;
		EditWindowIntent Intent; // set to indicate what value we are editing
		public EditWindow(MainWindow MWIn, EditWindowIntent IntentIn)
		{
			MW = MWIn;
			Intent = IntentIn;
			InitializeComponent();
			Owner = MW; // set the main window as owner so that this window appears in the right location
			updateTitle();
			// focus on the text box
			edit_textBox.Focus();
			edit_textBox.SelectAll();
		}

		// apply button clicked
		async void apply_action(object sender, RoutedEventArgs e) 
		{
			string str = edit_textBox.Text.Replace(".", ","); // TryParse wants "," not "."
			if (float.TryParse(str, out float val)) // convert to float
			{	
				if (val >= 0.0)
				{
					// valid float entered (replace commas in the display string with nicer American-style "commas")
					await MW.editValueCommit(val, str.Replace(",", "."), Intent);
				}
			}
			Close();
		}

		// cancel button clicked
		private void cancel_action(object sender, RoutedEventArgs e) { Close(); }

		// sets the title text according to our set Intent
		private void updateTitle() 
		{
			switch (Intent)
			{
				case EditWindowIntent.currentPrice:
					title_txt.Text = "Set new price";
					break;
				case EditWindowIntent.buyPrice:
					title_txt.Text = "Set acquisition price";
					break;
				case EditWindowIntent.courtage:
					title_txt.Text = "Set courtage rate (percentage)";
					break;
				case EditWindowIntent.minCourtage:
					title_txt.Text = "Set minimum courtage";
					break;
				default:
					return;
			}
		}
	}
}
