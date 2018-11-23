#pragma warning disable 414
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;

public class ViewOptionsPanelSetup : MonoBehaviour
{
    Button frameButton;
    Button wireframeButton;
    Button boundariesButton;
    Button cavitiesButton;
    Button slicesButton;
    Button clipPlaneButton;
    Button pathsButton;

    // Use this for initialization
    public void Start()
	{
        var anybutton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "ShowSlicesToggleButton", null);

        ColorBlock normalColor = anybutton.colors;
        ColorBlock onColor = normalColor; onColor.normalColor = Colorf.LightGreen; onColor.highlightedColor = Colorf.LightGreen;
        ColorBlock offColor = normalColor; offColor.normalColor = CotangentUI.DisabledButtonColor; offColor.highlightedColor = CotangentUI.DisabledButtonColor;

        frameButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "FrameToggle", on_frame_clicked);

        slicesButton = UnityUIUtil.FindButtonAndAddToggleBehavior(this.gameObject, "ShowSlicesToggleButton",
            () => { return CC.Slicer.ShowSlicePolylines; },
            (bSet) => { CC.Slicer.ShowSlicePolylines = bSet; },
            (bVal, btn) => { btn.colors = (bVal) ? onColor : offColor; });

        pathsButton = UnityUIUtil.FindButtonAndAddToggleBehavior(this.gameObject, "ShowToolpathsToggle",
            () => { return CC.Objects.ShowToolpaths; },
            (bSet) => { CC.Objects.ShowToolpaths = bSet; },
            (bVal, btn) => { btn.colors = (bVal) ? onColor : offColor; });

        clipPlaneButton = UnityUIUtil.FindButtonAndAddToggleBehavior(this.gameObject, "EnableClipPlaneToggle",
            () => { return CCActions.ClipPlaneEnabled; },
            (bSet) => { CCActions.ClipPlaneEnabled = bSet; },
            (bVal, btn) => { btn.colors = (bVal) ? onColor : offColor; });

        wireframeButton = UnityUIUtil.FindButtonAndAddToggleBehavior(this.gameObject, "WireframeToggle",
            () => { return CCActions.WireframeEnabled; },
            (bSet) => { CCActions.WireframeEnabled = bSet; },
            (bVal, btn) => { btn.colors = (bVal) ? onColor : offColor; }, true);

        boundariesButton = UnityUIUtil.FindButtonAndAddToggleBehavior(this.gameObject, "BoundariesToggle",
            () => { return CC.MeshAnalysis.EnableMeshBoundaries; },
            (bSet) => { CC.MeshAnalysis.EnableMeshBoundaries = bSet; },
            (bVal, btn) => { btn.colors = (bVal) ? onColor : offColor; }, true);

        cavitiesButton = UnityUIUtil.FindButtonAndAddToggleBehavior(this.gameObject, "CavitiesToggle",
            () => { return CC.MeshAnalysis.EnableCavities; },
            (bSet) => { CC.MeshAnalysis.EnableCavities = bSet; },
            (bVal, btn) => { btn.colors = (bVal) ? onColor : offColor; }, true);
    }


    public void on_frame_clicked()
    {
        var frameType = CC.ActiveContext.TransformManager.ActiveFrameType;
        if ( frameType == FrameType.LocalFrame ) {
            CC.ActiveContext.TransformManager.ActiveFrameType = FrameType.WorldFrame;
        } else {
            CC.ActiveContext.TransformManager.ActiveFrameType = FrameType.LocalFrame;
        }
    }


    int curViewMode = -1;

    int curFrameType = -1;
    UnityUIUtil.AutoSprite local_frame_sprite = new UnityUIUtil.AutoSprite("icons/frame_local");
    UnityUIUtil.AutoSprite world_frame_sprite = new UnityUIUtil.AutoSprite("icons/frame_world");

    public void Update()
    {
        if (curViewMode != (int)CCActions.CurrentViewMode) {
            curViewMode = (int)CCActions.CurrentViewMode;
            slicesButton.gameObject.SetVisible(CCActions.CurrentViewMode == AppViewMode.PrintView && CC.SHOW_DEVELOPMENT_FEATURES );
            clipPlaneButton.gameObject.SetVisible(CC.SHOW_DEVELOPMENT_FEATURES);
            pathsButton.gameObject.SetVisible(CCActions.CurrentViewMode == AppViewMode.PrintView);
        }

        if ( curFrameType != (int)CC.ActiveContext.TransformManager.ActiveFrameType ) {
            Image img = frameButton.gameObject.FindChildByName("Image", true).GetComponent<Image>();
            switch (CC.ActiveContext.TransformManager.ActiveFrameType) {
                case FrameType.WorldFrame:
                    img.sprite = world_frame_sprite.Sprite;
                    break;
                case FrameType.LocalFrame:
                    img.sprite = local_frame_sprite.Sprite;
                    break;
            }
            curFrameType = (int)CC.ActiveContext.TransformManager.ActiveFrameType;
        }
    }




}
