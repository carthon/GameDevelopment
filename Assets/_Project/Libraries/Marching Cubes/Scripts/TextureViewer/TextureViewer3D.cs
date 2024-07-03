using UnityEngine;

namespace _Project.Libraries.Marching_Cubes.Scripts.TextureViewer {
	public class TextureViewer3D : MonoBehaviour
	{

		[Range(0,1)]
		public float sliceDepth;
		Material material;

		void Start()
		{
		
			material = GetComponentInChildren<MeshRenderer>().material;
			//
		}

		public void Display() {

		}

	
		void Update()
		{
			material.SetFloat("sliceDepth", sliceDepth);
			material.SetTexture("DisplayTexture", FindObjectOfType<MeshGenerator>().rawDensityTexture);
		}
	}
}
