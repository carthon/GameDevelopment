using Unity.Jobs;
using UnityEngine;

namespace _Project.Libraries.Marching_Cubes.Scripts {
	public struct MeshBaker : IJob
	{
		int meshID;

		public MeshBaker(int meshID)
		{
			this.meshID = meshID;
		}

		public void Execute()
		{
			Physics.BakeMesh(meshID, convex: false);
		}
	}
}
