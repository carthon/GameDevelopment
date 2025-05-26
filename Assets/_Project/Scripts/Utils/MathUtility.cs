using System;
using System.Collections.Generic;
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
		
		public static Vector2 SphericalToEquirectangular(Vector3 position)
		{
			// Normalizar la posición para obtener coordenadas unitarias en la esfera
			Vector3 normalizedPos = position.normalized;

			// Calcular la longitud (lambda) y la latitud (phi)
			double lambda = Math.Atan2(normalizedPos.z, normalizedPos.x); // Longitud
			double phi = Math.Asin(normalizedPos.y); // Latitud

			// Convertir los ángulos a coordenadas 2D (u, v)
			double u = (lambda + Math.PI) / (2 * Math.PI); // Normalizar longitud a [0, 1]
			double v = (phi + (Math.PI / 2)) / Math.PI; // Normalizar latitud a [0, 1]

			return new Vector2((float) u, (float) v);
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

		public static int CeilToInt(float value) => (int)Ceil(value);

		public static (int, int) IndexToCoordinates(int index, int width) {
			return (index % width, index / width);
		}
		public static int CoordinatesToIndex(int x, int y, int width) => y * width + x;
	}
}
