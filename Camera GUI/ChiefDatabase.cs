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
        public JPLCProperty<bool> Power { get; set; }
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

        public ChiefDatabase(int address = 0) : base(address) {  }
    }
}
