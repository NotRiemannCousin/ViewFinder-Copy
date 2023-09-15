using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ViewFinder.Gameplay
{
    [System.Serializable]
    public abstract class ItemController : MonoBehaviour
    {
        [Header("Internal References")]
        [Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
        public Vector3 AimOffset;

        
        public GameObject Owner { get; set; }
        public GameObject SourcePrefab { get; set; }


        public bool IsActive { get; private set; }
        public bool IsUsing { get; private set; }


        protected virtual void OnUse() => print("Using...");
        public bool IsReadyToUse() => IsActive && !IsUsing;

        public void ShowItem(bool show)
        {
            if(show == false)
                IsUsing = false;

            gameObject.SetActive(show);

            IsActive = show;
        }

        public bool TryUse()
        {
            if (IsReadyToUse())
            {
                IsUsing = true;
                OnUse();
                return true;
            }
            return false;
        }
    }
}