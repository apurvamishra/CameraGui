using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPLC;

namespace Camera_GUI
{
    public class ChiefDatabase : JPLC_BASE
    {
        // An output, allowing the camera will level
        [Order(1)]
        public JPLCProperty<bool> level { get; set; }

        // An output, allowing the pan to find its centre
        [Order(2)]
        public JPLCProperty<bool> pan { get; set; }

        // An output, setting the zero for the X axis
        [Order(3)]
        public JPLCProperty<short> x_out { get; set; }

        // An output, setting the zero for the Y axis
        [Order(4)]
        public JPLCProperty<short> y_out { get; set; }

        // An output, setting the target angle for the camera to pan to
        [Order(5)]
        public JPLCProperty<float> pan_out { get; set; }

        // An input, showing the current angle the camera is currently panned to
        [Order(6)]
        public JPLCProperty<float> pan_in { get; set; }

        // An input, showing the current incline of the X axis
        [Order(7)]
        public JPLCProperty<float> x_in { get; set; }

        // An input, showing the current incline of the Y axis
        [Order(8)]
        public JPLCProperty<float> y_in { get; set; }

        // An input, showing the interative count of the flashes that have gone off
        [Order(9)]
        public JPLCProperty<short> flash_count { get; set; }

        // An input, showing the current state of the cooling systems, driven by the temprature of the camera
        [Order(10)]
        public JPLCProperty<bool> cool_state { get; set; }

        // An input, showing the current temperature of the camera
        [Order(11)]
        public JPLCProperty<float> temperature { get; set; }

        // An input, showing the estimated remaining percentage of the battery
        [Order(12)]
        public JPLCProperty<float> battery_level { get; set; }

        // An input, showing whether the battery is charging or not
        [Order(13)]
        public JPLCProperty<bool> e_stop { get; set; }
        [Order(14)]
        public JPLCProperty<bool> M1B { get; set; }
        [Order(15)]
        public JPLCProperty<bool> M1F { get; set; }
        [Order(16)]
        public JPLCProperty<bool> M2B { get; set; }
        [Order(17)]
        public JPLCProperty<bool> M2F { get; set; }

        public ChiefDatabase(int address = 0) : base(address) {  }
    }
}
