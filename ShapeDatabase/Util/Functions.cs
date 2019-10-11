using System;
using g3;
using OpenTK;

namespace ShapeDatabase.Util {

	public static class Functions {

		public static Vector3 VectorCreate(uint x, uint y, uint z) {
			return new Vector3(x, y, z);
		}

		public static Vector3 VectorCreate(int x, int y, int z) {
			return new Vector3(x, y, z);
		}

		public static Vector3 VectorCreate(float x, float y, float z) {
			return new Vector3(x, y, z);
		}

		public static Vector3 VectorCreate(double x, double y, double z) {
			return new Vector3(
				Convert.ToSingle(x),
				Convert.ToSingle(y),
				Convert.ToSingle(z)
			);
		}


		public static Vector3 VectorConvert(Vector3f vector) {
			return new Vector3(vector.x, vector.y, vector.z);
		}

		public static Vector3 VectorConvert(g3.Vector3d vector) {
			return new Vector3(
				Convert.ToSingle(vector.x),
				Convert.ToSingle(vector.y),
				Convert.ToSingle(vector.z)
			);
		}

		public static Vector3 VectorConvert(Index3i vector) {
			return new Vector3(vector.a, vector.b, vector.c);
		}

	}

}
