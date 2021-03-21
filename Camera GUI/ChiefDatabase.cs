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
        [Order(1)]
        public JPLCProperty<bool> flash_state { get; set; }
        [Order(2)]
        public JPLCProperty<float> x_out { get; set; }
        [Order(3)]
        public JPLCProperty<float> y_out { get; set; }
        [Order(4)]
        public JPLCProperty<float> pan_out { get; set; }
        [Order(5)]
        public JPLCProperty<float> pan_in { get; set; }
        [Order(6)]
        public JPLCProperty<float> x_in { get; set; }
        [Order(7)]
        public JPLCProperty<float> y_in { get; set; }
        [Order(8)]
        public JPLCProperty<short> flash_count { get; set; }
        [Order(9)]
        public JPLCProperty<bool> cool_state { get; set; }
        [Order(10)]
        public JPLCProperty<float> temperature { get; set; }
        [Order(11)]
        public JPLCProperty<float> battery_level { get; set; }
        [Order(12)]
        public JPLCProperty<bool> charge_state { get; set; }


        public ChiefDatabase(int address = 0) : base(address) {  }
    }
}
