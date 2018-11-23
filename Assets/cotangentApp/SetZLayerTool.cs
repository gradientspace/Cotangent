using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;
using f3;

namespace cotangent
{
    public class SetZLayerToolBuilder : IToolBuilder
    {
        public bool IsSupported(ToolTargetType type, List<SceneObject> targets)
        {
            return (type == ToolTargetType.Scene);
        }

        public ITool Build(FScene scene, List<SceneObject> targets)
        {
            return new SetZLayerTool(scene);
        }
    }


    public class SetZLayerTool : BaseSurfacePointTool
    {
        static readonly public string Identifier = "set_z_layer";

        override public string Name { get { return "SetZLayer"; } }
        override public string TypeIdentifier { get { return Identifier; } }

        public SetZLayerTool(FScene scene) : base(scene)
        {
        }


        override public bool ObjectFilter(SceneObject so)
        {
            return so is DMeshSO;
        }

        override public void Begin(SceneObject so, Vector2d downPos, Ray3f downRay)
        {
            SORayHit hit;
            bool bHit = Scene.FindSORayIntersection(downRay, out hit, (x) => { return x is DMeshSO; } );
            if ( bHit) {
                Vector3f ptScene = Scene.ToSceneP(hit.hitPos);
                CC.Objects.CurrentLayer = CC.Objects.CurrentLayersInfo.GetLayerIndex(ptScene.y);
            }
        }

        override public void Update(Vector2d downPos, Ray3f downRay)
        {
            SORayHit hit;
            bool bHit = Scene.FindSORayIntersection(downRay, out hit, (x) => { return x is DMeshSO; } );
            if ( bHit) { 
                Vector3f ptScene = Scene.ToSceneP(hit.hitPos);
                CC.Objects.CurrentLayer = CC.Objects.CurrentLayersInfo.GetLayerIndex(ptScene.y);
            }
        }

    }
}
