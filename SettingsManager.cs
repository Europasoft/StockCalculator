using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockCalculator
{
	public class KeyValue
	{
		public KeyValue() { }
		public KeyValue(string k, string v) { key = k; value = v; }
		public string key;
		public string value;
	}
	class CalculatorSettings 
	{
		public CalculatorSettings(bool error_ = false)
		{
			error = error_;
			getByKey("currentPrice").value = 0.0.ToString();
			getByKey("buyPrice").value = 0.0.ToString();
			getByKey("numShares").value = 0.ToString();
			getByKey("courtagePercentage").value = 0.0.ToString();
			getByKey("courtageMinAmount").value = 0.0.ToString();
		}
		public bool error;

		KeyValue[] settings =
		{
			new KeyValue("currentPrice", "0"), // the current market price
			new KeyValue("buyPrice", "0"), // the price the shares were bought at
			new KeyValue("numShares", "0"), // number of owned shares to use for calculation
			new KeyValue("courtagePercentage", "0"), // courtage cost rate
			new KeyValue("courtageMinAmount", "0"), // minimum courtage cost per trade
		};

		// returns a reference to the setting with the specified key
		public ref KeyValue getByKey(string keyIn)
		{
			for (int i = 0; i < settings.Length; i++)
			{ if (settings[i].key == keyIn) { return ref settings[i]; } }
			throw new Exception("could not find setting by key");
		}
	}

	class SettingsManager
	{
		protected static string settingsFileName = @"settings.ini";
		protected static string storagePath = @"%LocalAppData%\StockCalculator\";
		public CalculatorSettings updateSettingsFromFile() 
		{
			if (!setupStorageDir()) { return new CalculatorSettings(true); }

			List<KeyValue> values = new List<KeyValue>();

			// parse each line in file
			int counter = 0;
			foreach (string line in System.IO.File.ReadLines(settingsFilePathFull()))
			{
				bool ignoreLine = false; 
				KeyValue v = parseIniLine(line, ref ignoreLine);
				// add each key/value pair to values
				if (!ignoreLine) { values.Add(v); }
				counter++;
			}

			if (values.Count() < 1) { return new CalculatorSettings(true); }
			// construct and return an object from the read values
			return makeSettingsFromKeyValues(values);
		}
		protected KeyValue parseIniLine(string line, ref bool ignoreLineOut)
		{
			bool passedKeySection = false;
			List<char> key = new List<char>();
			List<char> val = new List<char>();

			// parse characters in a line
			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];
				// skip whitespace characters
				if (char.IsWhiteSpace(c)) { continue; }
				// do not parse comment lines
				if (c == ';')
				{
					ignoreLineOut = true;
					return new KeyValue();
				}
				// begin adding to the value field instead
				if (c == '=') { passedKeySection = true; continue; }
				// add the char to either field
				if (!passedKeySection) { key.Add(c); }
				else { val.Add(c); }
			}

			if (key.Count() < 1 && val.Count() < 1) { return new KeyValue(); }
			// construct and return a key/value pair object
			return new KeyValue(new string(key.ToArray()), new string(val.ToArray()));
		}

		protected CalculatorSettings makeSettingsFromKeyValues(List<KeyValue> keyValuePairs)
		{
			CalculatorSettings outSettings = new CalculatorSettings(false);
			foreach (var kv in keyValuePairs) 
			{
				// set each field's value by matching keys
				outSettings.getByKey(kv.key).value = kv.value;
			}
			return outSettings;
		}

		protected bool doesSettingsFileExist() { return System.IO.File.Exists(settingsFilePathFull()); }
		protected string settingsFilePathFull() { return storagePath + settingsFileName; }
		// returns true if the directory already exists or was successfully created
		protected bool setupStorageDir() 
		{
			if (System.IO.Directory.Exists(storagePath)) { return true; }
			try { System.IO.DirectoryInfo dirInfo = System.IO.Directory.CreateDirectory(storagePath); } 
			catch { return false; }
			return true;
		}
		
	}

	
}
