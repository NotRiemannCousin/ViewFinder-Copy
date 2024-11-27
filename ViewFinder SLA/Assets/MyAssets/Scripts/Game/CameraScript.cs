using UnityEngine;

namespace ViewFinder.Gameplay
{
    // * Maybe I should change the name, but I have no ideas for now...
    public class CameraScript : ItemController
    {
        #region Properties
        [SerializeField] Camera SkyCamera;
        [SerializeField] Camera CameraTarget;
        [SerializeField] Photo _Photo;
        #endregion


        #region Additional Methods
        protected override void OnUse()
        {
            _Photo.SayCheese();
        }
        #endregion

    }
}