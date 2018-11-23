using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using g3;
using f3;
using gs;
using cotangent;

public class ActiveToolPanel : MonoBehaviour
{
    static GameObject rootGO;
    public static void SetVisible(bool bVisible)
    {
        if (rootGO == null)
            rootGO = UnityUtil.FindGameObjectByName("ActiveToolPanel");
        rootGO.SetVisible(bVisible);
        CC.ActiveContext.RegisterNthFrameAction(2, () => {
            rootGO.GetComponent<ActiveToolPanel>().UpdateOnToolChange();
        });
    }
    public static bool IsVisible {
        get {
            if (rootGO == null)
                rootGO = UnityUtil.FindGameObjectByName("ActiveToolPanel");
            return rootGO.IsVisible();
        }
    }


    GameObject CurrentToolPanel;



    // Use this for initialization
    public void Start()
	{
        SetVisible(false);
    }


    public void UpdateOnToolChange()
    {
        if (CurrentToolPanel != null) {
            CurrentToolPanel.Destroy();
            CurrentToolPanel = null;
        }

        ITool curTool = CC.ActiveContext.ToolManager.ActiveRightTool;
        if (curTool == null)
            return;

        if (curTool is GenerateClosedMeshTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MakeClosedToolSettings"));
        } else if (curTool is WeldEdgesTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("WeldEdgesToolSettings"));
        } else if (curTool is RemoveHiddenFacesTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("RemoveHiddenFacesToolSettings"));
        } else if (curTool is ReprojectTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("ReprojectToolSettings"));
        } else if (curTool is RemeshTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("RemeshToolSettings"));
        } else if (curTool is ReduceTool ) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("ReduceToolSettings"));
        } else if (curTool is SeparateSolidsTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("SeparateSolidsToolSettings"));
        } else if (curTool is CombineMeshesTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("CombineMeshesToolSettings"));
        } else if (curTool is MeshEditorTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MeshEditorToolSettings"));
        } else if (curTool is PlaneCutTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("PlaneCutToolSettings"));
        } else if (curTool is RepairOrientationTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("RepairOrientationToolSettings"));
        } else if (curTool is MeshAutoRepairTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MeshAutoRepairToolSettings"));
        } else if (curTool is FillHolesTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("FillHolesToolSettings"));
        } else if (curTool is MeshShellTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MeshShellToolSettings"));
        } else if (curTool is MeshHollowTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MeshHollowToolSettings"));
        } else if (curTool is MeshMorphologyTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MeshMorphologyToolSettings"));
        } else if (curTool is MeshWrapTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MeshWrapToolSettings"));
        } else if (curTool is MeshVoxelBooleanTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MeshVoxelBooleanToolSettings"));
        } else if (curTool is MeshVoxelBlendTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MeshVoxelBlendToolSettings"));
        } else if (curTool is AddHoleTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("AddHoleToolSettings"));

        } else if (curTool is GenerateShapeTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("GenerateShapeToolSettings"));

        } else if (curTool is SetDimensionsTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("SetDimensionsToolSettings"));

        } else if (curTool is PurgeSpiralTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("PurgeSpiralToolSettings"));
        } else if (curTool is BrimTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("BrimToolSettings"));
        } else if (curTool is GenerateBlockSupportsTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("BlockSupportsToolSettings"));
        } else if (curTool is GenerateGraphSupportsTool) {
            CurrentToolPanel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("GraphSupportsToolSettings"));
        }

        if (CurrentToolPanel != null) {
            this.gameObject.AddChild(CurrentToolPanel, false);
            CurrentToolPanel.transform.SetAsFirstSibling();
        }
    }


    public void Update()
    {
 
    }


    void on_show()
    {
    }


    void on_hide()
    {
    }



}
