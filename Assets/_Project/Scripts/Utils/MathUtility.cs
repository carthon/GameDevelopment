using UnityEngine;
using static UnityEngine.Mathf;

namespace _Project.Scripts.Utils {
	public static class MathUtility
	{
		public static uint FloatToUint(float f)
		{
			byte[] bytes = System.BitConverter.GetBytes(f); // 4 bytes
			return System.BitConverter.ToUInt32(bytes, 0);
		}

		public static float UintToFloat(uint i)
		{
			byte[] bytes = System.BitConverter.GetBytes(i);
			return System.BitConverter.ToSingle(bytes, 0);
		}

		public static bool SphereIntersectsBox(Vector3 sphereCentre, float sphereRadius, Vector3 boxCentre, Vector3 boxSize)
		{
			float closestX = Clamp(sphereCentre.x, boxCentre.x - boxSize.x / 2, boxCentre.x + boxSize.x / 2);
			float closestY = Clamp(sphereCentre.y, boxCentre.y - boxSize.y / 2, boxCentre.y + boxSize.y / 2);
			float closestZ = Clamp(sphereCentre.z, boxCentre.z - boxSize.z / 2, boxCentre.z + boxSize.z / 2);

			float dx = closestX - sphereCentre.x;
			float dy = closestY - sphereCentre.y;
			float dz = closestZ - sphereCentre.z;

			float sqrDstToBox = dx * dx + dy * dy + dz * dz;
			return sqrDstToBox < sphereRadius * sphereRadius;
		}

		// Transform vector from local space to world space (based on rotation)
		public static Vector3 LocalToWorldVector(Quaternion rotation, Vector3 vector)
		{
			return rotation * vector;
		}

		// Transform vector from world space to local space (based on rotation)
		public static Vector3 WorldToLocalVector(Quaternion rotation, Vector3 vector)
		{
			return Quaternion.Inverse(rotation) * vector;
		}

		public static int CoordToIndex(int x, int y, int width) => y * width + x;
		public static void IndexToCoord(int index, int width, ref int x, ref int y) {
			y = index / width;
			x = index % width;
		}

	}
}
