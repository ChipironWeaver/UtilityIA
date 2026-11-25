using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UtilityAI : MonoBehaviour
{
    [SerializeField] public Dictionary<string, float> variables = new();
    [SerializeField] public Dictionary<string, Image> variablesUiElement = new();

    [SerializeField] private float _distanceGoalCompleteDistance;
    
    [SerializeField] private List<DistanceVariable> _distanceVariables = new();
    [SerializeField] private List<TimeVariable> _timeVariables = new();
    [SerializeField] private List<Goals> _goals = new();
    
    [SerializeField] private bool _disableMovement;

    private NavMeshAgent _agent;
    public void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public void FixedUpdate()
    {
        foreach (var variable in _distanceVariables)
        {
            if (variables.ContainsKey(variable.key))
            {
                variables[variable.key] = variable.Distance(transform.position);
            }
        }
        foreach (var variable in _timeVariables)
        {
            variable.Update();
            if (variables.ContainsKey(variable.key))
            {
                variables[variable.key] = variable.GetCurrentTime;
            }
        }
        
        float currentMaxWeight = Mathf.NegativeInfinity;
        Vector3 currentTargetPosition = Vector3.zero;
        Goals winningGoal = null;
        //Check for completed goals
        foreach (Goals goal in _goals)
        {
            goal.goalText.color = Color.black;
            if (Vector3.Distance(goal.goalCompleteTarget.position, transform.position) <= _distanceGoalCompleteDistance)
            {
                foreach (var variable in _timeVariables)
                {
                    if (goal.variablesToReset.Contains(variable.key))
                    {
                        variable.Reset();
                    }
                }
            }
            
            float goalWeight = goal.GetWeights(variables);
            if (goalWeight > currentMaxWeight)
            {
                currentMaxWeight = goalWeight;
                currentTargetPosition = goal.goalCompleteTarget.position;
                winningGoal = goal;
            }
        }

        winningGoal.goalText.color = Color.green;
        if(!_disableMovement)
        {
            _agent.isStopped = false;
            _agent.SetDestination(currentTargetPosition);
        }
        else _agent.isStopped = true;

        foreach (string key in variablesUiElement.Keys)
        {
            if(variables.TryGetValue(key, out float value))
            {
                variablesUiElement[key].fillAmount = value;
            }
        }
    }
    [Serializable]
    private class DistanceVariable
    {
        public string key;
        public AnimationCurve animationCurve;
        public Transform target;
        public float distanceBase; 
        
        public float Distance(Vector3 currentPosition)
        {
            return animationCurve.Evaluate(Mathf.Clamp01((distanceBase - Vector2.Distance(currentPosition, target.position)) / distanceBase));
        }
    }
    [Serializable]
    private class TimeVariable
    {
        public string key;
        public AnimationCurve animationCurve;
        public float timeRate;
        public float maxTime;
        
        private float currentTime;

        public void Reset()
        {
            currentTime = 0;
        }

        public void Update()
        {
            currentTime += Time.deltaTime / timeRate;
        }

        public float GetCurrentTime => animationCurve.Evaluate(Mathf.Clamp01(currentTime / maxTime));
    }

    [Serializable]
    private class Goals
    {
        [SerializeField,Tooltip("Put negative weight for one minus variable")]
        public Dictionary<string, float> variablesWeight = new();
        public Image targetGraphic;
        public TextMeshProUGUI goalText;
        
        public List<string> variablesToReset = new();
        [FormerlySerializedAs("goalPosition")] public Transform goalCompleteTarget;
        public float GetWeights(Dictionary<string, float> variables)
        {
            float totalWeight = 0;
            float totalValue = 0;
            foreach (string key in variablesWeight.Keys)
            {
                if (variables.TryGetValue(key, out float value))
                {
                    float weiht = Mathf.Abs(variablesWeight[key]);
                    totalWeight += weiht;
                    totalValue += (variablesWeight[key] > 0 ? value : 1-value)* weiht ;
                }
                else
                {
                    Debug.LogWarning("Variable " + key + " not found!");
                }
            }

            if (totalWeight == 0) return Mathf.NegativeInfinity;
            if(targetGraphic)targetGraphic.fillAmount = totalValue / totalWeight;
            return totalValue / totalWeight;
        }
    }
}