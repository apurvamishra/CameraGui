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
		public bool newMotorState = false;

		private void XClicked(object sender, RoutedEventArgs e)
		{
			LEDColour.Fill = new SolidColorBrush(Color.FromRgb(70, 172, 204));
		}
		private void YClicked(object sender, RoutedEventArgs e)
		{
			if (newMotorState is false)
			{
				LEDColour.Fill = new SolidColorBrush(Color.FromRgb(86, 139, 179));
			}
			else
			{
				LEDColour.Fill = new SolidColorBrush(Color.FromRgb(173, 220, 255));
			}
		}

		public IOHome()
		{
			// Connecting to PLC
			ConnectToPLC();

			// Start GUI
			InitializeComponent();
		}

		public void ReadFromPLC(ChiefDatabase CB)
        {
			// Grab the PLC information

			// Update GUI

			Dispatcher.Invoke(delegate
			{
				if (newMotorState is false)
				{
					LEDColour.Fill = new SolidColorBrush(Color.FromRgb(86, 139, 179));
					LEDText.Foreground = new SolidColorBrush(Color.FromRgb(255, 242, 213));
					string UpdateText = "LED is off";
					LEDText.Text = UpdateText;
				}
				else
				{
					LEDColour.Fill = new SolidColorBrush(Color.FromRgb(173, 220, 255));
					LEDText.Foreground = new SolidColorBrush(Color.FromRgb(179, 142, 86));
					string UpdateText = "LED is on";
                    LEDText.Text = UpdateText;
				}
				
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
			var acclinDBNum = 3;
			plc.Connect();
			chiefDB.ReadFromDB(plc, acclinDBNum);
			// FAULT DATA
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
					Console.WriteLine(chiefdb.SetState.Value);
					newMotorState = chiefdb.SetState.Value;
					ReadFromPLC(chiefdb);
					Console.WriteLine("\n" + "---------------------------------------------------------" + "\n");
				});
		}
	}
}
