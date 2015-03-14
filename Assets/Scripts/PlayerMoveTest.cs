using UnityEngine;
using System.Collections;

public class PlayerMoveTest : MonoBehaviour {

	[SerializeField]
	private Transform geometryTarget;
	[SerializeField]
	private float accelerationForce;
	[SerializeField]
	private float decelerationForce;
	[SerializeField]
	private float maxSpeed;
	[SerializeField]
	private float rotationSpeed;
	[SerializeField]
	private float jumpForce;
	[SerializeField]
	private float cubeFallForce;
	[SerializeField]
	private float sphereDrag;
	[SerializeField]
	private float cylindreDrag;
	[SerializeField]
	Mesh sphereMesh;
	[SerializeField]
	Mesh cylindreMesh;	
	[SerializeField]
	Mesh cubeMesh;
	[SerializeField]
	LayerMask groundedMask;
	[SerializeField]
	Transform groundCam;
	[SerializeField]
	Transform airCam;

	private enum Forme{
		SPHERE,
		CYLINDRE,
		CUBE
	}

	private Transform camTrans;
	private Forme forme = Forme.SPHERE;
	private Transform mTransform;
	private Rigidbody rigidbody;
	private float hAxis = 0f;
	private float vAxis = 0f;
	private bool isGrounded = false;

	void Awake(){
		mTransform = GetComponent<Transform> ();
	}

	void Start(){
		rigidbody = geometryTarget.GetComponent<Rigidbody> ();
		camTrans = Camera.main.GetComponent<Transform> ();
	}

	void FixedUpdate(){
		isGrounded = CheckGrounded ();
		if (forme == Forme.SPHERE) {
			if (isGrounded) {
				if (vAxis > 0 && rigidbody.velocity.magnitude < maxSpeed) {
					rigidbody.AddForce (mTransform.forward * accelerationForce);
				} else if (vAxis < 0) {
					rigidbody.AddForce (-mTransform.forward * decelerationForce);
				}
			}
		} else if (forme == Forme.CUBE) {
			//rigidbody.MovePosition(geometryTarget.position + Vector3.down * cubeFallForce);
			rigidbody.AddForce (-mTransform.up * cubeFallForce);
		}
	}

	void LateUpdate(){
		mTransform.position = geometryTarget.position;
		if (isGrounded) {
			camTrans.position = Vector3.Slerp(camTrans.position, groundCam.position, 0.05f);
			camTrans.rotation = Quaternion.Slerp(camTrans.rotation, groundCam.rotation, 0.05f);
		} else {
			camTrans.position = Vector3.Slerp(camTrans.position, airCam.position, 0.05f);
			camTrans.rotation = Quaternion.Slerp(camTrans.rotation, airCam.rotation, 0.05f);
		}
	}

	void Update(){
		//base
		hAxis = Input.GetAxis ("Horizontal");
		vAxis = Input.GetAxis ("Vertical");
		if (Input.touchCount > 0) {
			Vector2 touchPos = Input.GetTouch(0).position;
			if(touchPos.x > Screen.width / 3 && touchPos.x < Screen.width * 2/3){
				vAxis = 1;
			}else if(touchPos.x < Screen.width / 3){
				hAxis = -1;
			}else{
				hAxis = 1;
			}
		}
		mTransform.Rotate (mTransform.up * hAxis * rotationSpeed);
		if(forme == Forme.SPHERE)
			rigidbody.velocity = Quaternion.AngleAxis(hAxis * rotationSpeed, mTransform.up) * rigidbody.velocity;
		//cylindre
		if(Input.GetKeyDown(KeyCode.Alpha2)){
			ChangeToCylindre();
		}
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			ChangeToSphere();
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			ChangeToCube();
		}
	}
	
	void ChangeToCylindre(){
		forme = Forme.CYLINDRE;
		rigidbody.isKinematic = false;
		geometryTarget.GetComponent<MeshFilter> ().mesh = cylindreMesh;
		rigidbody.drag = cylindreDrag;
		geometryTarget.rotation = Quaternion.Euler (geometryTarget.rotation.x, mTransform.rotation.y, geometryTarget.rotation.z);
		if(isGrounded)
			rigidbody.AddForce (Vector3.up * jumpForce);
		geometryTarget.GetComponent<SphereCollider> ().enabled = false;
		geometryTarget.GetComponent<CapsuleCollider> ().enabled = true;
		geometryTarget.GetComponent<BoxCollider> ().enabled = false;
	}

	void ChangeToSphere(){
		forme = Forme.SPHERE;
		rigidbody.isKinematic = false;
		rigidbody.drag = sphereDrag;
		geometryTarget.GetComponent<MeshFilter> ().mesh = sphereMesh;
		geometryTarget.GetComponent<SphereCollider> ().enabled = true;
		geometryTarget.GetComponent<CapsuleCollider> ().enabled = false;
		geometryTarget.GetComponent<BoxCollider> ().enabled = false;

	}

	void ChangeToCube(){
		forme = Forme.CUBE;
		rigidbody.drag = cylindreDrag;
		//rigidbody.isKinematic = true;
		StartCoroutine(StopVelocity ());
		geometryTarget.GetComponent<MeshFilter> ().mesh = cubeMesh;
		geometryTarget.GetComponent<BoxCollider> ().enabled = true;
		geometryTarget.GetComponent<SphereCollider> ().enabled = false;
		geometryTarget.GetComponent<CapsuleCollider> ().enabled = false;
		geometryTarget.transform.rotation = mTransform.rotation;
		
	}

	IEnumerator StopVelocity(){
		rigidbody.isKinematic = true;
		rigidbody.velocity.Set (0, 0, 0);
		rigidbody.angularVelocity.Set (0, 0, 0);
		yield return null;
		rigidbody.isKinematic = false;
	}

	bool CheckGrounded(){
		Collider collider = forme == Forme.CUBE ? (Collider)geometryTarget.GetComponent<BoxCollider> () : forme == Forme.SPHERE ? (Collider)geometryTarget.GetComponent<SphereCollider> () : (Collider)geometryTarget.GetComponent<CapsuleCollider> ();
		return Physics.Raycast (mTransform.position, Vector3.down, collider.bounds.size.y/2 + 0.1f , groundedMask);
	}
}
