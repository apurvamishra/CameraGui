using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_GUI
{
	public class MotorController
	{
		public int angle;
		public bool relay1 { get; set; }
		public bool relay2 { get; set; }

        public void Move(ref int axis_angle)
		{
			if (axis_angle < 0)
			{
				relay1 = true;
				relay2 = false;
			}
			if (axis_angle > 0)
			{
				relay1 = false;
				relay2 = true;
			}
		}
	}
}
