using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using f3;
using g3;
using gs;

namespace cotangent
{

    class SetupPrintCockpit : ICockpitInitializer
    {


        public void Initialize(Cockpit cockpit)
        {
            cockpit.Name = "modelCockpit";


            // Configure how the cockpit moves

            //cockpit.PositionMode = Cockpit.MovementMode.TrackPosition;
            // [RMS] use orientation mode to make cockpit follow view orientation.
            //  (however default widgets below are off-screen!)
            cockpit.PositionMode = Cockpit.MovementMode.TrackOrientation;



            BoxContainer screenContainer = new BoxContainer(new Cockpit2DContainerProvider(cockpit));
            PinnedBoxes2DLayoutSolver screenLayout = new PinnedBoxes2DLayoutSolver(screenContainer);
            PinnedBoxesLayout layout = new PinnedBoxesLayout(cockpit, screenLayout) {
                StandardDepth = 2.0f
            };
            cockpit.AddLayout(layout, "2D", true);



            Func<string, float, HUDLabel> MakeButtonF = (label, buttonW) => {
                HUDLabel button = new HUDLabel() {
                    Shape = CotangentUI.MakeMenuButtonRect(buttonW, CotangentUI.MenuButtonHeight),
                    TextHeight = CotangentUI.MenuButtonTextHeight,
                    AlignmentHorz = HorizontalAlignment.Center,
                    BackgroundColor = CotangentUI.ButtonBGColor, TextColor = CotangentUI.ButtonTextColor, Text = label,
                    EnableBorder = true, BorderWidth = CotangentUI.StandardButtonBorderWidth, BorderColor = CotangentUI.ButtonTextColor
                };
                button.Create();
                button.Name = label;
                button.Enabled = true;
                return button;
            };



            Vector2f progressOffsetY = 4 * CotangentUI.PixelScale * Vector2f.AxisY;
            HUDRadialProgress slicerProgress = new HUDRadialProgress() {
                Radius = 18 * CotangentUI.PixelScale
            };
            slicerProgress.Create();
            slicerProgress.Name = "slicer_progress";
            int MAX_PROGRESS = 1000;
            slicerProgress.MaxProgress = MAX_PROGRESS;
            CC.SlicingProgressEvent += (status) => {
                if (status.bFailed) {
                    double t = 0.5 * (double)status.curProgress / (double)status.maxProgress;
                    slicerProgress.Progress = (int)(t * MAX_PROGRESS);
                    slicerProgress.CompletedColor = Colorf.VideoRed;
                } else {
                    double t = 0.5 * (double)status.curProgress / (double)status.maxProgress;
                    slicerProgress.Progress = (int)(t * MAX_PROGRESS);
                    slicerProgress.CompletedColor = Colorf.BlueMetal;
                }
            };
            CC.ToolpathProgressEvent += (status) => {
                if (status.bFailed) {
                    double t = 0.5 + 0.5*(double)status.curProgress / (double)status.maxProgress;
                    slicerProgress.Progress = (int)(t * MAX_PROGRESS);
                    slicerProgress.CompletedColor = Colorf.VideoRed;
                } else {
                    double t = 0.5 + 0.5 * (double)status.curProgress / (double)status.maxProgress;
                    if (status.curProgress == 0 && status.maxProgress == 1)
                        t = 0;
                    slicerProgress.Progress = (int)(t * MAX_PROGRESS);
                    slicerProgress.CompletedColor = (status.curProgress == status.maxProgress) ? Colorf.LightGreen : Colorf.BlueMetal;
                }
            };
            layout.Add(slicerProgress, new LayoutOptions() {
                Flags = LayoutFlags.None,
                PinSourcePoint2D = LayoutUtil.BoxPointF(slicerProgress, BoxPosition.CenterBottom),
                PinTargetPoint2D = LayoutUtil.BoxPointF(screenContainer, BoxPosition.CenterBottom, progressOffsetY)
            });



            HUDButton progressClick = new HUDButton() {
                Shape = new HUDShape(HUDShapeType.Disc, slicerProgress.Radius)
            };
            fMaterial normalMaterial = MaterialUtil.CreateFlatMaterialF(Colorf.White, 0);
            fMaterial pauseMaterial = MaterialUtil.CreateTransparentImageMaterialF("icons/progress_pause");
            fMaterial pausedMaterial = MaterialUtil.CreateTransparentImageMaterialF("icons/progress_play");
            if (CCPreferences.ActiveSlicingUpdateMode == CCPreferences.SlicingUpdateModes.ImmediateSlicing)
                progressClick.Create(normalMaterial, null, pauseMaterial);
            else
                progressClick.Create(pausedMaterial, null, null);
            progressClick.Name = "progress_click";
            layout.Add(progressClick, new LayoutOptions() {
                Flags = LayoutFlags.None,
                DepthShift = -0.1f,
                PinSourcePoint2D = LayoutUtil.BoxPointF(progressClick, BoxPosition.CenterBottom),
                PinTargetPoint2D = LayoutUtil.BoxPointF(screenContainer, BoxPosition.CenterBottom, progressOffsetY)
            });
            progressClick.OnClicked += (o, e) => {
                if ( CCPreferences.ActiveSlicingUpdateMode == CCPreferences.SlicingUpdateModes.ImmediateSlicing ) {
                    CCPreferences.ActiveSlicingUpdateMode = CCPreferences.SlicingUpdateModes.SliceOnDemand;
                    progressClick.StandardMaterial = pausedMaterial;
                    progressClick.HoverMaterial = null;
                } else {
                    CCPreferences.ActiveSlicingUpdateMode = CCPreferences.SlicingUpdateModes.ImmediateSlicing;
                    progressClick.StandardMaterial = normalMaterial;
                    progressClick.HoverMaterial = pauseMaterial;
                }
                if (CC.Toolpather.ToolpathsValid == false) {
                    // not sure why we have to invalidate slicing here, but if we don't toolpath
                    // computation will not stop when we pause...
                    CC.Slicer.InvalidateSlicing();
                    //CC.InvalidateToolPaths();
                }
            };


            CotangentUI.PrintViewHUDItems = new List<HUDStandardItem>() { slicerProgress, progressClick };

            screenLayout.RecomputeLayout();


            // Configure interaction behaviors
            //   - below we add behaviors for mouse, gamepad, and spatial devices (oculus touch, etc)
            //   - keep in mind that Tool objects will register their own behaviors when active

            // setup key handlers (need to move to behavior...)
            cockpit.AddKeyHandler(new CotangentKeyHandler(cockpit.Context));

            // these behaviors let us interact with UIElements (ie left-click/trigger, or either triggers for Touch)
            cockpit.InputBehaviors.Add(new Mouse2DCockpitUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRMouseUIBehavior(cockpit.Context) { Priority = 1 });

            // selection / multi-selection behaviors
            // Note: this custom behavior implements some selection redirects that we use in various parts of Archform
            cockpit.InputBehaviors.Add(new MouseMultiSelectBehavior(cockpit.Context) { Priority = 10 });

            // left click-drag to tumble, and left click-release to de-select
            cockpit.InputBehaviors.Add(new MouseClickDragSuperBehavior() {
                Priority = 100,
                DragBehavior = new MouseViewRotateBehavior(cockpit.Context) { Priority = 100, RotateSpeed = 3.0f },
                ClickBehavior = new MouseDeselectBehavior(cockpit.Context) { Priority = 999 }
            });

            // also right-click-drag to tumble
            cockpit.InputBehaviors.Add(new MouseViewRotateBehavior(cockpit.Context) {
                Priority = 100, RotateSpeed = 3.0f,
                ActivateF = MouseBehaviors.RightButtonPressedF, ContinueF = MouseBehaviors.RightButtonDownF
            });

            // middle-click-drag to pan
            cockpit.InputBehaviors.Add(new MouseViewPanBehavior(cockpit.Context) {
                Priority = 100, PanSpeed = 0.01f, Adaptive = true,
                ActivateF = MouseBehaviors.MiddleButtonPressedF, ContinueF = MouseBehaviors.MiddleButtonDownF
            });


            cockpit.OverrideBehaviors.Add(new MouseWheelZoomBehavior(cockpit) { Priority = 100, ZoomScale = 0.2f, Adaptive = true });

            // touch input
            cockpit.InputBehaviors.Add(new TouchUIBehavior(cockpit.Context) { Priority = 1 });
            cockpit.InputBehaviors.Add(new Touch2DCockpitUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new TouchViewManipBehavior(cockpit.Context) {
                Priority = 999, TouchZoomSpeed = 0.1f, TouchPanSpeed = 0.03f
            });

        }
    }












    public class CotangentKeyHandler : IShortcutKeyHandler
    {
        FContext context;
        public CotangentKeyHandler(FContext c)
        {
            context = c;
        }
        public bool HandleShortcuts()
        {
            bool bShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool bCtrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // ESCAPE CLEARS ACTIVE TOOL OR SELECTION
            if (Input.GetKeyUp(KeyCode.Escape)) {
                if ( CCActions.IsPreferencesVisible() ) {
                    CCActions.HidePreferencesDialog();
                } else if (CCActions.InTool) {
                    CCActions.CancelCurrentTool();
                } else if (context.Scene.Selected.Count > 0) {
                    context.Scene.ClearSelection();
                }
                return true;

            } else if (Input.GetKeyUp(KeyCode.Delete)) {
                if (CCActions.InTool == false)
                    CCActions.DeleteSelectedObjects(true);
                return true;


                // CENTER TARGET (??)
            } else if (Input.GetKeyUp(KeyCode.C)) {
                Ray3f cursorRay = context.MouseController.CurrentCursorWorldRay();
                AnyRayHit hit = null;
                if (context.Scene.FindSceneRayIntersection(cursorRay, out hit)) {
                    context.ActiveCamera.Animator().AnimatePanFocus(hit.hitPos, CoordSpace.WorldCoords, 0.3f);
                }
                return true;

                // DROP A COPY
            } else if (Input.GetKeyUp(KeyCode.D)) {
                if ( CCActions.InTool == false )
                    CCActions.DuplicateSelectedObjects(true);
                return true;

                // Mesh Editor
            } else if (Input.GetKeyUp(KeyCode.E)) {
                if (bCtrlDown)
                    CCActions.DoMeshExport();
                else
                    CCActions.BeginTool(MeshEditorTool.Identifier);
                return true;

                // TOGGLE FRAME TYPE
            } else if (Input.GetKeyUp(KeyCode.F)) {
                FrameType eCur = context.TransformManager.ActiveFrameType;
                context.TransformManager.ActiveFrameType = (eCur == FrameType.WorldFrame)
                    ? FrameType.LocalFrame : FrameType.WorldFrame;
                return true;

                // Fill Holes
            } else if (Input.GetKeyUp(KeyCode.H)) {
                CCActions.BeginTool(FillHolesTool.Identifier);
                return true;

                // Autorepair
            } else if (Input.GetKeyUp(KeyCode.I)) {
                CCActions.BeginTool(MeshAutoRepairTool.Identifier);
                return true;

                // CENTER AND ON BED
            } else if (Input.GetKeyUp(KeyCode.N)) {
                if (CCActions.InTool == false) {
                    CCActions.AcceptAndExitCurrentTool();
                    CCActions.MoveCurrentToPrintBed(false);
                    CCActions.CenterCurrent(true);
                }
                return true;

                // Remesh or Simplify
            } else if (Input.GetKeyUp(KeyCode.R)) {
                if ( bShiftDown )
                    CCActions.BeginTool(ReduceTool.Identifier);
                else
                    CCActions.BeginTool(RemeshTool.Identifier);
                return true;

                // SET UNITS/SIZE
            } else if (Input.GetKeyUp(KeyCode.U)) {
                CCActions.BeginTool(SetDimensionsTool.Identifier);
                return true;

                // VISIBILITY  (V HIDES, SHIFT+V SHOWS)
            } else if (Input.GetKeyUp(KeyCode.V)) {
                // show/hide (should be abstracted somehow?? instead of directly accessing GOs?)
                if (bShiftDown) {
                    foreach (SceneObject so in context.Scene.SceneObjects)
                        so.RootGameObject.Show();
                } else {
                    foreach (SceneObject so in context.Scene.Selected)
                        so.RootGameObject.Hide();
                    context.Scene.ClearSelection();
                }
                return true;

                // UNDO
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.Z)) {
                context.Scene.History.InteractiveStepBack();
                return true;
            } else if (Input.GetKeyUp(KeyCode.Backspace)) {
                context.Scene.History.InteractiveStepBack();
                return true;

                // REDO
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.Y)) {
                context.Scene.History.InteractiveStepForward();
                return true;

                // Toggle Y/Z Up
            } else if (bShiftDown && Input.GetKeyUp(KeyCode.Z)) {
                if (CCActions.InTool == false) {
                    CCActions.AcceptAndExitCurrentTool();
                    CCActions.SwapCurrentUpDirections(true);
                }
                return true;


            } else if (Input.GetKeyUp(KeyCode.UpArrow)) {
                int step = (bShiftDown) ? 10 : 1;
                CC.Objects.CurrentLayer = CC.Objects.CurrentLayer + step;
                return true;
            } else if (Input.GetKeyUp(KeyCode.DownArrow)) {
                int step = (bShiftDown) ? 10 : 1;
                CC.Objects.CurrentLayer = CC.Objects.CurrentLayer - step;
                return true;


            } else if (Input.GetKeyUp(KeyCode.A)) {
                CCActions.AcceptAndExitCurrentTool();
                return true;

            } else if (Input.GetKeyUp(KeyCode.Q) && bCtrlDown) {
                FPlatform.QuitApplication();
                return true;

            } else if (Input.GetKeyUp(KeyCode.S)) {
                if (bCtrlDown && bShiftDown)
                    CCActions.SaveCurrentSceneAs();
                else if (bCtrlDown)
                    CCActions.SaveCurrentSceneOrSaveAs();
                return true;

            } else if (Input.GetKeyUp(KeyCode.X)) {
                if (FPlatform.InUnityEditor()) {
                    //CC.ActiveContext.Scene.ClearHistory();
                    CC.ActiveContext.Scene.History.DebugPrint();
                }
                return true;


            } else if (bShiftDown && Input.GetKeyUp(KeyCode.Alpha1) ) {
                if (FPlatform.InUnityEditor() == false && bCtrlDown == false)
                    return true;
                string lastFile = FPlatform.GetPrefsString("LastImportPath", "");
                if (lastFile != "") {
                    if (lastFile.EndsWith(".cota")) {
                        CCActions.DoFileOpen(lastFile, false, (str) => {
                            CC.ActiveScene.ClearHistory();
                        });
                    } else {
                        CCActions.ClearScene();
                        CCActions.DoFileImport(lastFile, false, (str) => {
                            CC.ActiveScene.ClearHistory();
                        });
                    }
                }
                return true;


            } else if (Input.GetKeyUp(KeyCode.W)) {
                CCActions.WireframeEnabled = ! CCActions.WireframeEnabled;
                return true;

            } else if (Input.GetKeyUp(KeyCode.Alpha1)) {
                CCActions.SwitchToViewMode(AppViewMode.PrintView);
                return true;
            } else if (Input.GetKeyUp(KeyCode.Alpha2)) {
                CCActions.SwitchToViewMode(AppViewMode.RepairView);
                return true;
            } else if (Input.GetKeyUp(KeyCode.Alpha3)) {
                CCActions.SwitchToViewMode(AppViewMode.ModelView);
                return true;



            } else
                return false;
        }
    }

}