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
        // An output, to whether the flash is enabled or not
        [Order(1)]
        public JPLCProperty<bool> flash_state { get; set; }

        // An output, setting the zero for the X axis
        [Order(2)]
        public JPLCProperty<float> x_out { get; set; }

        // An output, setting the zero for the Y axis
        [Order(3)]
        public JPLCProperty<float> y_out { get; set; }

        // An output, setting the target angle for the camera to pan to
        [Order(4)]
        public JPLCProperty<float> pan_out { get; set; }

        // An input, showing the current angle the camera is currently panned to
        [Order(5)]
        public JPLCProperty<float> pan_in { get; set; }

        // An input, showing the current incline of the X axis
        [Order(6)]
        public JPLCProperty<float> x_in { get; set; }

        // An input, showing the current incline of the Y axis
        [Order(7)]
        public JPLCProperty<float> y_in { get; set; }

        // An input, showing the interative count of the flashes that have gone off
        [Order(8)]
        public JPLCProperty<short> flash_count { get; set; }

        // An input, showing the current state of the cooling systems, driven by the temprature of the camera
        [Order(9)]
        public JPLCProperty<bool> cool_state { get; set; }

        // An input, showing the current temperature of the camera
        [Order(10)]
        public JPLCProperty<float> temperature { get; set; }

        // An input, showing the estimated remaining percentage of the battery
        [Order(11)]
        public JPLCProperty<float> battery_level { get; set; }

        // An input, showing whether the battery is charging or not
        [Order(12)]
        public JPLCProperty<bool> charge_state { get; set; }

        public ChiefDatabase(int address = 0) : base(address) {  }
    }
}
