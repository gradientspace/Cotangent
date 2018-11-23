using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using f3;
using gs;

namespace cotangent
{
    public static partial class CCActions
    {

        /// <summary>
        /// Translate objects so that their bbox rests on y=0 plane
        /// </summary>
        public static void MoveCurrentToPrintBed(bool bInteractive)
        {
            List<PrintMeshSO> targets = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>(true);
            if (targets.Count == 0)
                targets.AddRange(CC.Objects.PrintMeshes);
            if (targets.Count == 0)
                return;
            foreach (var meshSO in targets) {
                OrientationChanges.MoveToPrintBed(CC.ActiveScene, meshSO, false);
            }
            if (bInteractive)
                CC.ActiveScene.History.PushInteractionCheckpoint();
        }



        /// <summary>
        /// Translate objects so that their bbox center is at x=0, y=0
        /// </summary>
        public static void CenterCurrent(bool bInteractive)
        {
            List<PrintMeshSO> targets = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>(true);
            if (targets.Count == 0)
                targets.AddRange(CC.Objects.PrintMeshes);
            if (targets.Count == 0)
                return;
            OrientationChanges.Center(CC.ActiveScene, targets.Cast<DMeshSO>(), false);
            //foreach (var meshSO in targets) {
            //    OrientationChanges.Center(CC.ActiveScene, meshSO, false);
            //}
            if (bInteractive)
                CC.ActiveScene.History.PushInteractionCheckpoint();
        }



        /// <summary>
        /// flip between Z-up and Y-up
        /// </summary>
        public static void SwapCurrentUpDirections(bool bInteractive)
        {
            List<PrintMeshSO> targets = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>(true);
            if (targets.Count == 0)
                targets.AddRange(CC.Objects.PrintMeshes);
            if (targets.Count == 0)
                return;
            OrientationChanges.SwapUpDirection(CC.ActiveScene, targets);
            if (bInteractive)
                CC.ActiveScene.History.PushInteractionCheckpoint();
        }



        /// <summary>
        /// reverse object
        /// </summary>
        public static void Mirror(bool bInteractive)
        {
            List<PrintMeshSO> targets = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>(true);
            if (targets.Count == 0)
                targets.AddRange(CC.Objects.PrintMeshes);
            if (targets.Count == 0)
                return;
            foreach (var meshSO in targets)
                OrientationChanges.Mirror(CC.ActiveScene, meshSO, false);
            if (bInteractive)
                CC.ActiveScene.History.PushInteractionCheckpoint();
        }





        /// <summary>
        /// Translate objects so that their bbox center is at x=0, y=0
        /// </summary>
        public static void ResetCurrentPivots(bool bInteractive)
        {
            List<PrintMeshSO> targets = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>(true);
            if (targets.Count == 0)
                targets.AddRange(CC.Objects.PrintMeshes);
            if (targets.Count == 0)
                return;
            foreach (var meshSO in targets) {
                OrientationChanges.RecenterPivot(CC.ActiveScene, meshSO, PivotLocation.BaseCenter, false);
            }
            if (bInteractive)
                CC.ActiveScene.History.PushInteractionCheckpoint();
        }



    }
}
