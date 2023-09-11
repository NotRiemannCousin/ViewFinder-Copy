using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public enum PositionType
    {
        Default,
        Aiming,
        Down
    }

    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerItemsManager : MonoBehaviour
    {
        [Tooltip("The camera for DaVinci Camera")]
        public DaVinciCamera m_Camera;

        [Tooltip("The photo for DaVinci Camera")]
        public Photo m_Photo;

        [Header("References")]

        [Tooltip("Parent transform where all items will be added in the hierarchy")]
        public Transform ItemParentSocket;

        [Tooltip("Position for items when active but not actively aiming")]
        public Transform DefaultItemPosition;

        [Tooltip("Position for items when aiming")]
        public Transform AimingItemPosition;

        [Tooltip("Position for inactive items")]
        public Transform DownItemPosition;

        public float AimingAnimationSpeed = 0.3f;


        public bool IsAiming { get; private set; }
        public int ActiveItemIndex { get; private set; }

        private PlayerInputHandler m_InputHandler;
        private PlayerCharacterController m_PlayerCharacterController;

        private void Start()
        {
            ActiveItemIndex = 0;

            m_InputHandler = GetComponent<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerItemsManager>(m_InputHandler, this, gameObject);

            m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerItemsManager>(m_PlayerCharacterController, this, gameObject);


            m_Camera.ShowItem(false);
            m_Photo.ShowItem(false);

            GetActiveItem().ShowItem(true);
            // ItemParentSocket.position = DownItemPosition.position;
            ItemParentSocket.position = AimingItemPosition.position;}


        private void Update()
        {
            LerpPosition();

            if (m_InputHandler.GetAimInputDown())
                IsAiming = !IsAiming;

            // Shoot handling
            var activeItem = GetActiveItem();
            if (activeItem == null)
                return;

            if (IsAiming && m_InputHandler.GetFireInputDown())
            {
                if (activeItem.TryUse())
                    SwitchItem();
            }
        }


        private void LerpPosition()
        {
            Vector3 destinationPos = DefaultItemPosition.localPosition;

            if (IsAiming)
                destinationPos = AimingItemPosition.localPosition + GetActiveItem().AimOffset;

            float movementDelta = Time.deltaTime * AimingAnimationSpeed;
            Vector3 newLocation = Vector3.MoveTowards(ItemParentSocket.localPosition, destinationPos, movementDelta);

            if (newLocation != ItemParentSocket.localPosition)
            {
                ItemParentSocket.localPosition = newLocation;
            }
        }

        // Iterate on all item slots to find the next valid item to switch to
        public void SwitchItem()
        {
            if (!IsAiming)
                return;
            ItemParentSocket.position = DownItemPosition.position;
            IsAiming = false;
            GetActiveItem().ShowItem(false);
            int v = ActiveItemIndex == 0 ? 1 : 0;
            ActiveItemIndex = v;
            GetActiveItem().ShowItem(true);
        }

        // Adds an item to our inventory


        public ItemController GetActiveItem()
        {
            return ActiveItemIndex == 0 ? m_Camera : m_Photo;
        }
    }
}
