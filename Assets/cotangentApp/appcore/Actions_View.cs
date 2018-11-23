using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using f3;

namespace cotangent
{
    public static partial class CCActions
    {

        public static void FitCurrentSelectionToView()
        {
            AxisAlignedBox3d fitBox = AxisAlignedBox3d.Zero;
            if ( CC.ActiveScene.Selected.Count == 0 ) {
                fitBox = CC.Objects.GetPrintMeshesBounds(false);
            } else {
                fitBox = AxisAlignedBox3d.Empty;
                foreach (var so in CC.ActiveScene.Selected)
                    fitBox.Contain(so.GetBoundingBox(CoordSpace.SceneCoords).ToAABB());
            }

            double height = 3 * fitBox.Height;
            if (height < 10.0f) height = 10.0f;
            double width = 1.5 * Math.Sqrt(fitBox.Width * fitBox.Width + fitBox.Depth * fitBox.Depth);
            Vector3d center = fitBox.Center;

            if (width > height) {
                CC.ActiveScene.ActiveCamera.Animator().AnimateFitWidthToView((Vector3f)center, (float)width, CoordSpace.SceneCoords, 0.5f);
            } else {
                CC.ActiveScene.ActiveCamera.Animator().AnimateFitHeightToView((Vector3f)center, (float)height, CoordSpace.SceneCoords, 0.5f);
            }

            UpdateViewClippingBounds();
        }



        /// <summary>
        /// recomputes clipping bounds for scene. Still probably needs some work...
        /// </summary>
        public static void UpdateViewClippingBounds()
        {
            AxisAlignedBox3f worldBounds = CC.Objects.GetPrintMeshesBounds(false);
            float bed_dim = (float)MathUtil.Max(CC.Settings.BedSizeXMM, CC.Settings.BedSizeYMM, CC.Settings.BedSizeZMM);
            float maxdim = Math.Max(worldBounds.MaxDim, bed_dim);
            float fVertFOV = FPlatform.MainCamera.VertFieldOfViewDeg;
            float dist = maxdim / (float)Math.Tan(0.5f * fVertFOV * MathUtil.Deg2Radf);
            dist *= 10;
            CC.ActiveContext.CameraManager.UpdateMainCamFarDistance(dist);
        }


        /// <summary>
        /// switches camera between orthographic and perspective
        /// </summary>
        public static void UpdateCameraMode(CCPreferences.CameraModes eMode)
        {
            bool bOrthographic = (eMode == CCPreferences.CameraModes.Orthographic);
            if (bOrthographic != FPlatform.MainCamera.IsOrthographic) {
                CC.ActiveContext.CameraManager.UpdateOrthographic(bOrthographic);
                FitCurrentSelectionToView();
            }
        }





        public static void UpdateGraphicsQuality(CCPreferences.GraphicsQualityLevels eLevel)
        {
            string[] names = UnityEngine.QualitySettings.names;
            int idx = names.Length - 1 - (int)eLevel;   /// haaaayyck
            UnityEngine.QualitySettings.SetQualityLevel(idx, true);
        }







    }
}
