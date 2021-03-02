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


namespace Camera_GUI
{
	/// <summary>
	/// Interaction logic for IOHome.xaml
	/// </summary>
	public partial class IOHome : Page
	{
		// On bootup, create local instance of acclination database

		private void Button_Click(object sender, RoutedEventArgs e)
		{ 
			// Synchronise local database with PLC database

		}

		public IOHome()
		{
			// Connecting to PLC
			ConnectToPLC();

			// Start GUI
			InitializeComponent();
		}

		public void ConnectToPLC()
        {
			// Connecting to PLC
			JPLCConnection plc = new JPLCConnection("192.168.1.140", 0, 1);
			string csvFilepath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\OPF.csv";
			double timeBetweenRetries = 6; // seconds
			double timeBetweenReads = 5; //seconds

			// Setup program
			var chiefDB = new ChiefDatabase();
			var acclinDBNum = 1337;

			// FAULT DATA
			var readFaultDBObservable = Observable.Create<ChiefDatabase>(o =>
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
				Observable.Interval(TimeSpan.FromSeconds(timeBetweenReads))
				.Zip(combinedDBObservable, (interval, dbs) => dbs).Repeat()
			);

			var disposable = observable
				.ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
				.RetryWhen(errors => errors.SelectMany(Observable.Timer(TimeSpan.FromSeconds(timeBetweenRetries))))
				.Subscribe(dbs =>
				{
					var (chiefDB) = dbs;
					//===============================================================================
					// THIS SECTION OF CODE WILL REPEAT

					Console.WriteLine("\n" + $"Reading from PLC Beam / Fault DataBase on {DateTime.Now}");
					Console.WriteLine(chiefDB);
					WriteToBeamsSqlDB(cnn, sql, chiefDB);
					Console.WriteLine("\n" + "---------------------------------------------------------" + "\n");
				});
		}
	}
}
