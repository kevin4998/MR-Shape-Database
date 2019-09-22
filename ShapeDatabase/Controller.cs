using OpenTK;
using ShapeDatabase.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShapeDatabase
{
    public class Controller {

		public Controller() {
			using (var window = new Window(800, 600, "Multimedia Retrieval - K. Westerbaan & G. de Jonge"))
			{
				window.Run(60.0);
			}
		}
	}
}
