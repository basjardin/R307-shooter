using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Events;
using UnityEngine;


public class Physic : MonoBehaviour{


        [SerializeField]
               UnityEvent m_OnSelected;

        public void grab()
        {
            Physics.IgnoreLayerCollision(8, 7, true);

        }
       }
    