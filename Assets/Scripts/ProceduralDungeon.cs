using UnityEngine;
using System.Collections.Generic;
namespace AccordingToJames
{
    // Presentation only: GameModel remains the authority for movement and collisions.
    public sealed class ProceduralDungeon : MonoBehaviour
    {
        readonly List<GameObject> pieces=new List<GameObject>();
        readonly Dictionary<string,Transform> gates=new Dictionary<string,Transform>();
        Material stone,floor,iron,wood; Mesh cube; int key=-1; Transform chestLid,stairsLock,secret;
        public static Vector3 Position(float x,float y,float height=0)=>new Vector3((x-480)/100,height,(300-y)/100);
        void Awake(){cube=Cube();}
        Material Surface(Color color,int seed,bool grain=false)
        {
            var tex=new Texture2D(64,64);var rng=new System.Random(seed);var pixels=new Color[4096];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++){float v=.84f+(float)rng.NextDouble()*.23f;if(grain)v+=Mathf.Sin(x*.8f+y*.05f)*.08f; pixels[y*64+x]=color*v;}
            tex.SetPixels(pixels);tex.Apply();tex.filterMode=FilterMode.Point;tex.wrapMode=TextureWrapMode.Repeat;
            var m=new Material(Resources.Load<Material>("Materials/DungeonSurface"));m.color=Color.white;m.mainTexture=tex;m.SetFloat("_Glossiness",.12f);return m;
        }
        Transform Box(string name,Vector3 p,Vector3 size,Material material,Transform parent=null)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent?parent:transform,false);go.transform.localPosition=p;go.transform.localScale=size;go.GetComponent<MeshFilter>().sharedMesh=cube;go.GetComponent<MeshRenderer>().sharedMaterial=material;pieces.Add(go);return go.transform;
        }
        public void Refresh(GameModel game)
        {
            int next=game.floor*100+game.room;if(next!=key){key=next;Build(game);}
            foreach(var gate in gates){float target=game.DoorOpen(gate.Key)?1.5f:0;var p=gate.Value.localPosition;p.y=Mathf.MoveTowards(p.y,target,Time.deltaTime*3);gate.Value.localPosition=p;}
            if(chestLid)chestLid.localRotation=Quaternion.Euler(game.Current.chestOpened?-105:0,0,0);
            if(stairsLock)stairsLock.gameObject.SetActive(!game.AllCleared);
            if(secret)secret.gameObject.SetActive(game.Current.secretKnown&&!game.Current.secretUsed);
        }
        void Build(GameModel g)
        {
            foreach(var piece in pieces)if(piece)Destroy(piece);pieces.Clear();gates.Clear();chestLid=stairsLock=secret=null;
            foreach(var m in new[]{stone,floor,iron,wood})if(m){if(m.mainTexture)Destroy(m.mainTexture);Destroy(m);}
            Color[] palette={new Color(.25f,.31f,.36f),new Color(.38f,.27f,.2f),new Color(.28f,.25f,.34f)};
            stone=Surface(palette[g.Dungeon],g.seed+g.floor);floor=Surface(palette[g.Dungeon]*1.22f,g.seed);iron=Surface(new Color(.16f,.19f,.21f),9);wood=Surface(new Color(.35f,.19f,.07f),7,true);

            for(int z=0;z<10;z++)for(int x=0;x<22;x++){float px=-3.85f+(x+.5f)*7.7f/22,pz=-1.75f+(z+.5f)*.35f;Box("Floor slab",new Vector3(px,-.08f,pz),new Vector3(7.7f/22-.015f,.15f,.335f),floor);}
            foreach(var side in new[]{"up","down","left","right"}){
                Wall(g,side);
                if(!g.Neighbors(g.room).ContainsKey(side))continue;
                var p=side=="up"?new Vector3(0,-.08f,2.2f):side=="down"?new Vector3(0,-.08f,-2.2f):new Vector3(side=="left"?-4.3f:4.3f,-.08f,-.1f);
                Box("Door threshold",p,side=="up"||side=="down"?new Vector3(.84f,.15f,1):new Vector3(1,.15f,.68f),floor);
            }
            if(g.Current.secret)secret=Box("Revealed loose stone",Position(GameModel.SecretX,125,.3f),new Vector3(.5f,.6f,.08f),wood);
            foreach(var b in g.Current.obstacles)Box("Solid obstacle",Position(b.x+b.w/2,b.y+b.h/2,.3f),new Vector3(b.w/100,.6f,b.h/100),wood);
            foreach(var b in g.Current.spikes){Box("Trap plate",Position(b.x+b.w/2,b.y+b.h/2,.02f),new Vector3(b.w/100,.05f,b.h/100),iron);for(int i=0;i<7;i++)Box("Trap teeth",Position(b.x+7+i*8,b.y+b.h/2,.1f),new Vector3(.035f,.18f,.035f),iron);}
            if(g.Current.chest){var p=Position(205,235,.25f);Box("Chest body",p,new Vector3(.8f,.5f,.42f),wood);chestLid=new GameObject("Chest hinge").transform;chestLid.SetParent(transform,false);chestLid.localPosition=p+new Vector3(0,.25f,.21f);pieces.Add(chestLid.gameObject);Box("Chest lid",new Vector3(0,.06f,-.21f),new Vector3(.82f,.12f,.44f),wood,chestLid);Box("Chest clasp",p+new Vector3(0,.06f,-.23f),new Vector3(.12f,.2f,.05f),iron);}
            if(g.Current.shrine){var p=Position(710,420);Box("Shrine plinth",p+Vector3.up*.18f,new Vector3(.62f,.36f,.34f),stone);Box("Shrine pillar",p+Vector3.up*.63f,new Vector3(.32f,.55f,.25f),iron);Lamp(p+Vector3.up*.9f,new Color(.2f,1,.8f),1.8f);}
            if(g.room==g.rooms.Count-1){for(int i=0;i<5;i++)Box("Descending stair",Position(805,200+i*8,-.04f-i*.08f),new Vector3(.8f,.1f,.1f),stone);stairsLock=Box("Stair barrier",Position(805,200,.18f),new Vector3(.84f,.12f,.65f),iron);}
            for(int i=0;i<4;i++){var p=new Vector3(-3+i*2,.95f,1.8f);Box("Torch bracket",p,new Vector3(.09f,.3f,.13f),iron);Lamp(p+Vector3.up*.2f,new Color(1,.52f,.16f),2.1f);}
        }
        void Lamp(Vector3 p,Color color,float range){var go=new GameObject("Dungeon light",typeof(Light));go.transform.SetParent(transform,false);go.transform.localPosition=p;var l=go.GetComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=2;l.range=range;pieces.Add(go);}
        void Wall(GameModel g,string side)
        {
            bool horizontal=side=="up"||side=="down";float length=horizontal?7.7f:3.5f;bool opening=g.Neighbors(g.room).ContainsKey(side);float height=side=="down"?.36f:1.1f;int rows=side=="down"?1:3;
            for(int r=0;r<rows;r++)for(float a=-length/2;a<length/2;a+=.48f){float center=a+.24f;if(opening&&Mathf.Abs(center)<.52f)continue;var p=horizontal?new Vector3(center,.18f+r*.36f,side=="up"?1.84f:-1.84f):new Vector3(side=="left"?-3.94f:3.94f,.18f+r*.36f,center);Box("Brick",p,horizontal?new Vector3(.46f,.34f,.2f):new Vector3(.2f,.34f,.46f),stone);}
            if(!opening)return;var gate=new GameObject(side+" portcullis").transform;gate.SetParent(transform,false);pieces.Add(gate.gameObject);gates[side]=gate;var baseP=horizontal?new Vector3(0,0,side=="up"?1.84f:-1.84f):new Vector3(side=="left"?-3.94f:3.94f,0,-.1f);
            for(int i=0;i<7;i++){var p=baseP+Vector3.up*.55f+(horizontal?Vector3.right:Vector3.forward)*((i-3)*.13f);Box("Iron bar",p,new Vector3(.035f,1.1f,.035f),iron,gate);}
            Box("Gate crossbar",baseP+Vector3.up*.85f,horizontal?new Vector3(.94f,.06f,.08f):new Vector3(.08f,.06f,.94f),iron,gate);
        }
        static Mesh Cube(){var mesh=new Mesh();var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();Vector3[] normals={Vector3.forward,Vector3.back,Vector3.left,Vector3.right,Vector3.up,Vector3.down};foreach(var n in normals){var u=Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.Cross(Vector3.up,n);var v=Vector3.Cross(n,u);int start=vertices.Count;foreach(var t in new[]{new Vector2(-1,-1),new Vector2(1,-1),new Vector2(1,1),new Vector2(-1,1)}){vertices.Add((n+u*t.x+v*t.y)*.5f);uv.Add((t+Vector2.one)*.5f);}triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});}mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
    }
}
