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
        public JPLCProperty<bool> SetState { get; set; }
/*        [Order(2)]
        public JPLCProperty<bool> Pulsed { get; set; }*/

        public ChiefDatabase(int address = 0) : base(address) {  }
    }
}
