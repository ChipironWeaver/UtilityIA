using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class UtilityAI : MonoBehaviour
{
    [SerializeField] public Dictionary<string, float> variables = new();
    public string[] variablesKeys => variables.Keys.ToArray();

    [SerializeField] private List<DistanceVariable> _distanceVariables = new();
    
    
    [Dropdown("variablesKeys")] public string variablesKey;

    [Serializable]
    private class DistanceVariable
    {
        public DistanceVariable(UtilityAI utilityAI)
        {
            _variablesKeys = utilityAI.variablesKeys;
        }
        
        private string[] _variablesKeys = {""};
        
        [Dropdown("_variablesKeys")] public string key;
        public AnimationCurve animationCurve;
        public Transform target;
        public float distanceBase;

        public float Distance(Vector3 currentPosition)
        {
            return animationCurve.Evaluate(Mathf.Clamp01(Vector3.Distance(currentPosition, target.position) / distanceBase));
        }
    }
}
