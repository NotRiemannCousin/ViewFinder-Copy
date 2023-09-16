using UnityEngine;

namespace ViewFinder.Gameplay
{
    public class DaVinciCamera : ItemController
    {
        #region Properties
        public Camera SkyCamera;
        public Camera CameraTarget;
        public Photo _Photo;
        #endregion


        #region Additional Methods
        protected override void OnUse()
        {
            _Photo.SayX();
        }
        #endregion

    }
}