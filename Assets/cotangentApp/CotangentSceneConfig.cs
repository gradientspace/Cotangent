using System;
using UnityEngine;
using System.Collections.Generic;
using f3;
using g3;
using gs;
using cotangent;

public class CotangentSceneConfig : BaseSceneConfig
{
    public GameObject VRCameraRig;

    public GameObject StartupPart;
    public GameObject StartupPrintHead;

    FContext context;
    public override FContext Context { get { return context; } }

    // Use this for initialization
    public override void Awake()
    {
        base.Awake();

        // if we need to auto-configure Rift vs Vive vs (?) VR, we need
        // to do this before any other F3 setup, because MainCamera will change
        // and we are caching that in a lot of places...
        if (AutoConfigVR) {
            VRCameraRig = gs.VRPlatform.AutoConfigureVR();
        }

        // add splash screen
        CCActions.ShowSplashScreen();

        // restore any settings
        SceneGraphConfig.RestorePreferences();
        CCPreferences.RestorePreferences();
        if (CCPreferences.CameraMode == CCPreferences.CameraModes.Orthographic)
            Camera.main.orthographic = true;

        // set up some defaults
        // this will move the ground plane down, but the bunnies will be floating...
        //SceneGraphConfig.InitialSceneTranslate = -4.0f * Vector3f.AxisY;
        SceneGraphConfig.InitialSceneTranslate = Vector3f.Zero;
        SceneGraphConfig.DefaultSceneCurveVisualDegrees = 0.5f;
        SceneGraphConfig.DefaultPivotVisualDegrees = 0.5f;
        SceneGraphConfig.DefaultAxisGizmoVisualDegrees = 7.5f;
        SceneGraphConfig.CameraPivotVisualDegrees = 0.3f;

        if (FPlatform.InUnityEditor())
            Util.DebugBreakOnDevAssert = false;     // throw exception on gDevAssert

        SceneOptions options = new SceneOptions();
        options.UseSystemMouseCursor = true;
        options.Use2DCockpit = true;
        options.ConstantSize2DCockpit = true;
        options.EnableTransforms = true;
        options.EnableCockpit = true;
        options.CockpitInitializer = new SetupPrintCockpit();

        options.EnableDefaultLighting = true;

        options.MouseCameraControls = new MayaExtCameraHotkeys() {
            MouseOrbitSpeed = 5.0f, MousePanSpeed = 0.01f, MouseZoomSpeed = 0.1f, UseAdaptive = true
        };
        options.SpatialCameraRig = VRCameraRig;

        options.DefaultGizmoBuilder = new AxisTransformGizmoBuilder() { ScaleSpeed = 0.03f, TranslateSpeed = 1.0f };

        // very verbose
        options.LogLevel = 4;

        CCMaterials.InitializeMaterials();

        context = new FContext();
        CotangentUI.Initialize(context);   // have to do this before cockpit is configured, which currently
                                           // happens automatically on .Start() (which is dumb...)
        context.Start(options);

        CCMaterials.SetupSceneMaterials(context);

        // if you had other gizmos, you would register them here
        //context.TransformManager.RegisterGizmoType("snap_drag", new SnapDragGizmoBuilder());
        //controller.TransformManager.SetActiveGizmoType("snap_drag");
        //context.TransformManager.RegisterGizmoType(CotangentTypes.SlicePlaneHeightGizmoType, new SlicePlaneHeightGizmoBuilder() {
        //    Factory = new SlicePlaneHeightGizmoBuilder.WidgetFactory(), GizmoVisualDegrees = 4.0f, GizmoLayer = FPlatform.GeometryLayer
        //});
        //context.TransformManager.AddTypeFilter(SlicePlaneHeightGizmoBuilder.MakeTypeFilter());

        // if you had other tools, you would register them here.
        context.ToolManager.RegisterToolType(DrawPrimitivesTool.Identifier, new DrawPrimitivesToolBuilder());
        context.ToolManager.RegisterToolType(SetZLayerTool.Identifier, new SetZLayerToolBuilder());
        CCActions.InitializeCotangentScene(context);
        CCActions.InitializePrintTools(context);
        CCActions.InitializeRepairTools(context);
        CCActions.InitializeModelTools(context);
        context.ToolManager.SetActiveToolType(DrawPrimitivesTool.Identifier, ToolSide.Right);

        // Set up standard scene lighting if requested
        if ( options.EnableDefaultLighting ) {
            GameObject lighting = GameObject.Find("SceneLighting");
            if (lighting == null)
                lighting = new GameObject("SceneLighting");
            SceneLightingSetup setup = lighting.AddComponent<SceneLightingSetup>();
            setup.Context = context;
            setup.LightDistance = 200; // related to total scene scale...
            setup.LightCount = 4;
            setup.ShadowLightCount = 1;
        }


        //GameObjectFactory.CurveRendererSource = new VectrosityCurveRendererFactory();


        // set up selection material
        context.Scene.SelectedMaterial = CCMaterials.SelectedMaterial;


        /*
         * Import elements of Unity scene that already exist into the FScene
         */

        // set up ground plane geometry (optional)
        GameObject boundsObject = GameObject.Find("PrintBed");
        if (boundsObject != null) {
            context.Scene.AddWorldBoundsObject(boundsObject);
            CC.PrinterBed = boundsObject;
        }

        CC.Initialize(context);


        if ( StartupPart != null ) {
            StartupPart.name = "Cylinder";
            DMeshSO startupSO = (DMeshSO)UnitySceneUtil.ImportExistingUnityGO(StartupPart, context.Scene, true, true, false,
                (mesh, material) => {
                    PrintMeshSO so = new PrintMeshSO();
                    return so.Create(mesh, CCMaterials.PrintMeshMaterial);
                }
            );
            GameObject.Destroy(StartupPart);

            CC.Objects.AddPrintMesh(startupSO as PrintMeshSO);
            CCActions.StartupObjectUUID = CC.Objects.PrintMeshes[0].UUID;
        }


        //if (StartupPrintHead != null) {
        //    SceneObject wrapSO = UnitySceneUtil.WrapAnyGameObject(StartupPrintHead, context, false);
        //    CC.PrintHeadSO = wrapSO;
        //    SceneUtil.SetVisible(wrapSO, false);
        //}

        Context.ActiveCamera.Manipulator().SceneOrbit(Context.Scene, context.ActiveCamera, -25, -25);

        CCActions.SwitchToViewMode(CCPreferences.StartupWorkspace, true);

        // enable drag-drop on windows when not in editor
        StartAnonymousCoroutine(enable_drag_drop());

        // import command-line args
        CCActions.DoArgumentsImport(Environment.GetCommandLineArgs());

        // start auto-update check
        if ( FPlatform.InUnityEditor() == false )
            StartAnonymousCoroutine(auto_update_check());

        // set window title
        FPlatform.SetWindowTitle(string.Format("cotangent {0}", CotangentVersion.CurrentVersionString));

        // show privacy dialog soon
        Context.RegisterNthFrameAction(100, CCActions.ShowPrivacyDialogIfRequired);
    }




    System.Collections.IEnumerator auto_update_check()
    {
        yield return new WaitForSeconds(2);
        CotangentVersion.DoAutoUpdateCheck();
    }


    System.Collections.IEnumerator enable_drag_drop()
    {
        yield return new WaitForSeconds(2);
        bool bEnableDragDrop = (FPlatform.InUnityEditor() == false && FPlatform.GetDeviceType() == FPlatform.fDeviceType.WindowsDesktop);
        bEnableDragDrop = true;
        if (bEnableDragDrop) {
            Context.RegisterNextFrameAction(() => {
                if ( FPlatform.InUnityEditor() ) 
                    DragDropHandler.Initialize();
                else
                    DragDropHandler.Initialize(WinAPI.GetCurrentThreadId(), "UnityWndClass");   // is this necessary?
                DragDropHandler.OnDroppedFilesEvent += CCActions.DoDragDropImport;
            });
        }
    }


    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        CCActions.ShutdownApp();
        DragDropHandler.Shutdown();
    }



    public override void Update()
    {
        base.Update();

        CC.Update();

        if ( CCState.ClipPlaneEnabled ) {
            Vector3f worldUp = CC.ActiveScene.ToWorldN( CCState.ClipPlaneFrameS.Z );
            Vector3f worldPos = CC.ActiveScene.ToWorldP(CCState.ClipPlaneFrameS.Origin);
            var shader_vec = new UnityEngine.Vector4(worldUp.x, worldUp.y, worldUp.z, worldUp.Dot(worldPos));
            UnityEngine.Shader.SetGlobalVector("_ClipPlaneEquation", shader_vec);
        }

        Vector3f sceneOriginWorld = CC.ActiveScene.ToWorldP(Vector3f.Zero);
        Shader.SetGlobalVector("_SceneOriginWorld", new Vector4(sceneOriginWorld.x, sceneOriginWorld.y, sceneOriginWorld.z) );
    }


}