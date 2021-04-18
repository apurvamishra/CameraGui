﻿using System;
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
		// A large amount of variables, all meant for temporary storage for data being read, to being set in the GUI
		// Some of these could be removed or combined, they're rather inconsistent
		public bool level;
		public bool pan;
		public bool cool_state;
		public double angle_x;
		public double angle_y;
		public double pan_reading;
		public double temperature;
		public double battery_level;
		public float x_set_box;
		public float y_set_box;
		public float pan_set_box;
		string flash_text;
		short flash_number;
		public bool e_stop;
		public bool M1B;
		public bool M1F;
		public bool M2B;
		public bool M2F;

		System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
		float old_data_sum;
		float data_sum;

		// This function is called to alter the LED bar at the top, is currently unused
		private void FlashButton_Checked(object sender, RoutedEventArgs e)
		{
			if (FlashButton.IsChecked == true)
            {
				flash_text = "Flash: ON";
			}
            else
            {
				flash_text = "Flash: OFF";
			}
			FlashButton.Content = flash_text;
		}

		private void LevelStatus_Click(object sender, RoutedEventArgs e)
		{
			level = true;
			
		}

		// This function is somewhat incomplete, removing the focus from GUI elements when the escape key is pressed
		private void EscapePressed(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				Keyboard.ClearFocus();
			}
		}
		// When enter is pressed while a value is written in the "Set X" field, this sets the data in a variable to be sent later
		private void XReturnPressed(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				XSetBox.Text = XSetField.Text;
				XSetField.Text = "";
			}
		}
		// When enter is pressed while a value is written in the "Set Y" field, this sets the data in a variable to be sent later
		private void YReturnPressed(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				YSetBox.Text = YSetField.Text;
				YSetField.Text = "";
			}
		}
		// When enter is pressed while a value is written in the "Set Pan" field, this sets the data in a variable to be sent later
		private void PanReturnPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
			if (e.Key == Key.Return)
            {
				PanSetBox.Text = PanSetField.Text;
				PanSetField.Text = "";
            }
		}

		// The base function, called when iniating the window. First connects to the PLC, then opens the window
		public IOHome()
		{
			// Connecting to PLC
			ConnectToPLC();

			// Start GUI
			InitializeComponent();
		}

		// This command takes the temporary buffer variables and assigns them to GUI elements, refreshing them in their own thread
		public void UpdateGUI(ChiefDatabase CB)
        {
			Dispatcher.Invoke(delegate
			{

				// IF/ELSE statements will update the gui whether the cooling system is on or off
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
				
				// The following lines are copying data from the storage variables, and writing it onto the gui
				AngleXText.Text = angle_x.ToString() + " degrees";
				AngleYText.Text = angle_y.ToString() + " degrees";
				PanReadBox.Text = pan_reading.ToString() + " degrees";
				CameraTempBox.Text = temperature.ToString() + " degrees";
				FlashBox.Text = "Flashes: " + flash_number;
				BatteryChargeBox.Text = battery_level.ToString() + "%";
				BatteryChargeBox.Width = Math.Round(battery_level*4.75, 0);
				e_stop = (bool)STOP.IsChecked;
				M1B = Jog_M1B.IsChecked.Value;
				M1F = Jog_M1F.IsChecked.Value;
				M2B = Jog_M2B.IsChecked.Value;
				M2F = Jog_M2F.IsChecked.Value;

				// In order to get the target angles, the strings in the x, y and pan fields are parsed to floats
				float.TryParse(XSetBox.Text, out x_set_box);
				float.TryParse(YSetBox.Text, out y_set_box);
				float.TryParse(PanSetBox.Text, out pan_set_box);

				Console.WriteLine("Timer: " + t.ToString());
			});
		}

		// This function primarily deals with establishing a connection with the Siemens 1215 PLC
		public void ConnectToPLC()
        {
			// Connecting to PLC
			JPLCConnection plc = new JPLCConnection("192.168.1.140", 0, 1);
			// Upon a failed connection, will wait an extra section of time before trying again
			// For larger databases, this is best to be a larger number
			double timeBetweenRetries = 0.06; // seconds
			// The time between successful reads of the database
			double timeBetweenReads = 0.05; //seconds

			// Setup program, which creates a database object, references the database, and uses a connect Function
			var chiefDB = new ChiefDatabase();
			var acclinDBNum = 4;
			plc.Connect();
			// This function is an inital read from the database
			chiefDB.ReadFromDB(plc, acclinDBNum);

			// -- READ PLC --
			// This is an observable object, which is important when dealing with asynchronous data
			// The observable will feed data back to an observer to be analysed
			var readDBObservable = Observable.Create<ChiefDatabase>(o =>
			{
				// This runs the ReadFromDB function, which updates the local database from the PLC's database
				var result = chiefDB.ReadFromDB(plc, acclinDBNum);
				// The next two statements are error checking, so the local database can read at the right time
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

			// -- ESTABLISH CONNECTION --
			// This observable establishes a connection with the PLC with the given details. 
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
				// The important part of this function is setting up the subscription elements
				// Here, it is organising our read observable so that the database is constantly read
			.Concat(
				Observable
				// The interval between successful executions of the zipped observable
				.Interval(TimeSpan.FromSeconds(timeBetweenReads))
				// Packs the observable up into a pointer location called dbs 
				.Zip(readDBObservable, (interval, dbs) => dbs)
				// Repeats the command ...
				.Repeat()
			);

			// A disposable variable that subscribes a set of commands to a scheduler, which executes every so often
			var disposable = observable
				.ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
				// If errors are detected, the loop will pause for the given timespan
				.RetryWhen(errors => errors.SelectMany(Observable.Timer(TimeSpan.FromSeconds(timeBetweenRetries))))
				// Subscribes 
				.Subscribe(dbs =>
				{
					var chiefdb = dbs;
					//===============================================================================
					// THIS SECTION OF CODE WILL REPEAT

					// -- READING --
					// The following lines are taking data from the database object, and copying them to global variables
					angle_x = Math.Round(((double)chiefdb.x_out.Value / 27648 * 90) - 45, 2);
					angle_y = Math.Round(((double)chiefdb.y_out.Value / 27648 * 90) - 45, 2);
					pan_reading = Math.Round(chiefdb.pan_out.Value,1);
					flash_number = chiefdb.flash_count.Value;
					temperature = Math.Round(chiefdb.temperature.Value, 1);
					cool_state = chiefdb.cool_state.Value;
					battery_level = Math.Round(chiefdb.battery_level.Value,0);

					// -- WRITING --
					// First, the GUI will update the gui based off the most recent data, so that the PLC can recieve the newest data
					UpdateGUI(chiefdb);
					// The next lines copy data into  the database 
					chiefdb.level.Value = level;
					chiefdb.pan.Value = pan;
					chiefdb.x_in.Value = (x_set_box + 45) / 90 * 27648;
					chiefdb.y_in.Value = (y_set_box + 45) / 90 * 27648;
					chiefdb.pan_in.Value = (pan_set_box + 45) / 90 * 27648;
					chiefdb.e_stop.Value = e_stop;
					chiefdb.M1B.Value = M1B;
					chiefdb.M1F.Value = M1F;
					chiefdb.M2B.Value = M2B;
					chiefdb.M2F.Value = M2F;

					// delta is a concept that minimises bandwidth across the connection from the PC to the PLC
					// This first line adds up data to an arbitrary number
					data_sum = x_set_box + y_set_box + pan_set_box 
						+ Convert.ToInt32(level) + Convert.ToInt32(pan) + Convert.ToInt32(e_stop)
						+ Convert.ToInt32(M1B) + Convert.ToInt32(M1F) + Convert.ToInt32(M2B) + Convert.ToInt32(M2F);
					// If data_sum has changed since the last measurement, then the given command/s are executed
					if(data_sum != old_data_sum)
                    {
						chiefDB.WriteToDB(plc, acclinDBNum);
					}
					// To ensure the difference between values can be measured, the old value must be saved for future reference
					// This is done after the comparison so that by the time the next comparison happens, the old and new deltas might be different
					old_data_sum = data_sum;

					if (level == true)
					{
						level = false;
					}
				});
		}

        private void X_Down_Click(object sender, RoutedEventArgs e)
        {
			float.TryParse(XSetBox.Text, out x_set_box);
			x_set_box -= (float)0.1;
			XSetBox.Text = x_set_box.ToString("0.00");
		}
        private void X_Up_Click(object sender, RoutedEventArgs e)
        {
			float.TryParse(XSetBox.Text, out x_set_box);
			x_set_box += (float)0.1;
			XSetBox.Text = x_set_box.ToString("0.00");
		}
        private void Y_Down_Click(object sender, RoutedEventArgs e)
        {
			float.TryParse(YSetBox.Text, out y_set_box);
			y_set_box -= (float)0.1;
			YSetBox.Text = y_set_box.ToString("0.00");
		}
        private void Y_Up_Click(object sender, RoutedEventArgs e)
        {
			float.TryParse(YSetBox.Text, out y_set_box);
			y_set_box += (float)0.1;
			YSetBox.Text = y_set_box.ToString("0.00");
		}
		private void Pan_Down_Click(object sender, RoutedEventArgs e)
		{
			float.TryParse(PanSetBox.Text, out pan_set_box);
			pan_set_box -= (float)0.1;
			PanSetBox.Text = pan_set_box.ToString("0.00");
		}
		private void Pan_Up_Click(object sender, RoutedEventArgs e)
        {
			float.TryParse(PanSetBox.Text, out pan_set_box);
			pan_set_box += (float)0.1;
			PanSetBox.Text = pan_set_box.ToString("0.00");
		}
    }
}
