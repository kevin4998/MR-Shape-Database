using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.IO {

	public interface IWriter<T> {

		string[] SupportedFormats { get; }



		void WriteFile(T type, string location);

		void WriteFile(T type, StreamWriter writer);


		Task WriteFileAsync(T type, string location);

		Task WriteFileAsync(T type, StreamWriter writer);

	}
}
