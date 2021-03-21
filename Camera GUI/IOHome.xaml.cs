using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JPLC;
using System.Reactive.Linq;
using System.IO;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace Camera_GUI
{
	/// <summary>
	/// Interaction logic for IOHome.xaml
	/// </summary>
	public partial class IOHome : Page
	{
		public bool flash_state;
		public bool cool_state;
		public double angle_x;
		public double angle_y;
		public double pan_reading;
		public double temperature;
		public double battery_level;
		readonly string LEDOff = "LED is off";
		readonly string LEDOn = "LED is on";
		public float x_set_box;
		public float y_set_box;
		public float pan_set_box;
		string flash_text;
		short flash_number;

		float old_delta;
		float new_delta;

		private void FlashButton_Checked(object sender, RoutedEventArgs e)
		{
			if (FlashButton.IsChecked == true)
            {
				flash_text = "Flash: ON";
				flash_state = true;
			}
            else
            {
				flash_text = "Flash: OFF";
				flash_state = false;
			}
			FlashButton.Content = flash_text;
		}

		private void EscapePressed(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				Keyboard.ClearFocus();
			}
		}
		private void XReturnPressed(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				XSetBox.Text = XSetField.Text;
				XSetField.Text = "";
			}
		}
		private void YReturnPressed(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				YSetBox.Text = YSetField.Text;
				YSetField.Text = "";
			}
		}
		private void PanReturnPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
			if (e.Key == Key.Return)
            {
				PanSetBox.Text = PanSetField.Text;
				PanSetField.Text = "";
            }
		}

		public IOHome()
		{
			// Connecting to PLC
			ConnectToPLC();

			// Start GUI
			InitializeComponent();
		}

		public void UpdateGUI(ChiefDatabase CB)
        {
			Dispatcher.Invoke(delegate
			{
				if (flash_state is false)
				{
					LEDIndicator.Text = LEDOff;
					LEDIndicator.Background = new SolidColorBrush(Color.FromRgb(86, 139, 179));
					LEDIndicator.Foreground = new SolidColorBrush(Color.FromRgb(255, 242, 213));
				}
				else
				{
					LEDIndicator.Text = LEDOn;
					LEDIndicator.Background = new SolidColorBrush(Color.FromRgb(173, 220, 255));
					LEDIndicator.Foreground = new SolidColorBrush(Color.FromRgb(179, 142, 86));
				}
				if (cool_state is true)
				{
					CameraTempStatus.Foreground = Brushes.PowderBlue;
					string cooling = "Cooling...";
					CameraTempStatus.Text = cooling;
				}
				else
				{
					CameraTempStatus.Foreground = Brushes.Moccasin;
					string not_cooling = "Not Cooling";
					CameraTempStatus.Text = not_cooling;
				}

				AngleXText.Text = angle_x.ToString() + " degrees";
				AngleYText.Text = angle_y.ToString() + " degrees";
				PanReadBox.Text = pan_reading.ToString() + " degrees";
				CameraTempBox.Text = temperature.ToString() + " degrees";
				FlashBox.Text = "Flashes: " + flash_number;
				BatteryChargeBox.Text = battery_level.ToString() + "%";
				BatteryChargeBox.Width = Math.Round(battery_level*4.75, 0);

				float.TryParse(XSetBox.Text, out x_set_box);
				float.TryParse(YSetBox.Text, out y_set_box);
				float.TryParse(PanSetBox.Text, out pan_set_box);
			});
		}

		public void ConnectToPLC()
        {
			// Connecting to PLC
			JPLCConnection plc = new JPLCConnection("192.168.1.140", 0, 1);
			double timeBetweenRetries = 0.06; // seconds
			double timeBetweenReads = 0.05; //seconds

			// Setup program
			var chiefDB = new ChiefDatabase();
			var acclinDBNum = 4;
			plc.Connect();
			chiefDB.ReadFromDB(plc, acclinDBNum);

			// READ PLC
			var readDBObservable = Observable.Create<ChiefDatabase>(o =>
			{
				var result = chiefDB.ReadFromDB(plc, acclinDBNum);
				if (result != 0)
				{
					o.OnError(new Exception("Could not read from DB"));
					Console.WriteLine("Read failure");
				}
				else
				{
					o.OnNext(chiefDB);
					o.OnCompleted();
				}
				return Disposable.Empty;
			});

			// ESTABLISH CONNECTION
			var observable = Observable.Create<ChiefDatabase>(o =>
			{
				Console.WriteLine($"Attempting to connect to PLC on ip={plc.IPAddress}, rack={plc.Rack}, slot={plc.Slot}");
				if (plc.Connect() != 0)
				{
					Console.WriteLine("Failed to connect");
					o.OnError(new Exception("Failed to connect to PLC"));
				}
				else
				{
					Console.WriteLine("Successfully Connected");
					o.OnCompleted();
				}
				return Disposable.Empty;
			})
			.Concat(
				Observable
				.Interval(TimeSpan.FromSeconds(timeBetweenReads))
				.Zip(readDBObservable, (interval, dbs) => dbs)
				.Repeat()
			);

			var disposable = observable
				.ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
				.RetryWhen(errors => errors.SelectMany(Observable.Timer(TimeSpan.FromSeconds(timeBetweenRetries))))
				.Subscribe(dbs =>
				{
					var chiefdb = dbs;
					//===============================================================================
					// THIS SECTION OF CODE WILL REPEAT

					Console.WriteLine("\n" + $"Reading from PLC Beam / Fault DataBase on {DateTime.Now}");
					Console.WriteLine(chiefdb.flash_state.Value);

					// Reading
					angle_x = Math.Round(chiefdb.x_out.Value,1);
					angle_y = Math.Round(chiefdb.y_out.Value,1);
					pan_reading = Math.Round(chiefdb.pan_out.Value,1);
					flash_number = chiefdb.flash_count.Value;
					temperature = Math.Round(chiefdb.temperature.Value, 1);
					cool_state = chiefdb.cool_state.Value;
					battery_level = Math.Round(chiefdb.battery_level.Value,0);
					//Writing
					UpdateGUI(chiefdb);
					chiefdb.flash_state.Value = flash_state;
					chiefdb.x_in.Value = x_set_box;
					chiefdb.y_in.Value = y_set_box;
					chiefdb.pan_in.Value = pan_set_box;

					new_delta = x_set_box + y_set_box + pan_set_box + Convert.ToInt32(flash_state);
					if(new_delta != old_delta)
                    {
						chiefDB.WriteToDB(plc, acclinDBNum);
					}
					old_delta = new_delta;
					
					Console.WriteLine("\n" + "---------------------------------------------------------" + "\n");
				});
		}
    }
}
