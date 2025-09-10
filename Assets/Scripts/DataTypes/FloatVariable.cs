using UnityEngine;

namespace DataTypes
{
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "Data Types/Float Variable", order = 1)]
    public class FloatVariable : ScriptableObject
    {
        public float value;

        public void Set(float newValue)
        {
            value = newValue;
        }

        public void Increase(float amount)
        {
            value += amount;
        }
        
        public void Decrease(float amount)
        {
            value -= amount;
        }
    }
}