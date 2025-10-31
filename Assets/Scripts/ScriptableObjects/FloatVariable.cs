using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "ScriptableObjects/Variables/FloatVariable", order = 3)]
    public class FloatVariable : ScriptableObject
    {
        [SerializeField] private float value;
        
        public float Value
        {
            get => value;
            set => this.value = value;
        }
    }
}