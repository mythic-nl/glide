using UnityEngine;

namespace ScriptableObjects 
{
    [CreateAssetMenu(fileName = "Vector2Variable", menuName = "ScriptableObjects/Variables/Vector2Variable", order = 2)]
    public class Vector2Variable : ScriptableObject
    {
        [SerializeField] private Vector2 value;
        
        public Vector2 Value
        {
            get => value;
            set => this.value = value;
        }
    }
}