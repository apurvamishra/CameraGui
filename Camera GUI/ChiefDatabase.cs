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
        public JPLCProperty<short> pan_out { get; set; }

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

        // A button which alters a jog counter, slightly nudging the pitch angle
        public JPLCProperty<short> jog_pitch_in { get; set; }
        [Order(15)]

        // A button which alters a jog counter, slightly nudging the yaw angle
        public JPLCProperty<short> jog_yaw_in { get; set; }
        [Order(16)]

        // A button which alters a jog counter, slightly nudging the yaw angle
        public JPLCProperty<short> jog_pitch_out { get; set; }
        [Order(17)]

        // A button which alters a jog counter, slightly nudging the yaw angle
        public JPLCProperty<short> jog_yaw_out { get; set; }
        [Order(18)]

        public JPLCProperty<bool> flash_state { get; set; }
        [Order(19)]

        public JPLCProperty<bool> camera_state { get; set; }
        [Order(20)]

        public JPLCProperty<short> jog_pan_in { get; set; }
        [Order(21)]

        public JPLCProperty<short> jog_pan_out { get; set; }
        [Order(22)]

        public JPLCProperty<bool> level_status { get; set; }
        [Order(23)]

        public JPLCProperty<bool> pan_status { get; set; }

        public ChiefDatabase(int address = 0) : base(address) {  }
    }
}
