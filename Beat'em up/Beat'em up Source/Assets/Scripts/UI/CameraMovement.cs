using UnityEngine;

public class CameraMovement : MonoBehaviour
{
	public float DampTime = 0.15f;
	public Transform Target;

	private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
		Target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
	{
		if (Target) {
			Vector3 point = Camera.main.WorldToViewportPoint(new Vector3(Target.position.x, Target.position.y + 0.75f, Target.position.z));
			Vector3 delta = new Vector3(Target.position.x, Target.position.y + 0.75f, Target.position.z) - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
			Vector3 destination = transform.position + delta;


			transform.position = Vector3.SmoothDamp(transform.position, destination, ref _velocity, DampTime);
		}

	}
}
