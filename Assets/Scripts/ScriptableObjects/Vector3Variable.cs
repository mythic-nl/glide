using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Vector3Variable", menuName = "ScriptableObjects/Variables/Vector3Variable", order = 1)]
    public class Vector3Variable : ScriptableObject
    {
        [SerializeField] private Vector3 value;
        
        public Vector3 Value
        {
            get => value;
            set => this.value = value;
        }
    }
}