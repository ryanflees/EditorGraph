using UnityEngine;

public class MovementController : MonoBehaviour
{
    //rotation
    public Transform targetToLook;

    private float _rotationSpeed;
    private Vector3 _rotationOffset;
    //movement
    private Transform _targetToGo;
    private Vector3 _movementOffset;
    private float _smoothTime;
    private bool _alloweMovement;
    private bool _glueAtTarget;
    private Vector3 _velocity;
    private float _distanceOffset;

    protected void Init()
    {
        _rotationSpeed = 1;
    }

    protected void MovementControllerUpdate()
    {
        // look 
        if (targetToLook != null)
        {
            var targetRotation = Quaternion.LookRotation(targetToLook.transform.position + _rotationOffset - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
        // movement
        if (_alloweMovement)
        {
            // colliding
            Vector3 additionalUP = Vector3.zero;

            Vector3 targetPosition = _targetToGo.position + _movementOffset + additionalUP;
            Vector3 distanceOff = (targetPosition - transform.position).normalized * _distanceOffset;

            transform.position = Vector3.SmoothDamp(transform.position + distanceOff, targetPosition, ref _velocity, _smoothTime) - distanceOff;
            if (Approximately(transform.position, targetPosition - distanceOff, 1f, true))
            {
                if (!_glueAtTarget)
                    _alloweMovement = false;
            }
        }
    }


    /// <summary> Płynnie obraca się w zadanym kierunku. </summary>
    /// <param name="target">Cel, na który ma się patrzeć kamera.</param>
    /// <param name="offset">Przesunięcie punktu na który ma się patrzeć kamera.</param>
    /// <param name="speed">Szybkość obrotu.</param>
    public void SmoothLookAt(Transform target, Vector3 offset, float speed)
    {
        _rotationOffset = offset;
        targetToLook = target;
        _smoothTime = speed;
    }
    /// <summary> Płynnie obraca się w zadanym kierunku. </summary>
    /// <param name="target">Cel, na który ma się patrzeć kamera.</param>
    /// <param name="speed">Szybkość obrotu.</param>
    public void SmoothLookAt(Transform target, float speed)
    {
        SmoothLookAt(target, Vector3.zero, speed);
    }

    /// <param name="target">Miejsce, do kórego ma dotrzeć kamera.</param>
    /// <param name="offset">Przesunięcie celu kamery.</param>
    /// <param name="smoothTime">Przybliżony czas [s] w jakim kamera ma dotrzeć do celu.</param>
    /// <param name="glueAtTarget">Jeśli prawda, w momencie kiedy kamera dotrze do celu, przyklei się do niego (tryb podążania)</param>
    /// <param name="callback">Sprzężenie zwrotne gdy kamera dotrze do celu.</param>
    public void SmoothMoveTo(Transform target, Vector3 offset, float distanceOffset, float smoothTime, bool glueAtTarget)
    {
        _targetToGo = target;
        _movementOffset = offset;
        _glueAtTarget = glueAtTarget;
        _smoothTime = smoothTime;
        _distanceOffset = distanceOffset;

        _alloweMovement = true;
        _velocity = Vector3.zero;
    }


    private static bool Approximately(Vector3 a, Vector3 b, float delta = 0.001f, bool excludeYAxis = false)
    {
        return Mathf.Abs(a.x - b.x) < delta && (excludeYAxis || !excludeYAxis && Mathf.Abs(a.y - b.y) < delta) && Mathf.Abs(a.z - b.z) < delta;
    }
}
