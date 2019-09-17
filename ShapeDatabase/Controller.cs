using OpenTK;
using ShapeDatabase.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShapeDatabase
{
    public class Controller {

		public Controller() {

			using (Window window = new Window(800, 600, "LearnOpenTK"))
			{
				//Run takes a double, which is how many frames per second it should strive to reach.
				//You can leave that out and it'll just update as fast as the hardware will allow it.
				window.Run(60.0);
			}
		}
	}
}
