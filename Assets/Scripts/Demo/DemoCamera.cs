using UnityEngine;

namespace Demo
{
	public class DemoCamera : MonoBehaviour
	{
		[SerializeField] private Transform[] cameraPoints;
		[SerializeField] private float timePerPoint = 5f;
		
		private Transform trans;
		private int currentCamIndex = -1;
		private float nextCamTimeStamp;

		protected void Awake()
		{
			trans = GetComponent<Transform>();
		}

		protected void Update()
		{
			if(cameraPoints == null || cameraPoints.Length == 0)
				return;

			if(Time.time > nextCamTimeStamp)
			{
				currentCamIndex = ++currentCamIndex % cameraPoints.Length;
				Transform camPoint = cameraPoints[currentCamIndex];
				trans.position = camPoint.position;
				trans.rotation = camPoint.rotation;

				nextCamTimeStamp = Time.time + timePerPoint;
			}
		}
	}
}