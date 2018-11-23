using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using f3;
using g3;
using gs;

using UnityEngine;

namespace cotangent
{
    public class PrintAnimation : IAnimatable
    {
        public SceneObject PrintHeadSO;
        public float PrintHeadOffset = 10.0f;

        public float SpeedUnitsToMMPerSec = 1.0f / 60.0f;   // print units are in mm/min


        fLineGameObject LaserGO;
        PrintTempParticleSystem HeatParticles;


        GCodeFile GCode;
        bool isFinished = false;


        MakerbotInterpreter interp;
        IEnumerator<bool> active_anim;
        GCodeAnimationLister listener;

        double StartTime = 0;

        public void Begin(GCodeFile gcode)
        {
            GCode = gcode;

            listener = new GCodeAnimationLister();
            listener.OnBeginDepositF = OnBeginDepositF;
            listener.OnBeginTravelF = OnBeginTravelF;
            listener.OnMoveToAtTimeF = OnMoveToAtTimeF;
            listener.SpeedScale = SpeedUnitsToMMPerSec;

            interp = new MakerbotInterpreter();
            interp.AddListener(listener);
            active_anim = interp.InterpretInteractive(GCode, InterpretArgs.Default).GetEnumerator();
            isFinished = false;
            
            LaserGO = GameObjectFactory.CreateLineGO("laser", Colorf.VideoRed, 0.5f, LineWidthType.World);
            HeatParticles = new PrintTempParticleSystem(CC.ActiveScene.RootGameObject);

            StartTime = FPlatform.RealTime();
            CurrentPosition = Vector3d.Zero;
            PrevPosition = Vector3d.Zero;
            PrevTime = 0;
        }


        bool emitting = false;
        Vector3d CurrentPosition;

        Vector3d PrevPosition;
        double PrevTime;
        Vector3d NextPosition;
        double NextTime;

        public void Start()
        {
        }

        public void Update()
        {
            double time = FPlatform.RealTime() - StartTime;
            while (NextTime < time) {
                PrevPosition = NextPosition;
                PrevTime = NextTime;

                update_heat_display((Vector3f)PrevPosition, PrevTime);
                bool bContinue = active_anim.MoveNext();
                if (bContinue == false) {
                    isFinished = true;
                    return;
                } 
            }
            double dt = (time - PrevTime) / (NextTime - PrevTime);
            Vector3d interpPos = (1.0 - dt) * PrevPosition + (dt) * NextPosition;
            //CurrentPosition = NextPosition;
            //CurrentTime = NextTime;
            update_pos((Vector3f)interpPos, time);
        }


        public bool DeregisterNextFrame {
            get {
                return isFinished;
            }
        }



        void OnBeginTravelF()
        {
            LaserGO.SetVisible(false);
            emitting = false;
            HeatParticles.BeginTravel();
        }
        void OnBeginDepositF()
        {
            LaserGO.SetVisible(true);
            emitting = true;
            HeatParticles.BeginDeposit();
        }
        void OnMoveToAtTimeF(Vector3d newPos, double atTime)
        {
            NextPosition = newPos;
            NextTime = atTime;
        }


        void update_pos(Vector3f pos, double time)
        {
            Frame3f f = PrintHeadSO.GetLocalFrame(CoordSpace.SceneCoords);
            f.Origin = pos + PrintHeadOffset * Vector3f.AxisY;
            PrintHeadSO.SetLocalFrame(f, CoordSpace.SceneCoords);

            Vector3f worldNozzlePt = PrintHeadSO.GetScene().ToWorldP(f.Origin);
            Vector3f worldPathPt = PrintHeadSO.GetScene().ToWorldP(pos);

            LaserGO.SetStart(worldNozzlePt);
            LaserGO.SetEnd(worldPathPt);

            if ( emitting ) 
                CC.SetLayerFromZ(pos.y);

            update_heat_display(pos, time);
        }

        void update_heat_display(Vector3f posS, double time)
        {
            if (emitting) {
                posS += 0.02f * Vector3f.AxisY;
                HeatParticles.AddParticle(posS, (float)time);
            }
        }

    }




    public class GCodeAnimationLister : IGCodeListener
    {
        public Action OnBeginTravelF;
        public Action OnBeginDepositF;
        public Action<Vector3d, double> OnMoveToAtTimeF;

        public double SpeedScale = 1.0f;

        Vector3d CurrentPosition;
        double CurrentTime;

        public void Begin()
        {
            CurrentPosition = Vector3d.Zero;
            CurrentTime = 0;
        }
        public void End()
        {

        }

        public void BeginTravel()
        {
            if (OnBeginTravelF != null)
                OnBeginTravelF();
        }
        public void BeginDeposition()
        {
            if (OnBeginDepositF != null)
                OnBeginDepositF();
        }
        public void BeginCut()
        {
            // ignore
        }

        // for hacks
        public void CustomCommand(int code, object o) { }

        public void LinearMoveToAbsolute2d(LinearMoveData move) { }
        public void LinearMoveToRelative2d(LinearMoveData move) { }
        public void ArcToRelative2d(Vector2d pos, double radius, bool clockwise, double rate) { }

        public void LinearMoveToAbsolute3d(LinearMoveData move)
        {
            Vector3d nextPos = move.position;
            nextPos = MeshTransforms.ConvertZUpToYUp(nextPos);

            double dist = CurrentPosition.Distance(nextPos);
            double speed = move.rate * SpeedScale;
            double time = dist / speed;
            if (OnMoveToAtTimeF != null)
                OnMoveToAtTimeF(nextPos, CurrentTime + time);
            CurrentTime += time;
            CurrentPosition = nextPos;
        }
        public void LinearMoveToRelative3d(LinearMoveData move)
        {
            Vector3d nextPos = CurrentPosition + move.position;
            nextPos = MeshTransforms.ConvertZUpToYUp(nextPos);

            double dist = CurrentPosition.Distance(nextPos);
            double speed = move.rate * SpeedScale;
            double time = dist / speed;
            if (OnMoveToAtTimeF != null)
                OnMoveToAtTimeF(nextPos, CurrentTime + time);
            CurrentTime += time;
            CurrentPosition = nextPos;
        }
    }





    public class PrintTempParticleSystem
    {
        GameObject heatParticlesGO;
        ParticleSystem ps;

        GameObject sparksGO;
        ParticleSystem sparksPS;

        fPolylineGameObject trailGO;
        struct TrailVert
        {
            public Vector3f pos;
            public float time;
        }
        List<TrailVert> trailVerts = new List<TrailVert>(2048);
        Vector3f[] TrailPosArray()
        {
            Vector3f[] v = new Vector3f[trailVerts.Count];
            for (int i = 0; i < trailVerts.Count; ++i)
                v[i] = trailVerts[i].pos;
            return v;
        }

        bool in_deposit = false;

        public float TrailTime = 0.5f;

        //GameObject trailGO;
        //TrailRenderer trailRen;

        public PrintTempParticleSystem(GameObject parentGO)
        {
            heatParticlesGO = new GameObject("heatParticleSystem");
            ps = heatParticlesGO.AddComponent<ParticleSystem>();
            parentGO.AddChild(heatParticlesGO, false);

            Material heatMat = MaterialUtil.SafeLoadMaterial("HeatParticleMaterial");
            heatParticlesGO.GetComponent<ParticleSystemRenderer>().material =
                heatMat;
            //MaterialUtil.SafeLoadMaterial("Particles/Alpha Blended Premultiply");
            heatParticlesGO.SetLayer(FPlatform.WidgetOverlayLayer);
            heatParticlesGO.GetComponent<Renderer>().material.renderQueue = 4000;
            MaterialUtil.DisableShadows(heatParticlesGO);

            ps.Stop();


            sparksGO = UnityUtil.FindGameObjectByName("SparksPS");
            sparksGO.SetVisible(true);
            sparksPS = sparksGO.GetComponent<ParticleSystem>();
            sparksPS.Stop();
            parentGO.AddChild(sparksGO, false);


            fMaterial mat = MaterialUtil.CreateFlatMaterial(Colorf.VideoRed, 0.75f);

            trailGO = GameObjectFactory.CreatePolylineGO("trail", new List<Vector3f>(), mat, true, 0.5f, LineWidthType.World);
            trailGO.GetComponent<Renderer>().material.renderQueue = 3500;
            trailGO.SetCornerQuality(fCurveGameObject.CornerQuality.Minimal);
            trailGO.SetParent(parentGO);
            trailGO.SetLayer(FPlatform.WidgetOverlayLayer);
            MaterialUtil.DisableShadows(trailGO);

            //trailGO = new GameObject("heatTrail");
            //trailRen = trailGO.AddComponent<TrailRenderer>();
            //parentGO.AddChild(trailGO, false);

            //trailRen.material = MaterialUtil.CreateFlatMaterial(Colorf.VideoRed);
            //trailRen.material.renderQueue = 3500;
            //trailRen.minVertexDistance = 0.2f;
            //trailRen.numCornerVertices = 4;
            //trailRen.startWidth = 0.5f;
            //trailRen.endWidth = 0.1f;

            //trailRen.time = 2.0f;

        }



        public void BeginTravel()
        {
            sparksPS.Stop();
            in_deposit = false;
        }
        public void BeginDeposit()
        {
            ClearTrail();
            sparksPS.Play();
            in_deposit = true;
        }


        public void AddParticle(Vector3f posS, float time)
        {
            if (!in_deposit)
                return;

            //ParticleSystem.EmitParams p = new ParticleSystem.EmitParams() {
            //    position = posS,
            //    velocity = Vector3f.Zero,
            //    startSize3D = Vector3f.One,
            //    startLifetime = 3.0f,
            //    startColor = Colorf.VideoRed
            //};
            //ps.Emit(p, 1);

            sparksGO.SetLocalPosition(posS);

            int shift = 0;
            for ( int i = 0; i < trailVerts.Count; ++i ) {
                if (time - trailVerts[i].time > TrailTime)
                    shift++;
            }
            if (shift > 0)
                trailVerts.RemoveRange(0, shift);

            TrailVert newv = new TrailVert() { pos = posS, time = time };
            if (trailVerts.Count == 0) {
                trailVerts.Add(newv);
            } else { 
                TrailVert last = trailVerts[trailVerts.Count - 1];
                if (posS.Distance(last.pos) > 0.1f) {
                    trailVerts.Add(newv);
                } else {
                    trailVerts[trailVerts.Count - 1] = newv;
                }
            }

            trailGO.SetVertices(TrailPosArray());
            trailGO.SetLineWidth(0.1f, 0.5f);

            //trailGO.SetLocalPosition(posS);
        }


        public void ClearTrail()
        {
            //if (trailRen != null) 
            //    trailRen.Clear();

            if ( trailGO != null ) {
                trailGO.SetVertices(new List<Vector3f>());
                trailVerts.Clear();
            }
        }

    }


}
