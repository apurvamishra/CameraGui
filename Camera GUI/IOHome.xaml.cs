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
		// A large amount of variables, all meant for temporary storage for data being read, to being set in the GUI
		// Some of these could be removed or combined, they're rather inconsistent
		public bool level;
		public bool level_clicked;
		public bool pan;
		public bool pan_clicked;
		public bool cool_state;
		public bool e_stop;
		public short jog_pitch_in;
		public short jog_yaw_in;
		public short jog_pan_in;
		public short jog_pitch_out;
		public short jog_yaw_out;
		public short jog_pan_out;
		public bool flash_state;
		public bool camera_state;
		public double angle_x;
		public double angle_y;
		public double pan_reading;
		public double temperature;
		public double battery_level;
		public float x_set_box;
		public float y_set_box;
		public float pan_set_box;
		string flash_text;
		string camera_text;
		short flash_number;

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
			float yvalue;
			float.TryParse(YSetBox.Text, out yvalue);
			float xvalue;
			float.TryParse(XSetField.Text, out xvalue);
			if (e.Key == Key.Return)
			{
				if (Math.Abs(2 * (xvalue - 1.8)) + Math.Abs(yvalue + 0.6) < 12)
				{
					XSetBox.Text = XSetField.Text;
					XSetField.Text = "";
				}
			}
		}
		// When enter is pressed while a value is written in the "Set Y" field, this sets the data in a variable to be sent later
		private void YReturnPressed(object sender, System.Windows.Input.KeyEventArgs e)
		{
			float yvalue;
			float.TryParse(YSetField.Text, out yvalue);
			float xvalue;
			float.TryParse(XSetBox.Text, out xvalue);
			if (e.Key == Key.Return)
			{
				if (Math.Abs(2 * (xvalue - 1.8)) + Math.Abs(yvalue + 0.6) < 12)
                {
                    YSetBox.Text = YSetField.Text;
					YSetField.Text = "";
				}
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
				BatteryChargeBox.Width = Math.Round(battery_level / 27648 * (BatteryChargeBox.MaxWidth / 100), 0);
				e_stop = (bool)STOP.IsChecked;
				flash_state = FlashButton.IsChecked.Value;
				camera_state = CameraButton.IsChecked.Value;
				Pitch.Text = "Pitch: " + jog_pitch_out.ToString();
				Yaw.Text = "Yaw: " + jog_yaw_out.ToString();
				Pan.Text = "Pan: " + jog_pan_out.ToString();

				if(level_clicked == true)
                {
					string level_text = "Levelling ...";
					LevelStatus.Content = level_text;
					LevelStatus.Background = new SolidColorBrush(Colors.NavajoWhite);
					LevelStatus.Foreground = new SolidColorBrush(Colors.Purple);
				}
                else
                {
					string level_text = "Level";
					LevelStatus.Content = level_text;
					LevelStatus.Background = new SolidColorBrush(Colors.Purple);
					LevelStatus.Foreground = new SolidColorBrush(Colors.NavajoWhite);
				}

				if (pan_clicked == true)
				{
					string pan_text = "Pan ...";
					PanStatus.Content = pan_text;
					PanStatus.Background = new SolidColorBrush(Colors.NavajoWhite);
					PanStatus.Foreground = new SolidColorBrush(Colors.ForestGreen);
				}
				else
				{
					string pan_text = "Pan";
					PanStatus.Content = pan_text;
					PanStatus.Background = new SolidColorBrush(Colors.ForestGreen);
					PanStatus.Foreground = new SolidColorBrush(Colors.NavajoWhite);
				}

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
					pan_reading = Math.Round(((double)chiefdb.pan_out.Value - 13824) / 13824 * 14, 1);
					flash_number = chiefdb.flash_count.Value;
					temperature = Math.Round(chiefdb.temperature.Value, 1);
					cool_state = chiefdb.cool_state.Value;
					battery_level = Math.Round(chiefdb.battery_level.Value,0);
					level_clicked = chiefdb.level_status.Value;
					pan_clicked = chiefdb.pan_status.Value;

					// -- WRITING --
					// First, the GUI will update the gui based off the most recent data, so that the PLC can recieve the newest data
					UpdateGUI(chiefdb);
					// The next lines copy data into the database 
					chiefdb.level.Value = level;
					chiefdb.pan.Value = pan;
					chiefdb.y_in.Value = (y_set_box + 45) / 90 * 27648;
					chiefdb.x_in.Value = (x_set_box + 45) / 90 * 27648;
					chiefdb.pan_in.Value = (pan_set_box + 45) / 90 * 27648;
					chiefdb.e_stop.Value = e_stop;
					chiefdb.flash_state.Value = flash_state;
					chiefdb.camera_state.Value = camera_state;

					chiefDB.jog_pitch_in.Value = jog_pitch_out;
					chiefDB.jog_yaw_in.Value = jog_yaw_out;
					chiefDB.jog_pan_in.Value = jog_pan_out;
					jog_pitch_in = chiefDB.jog_pitch_out.Value;
					jog_yaw_in = chiefDB.jog_yaw_out.Value;
					jog_pan_in = chiefDB.jog_yaw_out.Value;

					// delta is a concept that minimises bandwidth across the connection from the PC to the PLC
					// This first line adds up data to an arbitrary number
					data_sum = x_set_box + y_set_box + pan_set_box 
						+ Convert.ToInt32(level) + Convert.ToInt32(pan) + Convert.ToInt32(e_stop)
						+ jog_pitch_out + jog_yaw_out + jog_pan_out
						+ Convert.ToInt32(flash_state) + Convert.ToInt32(camera_state);
					// If data_sum has changed since the last measurement, then the given command/s are executed
					if(data_sum != old_data_sum)
                    {
						chiefDB.WriteToDB(plc, acclinDBNum);
					}
					// To ensure the difference between values can be measured, the old value must be saved for future reference
					// This is done after the comparison so that by the time the next comparison happens, the old and new deltas might be different
					old_data_sum = data_sum;
					
					if(level_clicked == false)
                    {
						level = false;
					}
					if (pan_clicked == false)
					{
						pan = false;
					}

					jog_pitch_out = jog_pitch_in;
					jog_yaw_out = jog_yaw_in;
					jog_pan_out = jog_pan_in;
				});
		}

        private void X_Down_Click(object sender, RoutedEventArgs e)
        {
			float yvalue;
			float.TryParse(YSetBox.Text, out yvalue);
			float xvalue;
			float.TryParse(XSetBox.Text, out xvalue);
			if (Math.Abs(2 * (xvalue - 0.1 - 1.8)) + Math.Abs(yvalue + 0.6) < 12)
			{
				float.TryParse(XSetBox.Text, out x_set_box);
				x_set_box -= (float)0.1;
				XSetBox.Text = x_set_box.ToString("0.00");
			}
		}
        private void X_Up_Click(object sender, RoutedEventArgs e)
        {
			float yvalue;
			float.TryParse(YSetBox.Text, out yvalue);
			float xvalue;
			float.TryParse(XSetBox.Text, out xvalue);
			if (Math.Abs(2 * (xvalue + 0.1 - 1.8)) + Math.Abs(yvalue + 0.6) < 12)
			{
				float.TryParse(XSetBox.Text, out x_set_box);
				x_set_box += (float)0.1;
				XSetBox.Text = x_set_box.ToString("0.00");
			}

		}
        private void Y_Down_Click(object sender, RoutedEventArgs e)
        {
			float yvalue;
			float.TryParse(YSetBox.Text, out yvalue);
			float xvalue;
			float.TryParse(XSetBox.Text, out xvalue);
			if (Math.Abs(2 * (xvalue - 1.8)) + Math.Abs(yvalue - 0.1 + 0.6) < 12)
			{
				float.TryParse(YSetBox.Text, out y_set_box);
				y_set_box -= (float)0.1;
				YSetBox.Text = y_set_box.ToString("0.00");
			}
		}
        private void Y_Up_Click(object sender, RoutedEventArgs e)
        {
			float yvalue;
			float.TryParse(YSetBox.Text, out yvalue);
			float xvalue;
			float.TryParse(XSetBox.Text, out xvalue);
			if (Math.Abs(2 * (xvalue - 1.8)) + Math.Abs(yvalue + 0.1 + 0.6) < 12)
			{
				float.TryParse(YSetBox.Text, out y_set_box);
				y_set_box += (float)0.1;
				YSetBox.Text = y_set_box.ToString("0.00");
			}
		}
		private void Pan_Down_Click(object sender, RoutedEventArgs e)
		{
			float panvalue;
			float.TryParse(PanSetBox.Text, out panvalue);
			if (Math.Abs(panvalue - 0.1) < 12)
			{
				float.TryParse(PanSetBox.Text, out pan_set_box);
				pan_set_box -= (float)0.1;
				PanSetBox.Text = pan_set_box.ToString("0.00");
			}
		}
		private void Pan_Up_Click(object sender, RoutedEventArgs e)
        {
			float panvalue;
			float.TryParse(PanSetBox.Text, out panvalue);
			if (Math.Abs(panvalue + 0.1) < 12)
			{
				float.TryParse(PanSetBox.Text, out pan_set_box);
				pan_set_box += (float)0.1;
				PanSetBox.Text = pan_set_box.ToString("0.00");
			}
		}

        private void CameraButton_Checked(object sender, RoutedEventArgs e)
        {
			if (CameraButton.IsChecked == true)
			{
				camera_text = "Camera: ON";
			}
			else
			{
				camera_text = "Camera: OFF";
			}
			CameraButton.Content = camera_text;
		}

        private void YawRightButton_Click(object sender, RoutedEventArgs e)
        {
			jog_yaw_out += 1;
			Yaw.Text = "Yaw: " + jog_yaw_out.ToString();
		}

        private void PitchUpButton_Click(object sender, RoutedEventArgs e)
		{
			jog_pitch_out += 1;
			Pitch.Text = "Pitch: " + jog_pitch_out.ToString();
		}

        private void YawLeftButton_Click(object sender, RoutedEventArgs e)
        {
			jog_yaw_out -= 1;
			Yaw.Text = "Yaw: " + jog_yaw_out.ToString();
		}

        private void PitchDownButton_Click(object sender, RoutedEventArgs e)
        {
			jog_pitch_out -= 1;
			Pitch.Text = "Pitch: " + jog_pitch_out.ToString();
		}

        private void PanLeftButton_Click(object sender, RoutedEventArgs e)
        {
			jog_pan_out -= 1;
			Pan.Text = "Pan: " + jog_pan_out.ToString();
		}

        private void PanRightButton_Click(object sender, RoutedEventArgs e)
        {
			jog_pan_out += 1;
			Pan.Text = "Pan: " + jog_pan_out.ToString();
		}

		private void LevelStatus_Click(object sender, RoutedEventArgs e)
		{
			level = true;
		}

		private void PanStatus_Click(object sender, RoutedEventArgs e)
        {
			pan = true;
		}
    }
}
