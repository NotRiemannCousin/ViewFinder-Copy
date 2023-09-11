using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using System.Linq;

namespace Unity.FPS.Game
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