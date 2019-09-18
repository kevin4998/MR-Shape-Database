using OpenTK;
using ShapeDatabase.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShapeDatabase
{
    public class Controller {

		public Controller() {
			using (var window = new Window(800, 600, "LearnOpenTK - Camera"))
			{
				window.Run(60.0);
			}
		}
	}
}
