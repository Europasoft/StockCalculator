using System;

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StockCalculator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			// keep fields empty until values are applied
			courtageAmount_txt.Text = "";
			profitAmount_txt.Text = "";
			hideDisclaimerDelayed(2.5f);
		}

		public float currentPrice = 0.0f; // the current market price
		public float buyPrice = 0.0f; // the price the shares were bought at
		public int numShares; // number of owned shares to use for calculation
		public float courtagePercentage = 0.0f; // courtage cost rate
		public float courtageMinAmount = 0.0f; // minimum courtage cost per trade

		public double profitAmountTotal = 0.0f; // unrealized net profit
		public double profitMarginPercentage = 0.0f; // unrealized net profit in percents
		public double courtageCombined = 0.0f; // estimated courtage of both transactions

		private void KeyDownHandler(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && (sender == shares_textBox)) 
			{ Keyboard.ClearFocus(); }
		}

		private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
		{ 
			if (e.ChangedButton == MouseButton.Left) 
			{
				if (sender == closeButton) 
				{ this.Close(); }
				if (sender == titleBar)
				{ this.DragMove(); } // drag window by the custom title bar
			}
		}

		public void titleBar_buttonClick(object sender, RoutedEventArgs e) 
		{ 
			if (sender == closeButton) { Close(); }
			if (sender == minimizeButton) { WindowState = WindowState.Minimized; }
		}

		public void editPrice_action(object sender, RoutedEventArgs e) 
		{ openEditWindow(EditWindowIntent.currentPrice); }

		public void editBuyPrice_action(object sender, RoutedEventArgs e)
		{ openEditWindow(EditWindowIntent.buyPrice); }

		public void editCourtage_action(object sender, RoutedEventArgs e) 
		{ openEditWindow(EditWindowIntent.courtage); }

		public void editMinCourtage_action(object sender, RoutedEventArgs e)
		{ openEditWindow(EditWindowIntent.minCourtage); }

		public async Task editValueCommit(float val, string str, EditWindowIntent Intent) 
		{
			// called by EditWindow once the user clicks apply (use to update input params)
			switch (Intent)
			{
				case EditWindowIntent.currentPrice:
					currentPrice = val;
					price_Txt.Text = str;
					priceUpdateAnim();
					break;
				case EditWindowIntent.buyPrice:
					buyPrice = val;
					buyPrice_Txt.Text = str;
					break;
				case EditWindowIntent.courtage:
					courtagePercentage = val;
					courtage_Txt.Text = str;
					break;
				case EditWindowIntent.minCourtage:
					courtageMinAmount = val;
					minCourtage_Txt.Text = str;
					break;
				default:
					return;
			}
			valueUpdatedEvent(); // run profit analysis
		}

		public void openEditWindow(EditWindowIntent Intent) 
		{
			// open window
			EditWindow editWin = new EditWindow(this, Intent);
			editWin.Show(); // show input window
		}

		async Task priceUpdateAnim() 
		{
			// very simple color change animation
			price_Txt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2877ed"));
			await Task.Delay(500);
			price_Txt.Foreground = new SolidColorBrush(Colors.White);
		}

		private void valueUpdatedEvent(object sender, TextChangedEventArgs args) 
		{
			// bind to text boxes - updates the profit analysis
			if (sender == shares_textBox) 
			{ 
				try { numShares = Int32.Parse(shares_textBox.Text); }
				catch { shares_textBox.Text = "0"; numShares = 0; }
			}
			valueUpdatedEvent(); // actual logic runs in the other overload
		}

		private void valueUpdatedEvent()
		{
			// overload to enable calling anywhere (runs the actual update)
			calculateProfit();
			if (profitAmount_txt != null) 
			{
				string profitAmountStr = toStringLimitDecimals(profitAmountTotal, 3);
				if (profitAmountTotal >= 1000) { profitAmountStr = toStringLimitDecimals(profitAmountTotal / 1000, 2) + "K"; }
				if (profitAmountTotal >= 1000000) { profitAmountStr = toStringLimitDecimals(profitAmountTotal / 1000000, 2).ToString() + "M"; }

				string strOut = profitAmountStr + " (" + toStringLimitDecimals(profitMarginPercentage, 1) + "%)";
				if (numShares <= 0) { strOut = "?"; } // only display output if number of shares is valid
				profitAmount_txt.Text = "Unrealized net profit: " + strOut.Replace(",", "."); // display with American-style point "commas"

				if (courtageAmount_txt != null) 
				{
					string courtageStr = "?";
					if (numShares > 0 && buyPrice > 0 && currentPrice > 0) { courtageStr = toStringLimitDecimals(courtageCombined, 2); }
					courtageAmount_txt.Text = "Combined courtage: " + courtageStr;
				}
			}
		}

		public void calculateProfit() 
		{
			courtageCombined = getCourtageAmount(numShares, buyPrice) + getCourtageAmount(numShares, currentPrice);
			double costTotal = (buyPrice * numShares) + courtageCombined;
			double valueTotal = currentPrice * numShares; // current value of owned shares

			profitAmountTotal = valueTotal - costTotal; // update net profit
			profitMarginPercentage = 100 * (profitAmountTotal / valueTotal); // update net profit percentage
		}

		private double getCourtageAmount(int numShares, float price) 
		{
			// returns the courtage fee for a transaction (courtage rate must be set)
			double c = ((numShares * price) / 100) * courtagePercentage;
			if (c < courtageMinAmount) { c = courtageMinAmount; }
			return c;
		}

		private string toStringLimitDecimals(double input, int decimalPlaces) 
		{
			// returns the provided double as string with specified number of decimal places
			int integralDigitsNum = Math.Truncate(input).ToString().Length;
			int outStrLength = integralDigitsNum + decimalPlaces + 1;
			// safeguard against strange occurrences
			try { return input.ToString().Substring(0, outStrLength); }
			catch (System.ArgumentOutOfRangeException) { return input.ToString(); }
		}

		async Task hideDisclaimerDelayed(float delaySeconds) 
		{
			await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
			dsc_txt.Visibility = Visibility.Collapsed; // hide the bottom disclaimer text
		}
	}
}
