using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Vector3Variable", menuName = "ScriptableObjects/Variables/Vector3Variable", order = 1)]
    public class Vector3Variable : ScriptableObject
    {
        [SerializeField] private Vector3 value;
        [SerializeField] [TextArea] private string description;
        
        [Header("Readonly Values")]
        [SerializeField] private float magnitude;
        [SerializeField] private float planarMagnitude;
        
        public Vector3 Value
        {
            get => value;
            set {
                this.magnitude = value.magnitude;
                this.planarMagnitude = Vector3.ProjectOnPlane(value, Vector3.up).magnitude;
                this.value = value;
            }
        }

        
    }
}