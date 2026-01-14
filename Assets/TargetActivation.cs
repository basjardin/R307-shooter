using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Events;
using UnityEngine;


public class TargetActivation : MonoBehaviour{


        [SerializeField]
               UnityEvent m_OnSelected;

        void Start()
        {
            m_OnSelected.Invoke();

        }
       }
    