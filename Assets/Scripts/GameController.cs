using UnityEngine;
using System;
using System.Collections.Generic;

namespace AccordingToJames
{
    public class GameController : MonoBehaviour
    {
        GameModel game;SpriteAtlas art;Chiptunes music;Camera cam;
        readonly List<SpriteRenderer> pool=new List<SpriteRenderer>();int used;
        bool paused,settingsOpen,mapOpen;string toast="",panel="title";float toastLife;
        float initial=1,volume=.35f;bool reducedMotion;float guiScale,offsetX,offsetY;GUIStyle heading,label,small,button;
        int smokeFrames;bool smoke;string captureDir;
        void Start(){var args=Environment.GetCommandLineArgs();smoke=Array.IndexOf(args,"--self-test")>=0;int capture=Array.IndexOf(args,"--capture-dir");if(capture>=0&&capture+1<args.Length){captureDir=args[capture+1];System.IO.Directory.CreateDirectory(captureDir);}if(smoke)StartRun();}
        void CaptureWorld(string name){var target=new RenderTexture(960,600,24);var previous=RenderTexture.active;var oldRect=cam.pixelRect;cam.targetTexture=target;cam.rect=new Rect(0,0,1,1);cam.Render();RenderTexture.active=target;var image=new Texture2D(960,600,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,600),0,0);image.Apply();System.IO.File.WriteAllBytes(System.IO.Path.Combine(captureDir,name),image.EncodeToPNG());cam.targetTexture=null;cam.pixelRect=oldRect;RenderTexture.active=previous;target.Release();Destroy(target);Destroy(image);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(!FindFirstObjectByType<GameController>())new GameObject("According to James").AddComponent<GameController>();}
        void Awake()
        {
            Application.targetFrameRate=60;Application.runInBackground=true;initial=PlayerPrefs.GetFloat("JamesDifficulty",1);volume=PlayerPrefs.GetFloat("JamesVolume",.35f);reducedMotion=PlayerPrefs.GetInt("JamesReducedMotion",0)==1;
            art=new SpriteAtlas();music=gameObject.AddComponent<Chiptunes>();music.Volume=volume;
            var c=new GameObject("Dungeon Camera");cam=c.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=3;cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.025f,.04f,.065f);c.AddComponent<AudioListener>();music.Play("cellars");
        }
        void StartRun(bool resume=false)
        {
            if(resume){try{game=JsonUtility.FromJson<GameModel>(PlayerPrefs.GetString("JamesCheckpointV2"));if(game==null||!game.Validate())throw new Exception("Invalid save");}catch{game=new GameModel(initial);}}
            else game=new GameModel(initial,UnityEngine.Random.Range(1,1000000));
            panel="";paused=false;settingsOpen=false;mapOpen=false;ProcessEvents();music.Play(Tune());
        }
        string Tune(){if(game==null)return "cellars";if(game.status=="talking"||game.status=="dead")return "james";if(game.status=="won")return "victory";return game.Current.boss&&!game.Current.cleared?"boss":new[]{"cellars","pantry","crypt"}[game.Dungeon];}
        void Save(){if(game!=null&&game.status!="dead"&&game.status!="won"){PlayerPrefs.SetString("JamesCheckpointV2",JsonUtility.ToJson(game));PlayerPrefs.Save();}}
        void Update()
        {
            guiScale=Mathf.Min(Screen.width/1280f,Screen.height/800f);offsetX=(Screen.width-1280*guiScale)/2;offsetY=(Screen.height-800*guiScale)/2;
            cam.pixelRect=new Rect(offsetX+160*guiScale,Screen.height-(offsetY+710*guiScale),960*guiScale,600*guiScale);cam.aspect=1.6f;
            if(toastLife>0)toastLife-=Time.unscaledDeltaTime;
            if(game!=null&&panel==""&&!settingsOpen&&!mapOpen)
            {
                if(Input.GetKeyDown(KeyCode.Escape)){paused=!paused;if(paused){music.Pause();}else music.Play(Tune());}
                if(!paused&&game.status=="playing")
                {
                    float dx=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
                    float dy=(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0)-(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0);
                    if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.J))game.Attack();
                    if(Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift)||Input.GetKeyDown(KeyCode.K))game.Dodge(dx,dy);
                    if(Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.X))game.Interact();
                    if(Input.GetKeyDown(KeyCode.M)){mapOpen=true;}
                    if(!mapOpen)game.Tick(Time.deltaTime,dx,dy);
                }
                if(game.status=="talking"&&(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Escape))){game.status="playing";music.Play(Tune());}
                ProcessEvents();
            }
            RenderWorld();
            if(smoke){game.Tick(.02f,1,0);smokeFrames++;if(captureDir!=null){if(smokeFrames==20){game.player.x=480;game.player.y=290;game.Interact();game.JamesChoice("gift");ProcessEvents();}if(smokeFrames==35)CaptureWorld("james-room.png");if(smokeFrames==55){game.JamesChoice("leave");game.floor=20;game.NewFloor();game.EnterRoom(game.rooms.Count-1);game.player.inv=99;ProcessEvents();}if(smokeFrames==90)CaptureWorld("lunch-lady-boss.png");}if(smokeFrames==140){Debug.Log("ACCORDING_TO_JAMES_RUNTIME_OK: atlas, sprites, music and movement initialized");Application.Quit(0);}}
        }
        void ProcessEvents()
        {
            if(game==null||game.events==null)return;
            foreach(var e in game.events){if(e.type=="swing"||e.type=="hit"||e.type=="hurt"||e.type=="dodge"||e.type=="heal"||e.type=="clear")music.Effect(e.type);
                if(e.type=="floor"){toast="Floor "+game.floor+" · "+GameModel.DungeonNames[game.Dungeon];toastLife=3;}
                if(e.type=="clear"){toast=e.value==1?"Boss defeated. James takes full credit.":"Room cleared.";toastLife=2;}
                if(e.type=="stairsOpen"){toast="All rooms cleared. The stairs are open!";toastLife=3;}
                if(e.type=="locked"){toast="Clear every room on this floor first.";toastLife=2;}
                if(e.type=="heal"){toast="Health restored. A little kindness.";toastLife=2;}
                if(e.type=="checkpoint")Save();
                if(e.type=="notice"){toast=game.message;toastLife=7;}
                if(e.type=="fart")music.Effect("fart");
                if(e.type=="won")PlayerPrefs.DeleteKey("JamesCheckpointV2");
                if(e.type=="room"||e.type=="floor"||e.type=="james"||e.type=="clear"||e.type=="dead"||e.type=="won")music.Play(Tune());
            }
            game.events.Clear();
        }
        SpriteRenderer Next()
        {
            if(used==pool.Count){var go=new GameObject("Dungeon Sprite "+used);go.transform.SetParent(transform);pool.Add(go.AddComponent<SpriteRenderer>());}var r=pool[used++];r.enabled=true;r.color=Color.white;r.flipX=false;return r;
        }
        void Draw(Sprite s,float x,float y,float height,int order,Color? color=null,float width=0,float rotation=0)
        {
            var r=Next();r.sprite=s;r.sortingOrder=order;r.transform.position=new Vector3(x/100-4.8f,3-y/100,0);r.transform.localScale=new Vector3(width>0?width/s.rect.width:height/s.rect.height,height/s.rect.height,1);r.transform.rotation=Quaternion.Euler(0,0,rotation);r.color=color??Color.white;
        }
        void Rect(float x,float y,float w,float h,Color color,int order){Draw(art.White,x+w/2,y+h/2,h,order,color,w);}
        void Wall(float x,float y,float w,float h){Draw(art.Brick,x+w/2,y+h/2,h,-900,null,w);}
        void RenderWorld()
        {
            used=0;if(game!=null&&panel!="title")
            {
                var r=game.Current;Draw(art.Room,480,300,600,-1000,null,960);
                if(game.Dungeon>0)Rect(0,0,960,600,game.Dungeon==1?new Color(.8f,.25f,.08f,.1f):new Color(.5f,.15f,.8f,.12f),-990);
                var n=game.Neighbors(game.room);if(!n.ContainsKey("up"))Wall(435,14,90,86);if(!n.ContainsKey("down"))Wall(438,505,90,95);if(!n.ContainsKey("left"))Wall(0,266,80,86);if(!n.ContainsKey("right"))Wall(883,266,77,86);
                foreach(var o in r.obstacles)Wall(o.x,o.y,o.w,o.h);foreach(var s in r.spikes)for(int i=0;i<s.w;i+=15)Draw(art.Needle,s.x+i,s.y,18,-800,new Color(.8f,.9f,1),9,90);
                if(game.room==game.rooms.Count-1&&game.AllCleared){Rect(777,102,90,80,new Color(.9f,.75f,.3f,.3f),-850);for(int i=0;i<5;i++)Rect(785,106+i*13,78,9,i%2==0?new Color(.7f,.65f,.5f):new Color(.2f,.25f,.3f),-840);}else Wall(778,100,95,85);
                if(r.chest)Draw(art.Chest,195,210,45,400,r.chestOpened?new Color(.45f,.45f,.45f):Color.white);
                if(r.shrine)Draw(art.Shrine,710,420,60,600);
                if(r.secret&&r.secretKnown&&!r.secretUsed){Rect(65,115,60,70,new Color(.25f,.65f,.7f,.8f),-700);Draw(art.Gas,100,165,25,-690);}
                if(r.james)Draw(art.Idle[1,0],480,210,76,410);
                foreach(var e in r.enemies)if(e.hp>0)
                {
                    int order=200+(int)e.y;if(e.boss){int frame=e.moving?Mathf.FloorToInt(e.walkTime*4)%4:0,face=SpriteAtlas.Direction(e.dir);bool mirror=e.kind==2&&face==2;if(e.kind==2&&(face==1||face==2)){face=1;if(frame==3)frame=1;}Draw(art.Bosses[e.kind*4+face,frame],e.x,e.y,e.kind==0?76:e.kind==1?89:90,order,e.flash>0?new Color(1,1,1,.5f):Color.white);pool[used-1].flipX=mirror;}
                    else {Draw(art.Enemies[e.kind,e.state=="windup"?4:e.state=="strike"?5:e.moving?Mathf.FloorToInt(e.walkTime*4)%4:0],e.x,e.y,58,order,e.flash>0?new Color(1,1,1,.5f):Color.white);pool[used-1].flipX=e.dir=="left";}
                    if(e.state=="windup"||e.boss&&e.attackTimer<.5f)Draw(art.Heart,e.x,e.y-80,20,950,new Color(1,.2f,.15f));
                }
                foreach(var s in r.shots){Sprite sprite=s.type=="gas"?art.Gas:s.type=="flour"?art.Flour:s.type=="gravy"?art.Gravy:s.type=="tray"?art.Tray:s.type=="sausage"?art.Sausage:art.Candy;bool cloud=s.type=="gas"||s.type=="flour"||s.type=="gravy";Draw(sprite,s.x,s.y-12,cloud?80:18,150+(int)s.y,cloud?new Color(1,1,1,Mathf.Min(1,s.age/.45f)*Mathf.Min(1,s.life)):Color.white,s.type=="tray"?40:0,cloud?0:game.time*180);}
                var p=game.player;if(game.fall>0)Draw(art.Pit,p.x,p.y,36,190+(int)p.y,null,82);Sprite hero=art.Walk[SpriteAtlas.Direction(p.dir),p.moving?Mathf.FloorToInt(p.walkTime*6)%6:0];Draw(hero,p.x,p.y,game.fall>0?76*(.45f+game.fall*.6f):76,200+(int)p.y,p.inv>0&&Mathf.FloorToInt(p.inv*20)%2==0?new Color(1,1,1,.5f):Color.white);
                if(p.attack>0){float xx=p.dir=="left"?-45:p.dir=="right"?45:0,yy=p.dir=="up"?-55:p.dir=="down"?25:-15;Draw(art.Needle,p.x+xx,p.y+yy,8,900,new Color(1,.85f,.5f),50,p.dir=="up"||p.dir=="down"?90:0);}
            }
            for(int i=used;i<pool.Count;i++)pool[i].enabled=false;
        }
        void Styles()
        {
            if(label!=null)return;label=new GUIStyle(GUI.skin.label){fontSize=18,normal={textColor=new Color(.93f,.91f,.87f)}};small=new GUIStyle(label){fontSize=13,normal={textColor=new Color(.64f,.68f,.75f)}};heading=new GUIStyle(label){fontSize=36,fontStyle=FontStyle.Bold};button=new GUIStyle(GUI.skin.button){fontSize=18,padding=new RectOffset(10,10,10,10)};
        }
        void Text(float x,float y,float w,string text,GUIStyle style=null){GUI.Label(new Rect(x,y,w,48),text,style??label);}
        void Panel(float x,float y,float w,float h){GUI.color=new Color(.075f,.10f,.15f,.97f);GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);GUI.color=Color.white;}
        bool Button(float x,float y,float w,string text){return GUI.Button(new Rect(x,y,w,46),text,button);}
        void OnGUI()
        {
            if(art==null)return;Styles();GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,new Vector3(guiScale,guiScale,1));
            if(panel=="title"){
                Panel(160,80,960,600);Text(250,125,800,"ACCORDING TO JAMES",heading);Text(250,200,800,"A regular dude. The wrong world. Terrible advice.");Text(250,246,800,"3 dungeons · 30 floors · 3 ridiculous bosses",small);
                if(Button(250,330,390,"Enter the dungeon"))StartRun();if(PlayerPrefs.HasKey("JamesCheckpointV2")&&Button(250,390,390,"Continue your bad idea"))StartRun(true);if(Button(250,470,390,"Settings"))settingsOpen=true;
                Text(250,555,780,"WASD: move   Space: swing   Shift: dodge   E: interact",small);Text(250,590,780,"Chiptunes · Mystery gifts · Rest shrines on floors 1, 5, 9 of each dungeon",small);
            }
            else if(game!=null){
                Panel(160,24,960,78);Text(180,34,300,"DUNGEON "+(game.Dungeon+1),small);Text(180,56,300,GameModel.DungeonNames[game.Dungeon]);Text(490,34,160,"FLOOR",small);Text(490,56,160,game.floor.ToString("00")+" / 30");Text(640,34,160,"ROOM",small);Text(640,56,160,(game.room+1).ToString("00")+" / "+game.rooms.Count.ToString("00"));Text(800,34,190,"HEALTH "+game.player.hp+" / "+game.player.maxHp,small);
                GUI.color=new Color(.3f,.15f,.18f);GUI.DrawTexture(new Rect(800,64,190,10),Texture2D.whiteTexture);GUI.color=new Color(.9f,.5f,.4f);GUI.DrawTexture(new Rect(800,64,190*game.player.hp/game.player.maxHp,10),Texture2D.whiteTexture);GUI.color=Color.white;Text(1020,34,100,"SWORD",small);Text(1020,56,100,game.Sword.ToString("00"));
                Text(160,725,850,"Space: swing    Shift: dodge    E: interact    M: map    Esc: pause",small);Text(160,758,480,game.player.dodgeCooldown>0?"DODGE RECHARGING":"DODGE READY",small);Text(800,725,320,game.boost>0?"SWORD BOOST "+(int)game.boost+"s":game.dizzy>0?"WOBBLY "+(int)game.dizzy+"s":"SAVED: "+(game.checkpointFloor>0?"FLOOR "+game.checkpointFloor:"NO SHRINE YET"),small);Text(640,758,480,game.AllCleared?"Stairs open · final room, upper right":"Clear every room to unlock stairs",small);
                if(toastLife>0){Panel(245,110,790,62);GUI.Label(new Rect(260,120,760,48),toast,new GUIStyle(small){wordWrap=true});}string context=game.Context();if(context!=""){Panel(390,651,500,44);Text(410,660,470,context=="checkpoint"?"[E] Rest & save at shrine":context=="chest"?"[E] Open chest · could be trapped":context=="secret"?"[E] Try James’s secret passage":context=="james"?"[E] Talk to James":context=="loot"?"[E] Collect healing heart":context=="stairs"?"[E] Descend to the next floor":"Stairs locked · clear every room",small);}
                foreach(var e in game.Current.enemies)if(e.boss&&e.hp>0){Panel(400,160,480,55);Text(415,165,450,GameModel.BossNames[game.Dungeon],small);GUI.color=new Color(.85f,.4f,.45f);GUI.DrawTexture(new Rect(420,196,440*e.hp/e.maxHp,7),Texture2D.whiteTexture);GUI.color=Color.white;}
                if(game.status=="talking"){
                    Panel(270,180,740,480);Text(310,200,650,"ACCORDING TO JAMES…",small);Text(310,244,650,GameModel.Advice[game.Current.advice],new GUIStyle(heading){fontSize=23,wordWrap=true});
                    GUI.Label(new Rect(310,306,650,64),game.message,new GUIStyle(small){wordWrap=true});
                    GUI.enabled=!game.Current.giftTaken;if(Button(310,385,315,"Accept mystery gift"))game.JamesChoice("gift");
                    GUI.enabled=!game.Current.fingerPulled;if(Button(645,385,315,"Pull his finger"))game.JamesChoice("finger");GUI.enabled=true;
                    GUI.enabled=game.Current.secret;if(Button(310,447,650,"Ask about a secret entrance"))game.JamesChoice("secret");GUI.enabled=true;
                    if(Button(310,515,650,"Thanks, James. I guess.")){game.JamesChoice("leave");music.Play(Tune());}
                    Text(310,588,650,"Mystery gifts may help or cause a small mishap.",small);
                }
                if(game.status=="dead"||game.status=="won"){
                    Panel(350,230,580,340);Text(390,270,500,game.status=="won"?"You survived James.":"James was wrong.",heading);Text(390,323,500,game.status=="won"?"30 floors. Three bosses. Zero reliable tips.":"You reached floor "+game.floor+".");if(PlayerPrefs.HasKey("JamesCheckpointV2")&&game.status=="dead"&&Button(390,365,500,"Return to last rest shrine"))StartRun(true);if(Button(390,425,500,"Try another bad idea"))StartRun();if(Button(390,475,500,"Return to title")){panel="title";music.Play("cellars");}
                }
                if(paused){Panel(350,220,580,350);Text(390,250,500,"Dungeon on hold.",heading);if(Button(390,325,500,"Resume")){paused=false;music.Play(Tune());}if(Button(390,388,500,"Settings"))settingsOpen=true;if(Button(390,451,500,"Return to title")){panel="title";paused=false;music.Play("cellars");}}
                if(mapOpen){Panel(350,170,580,470);Text(390,200,500,"ROOM MAP · FLOOR "+game.floor,heading);for(int i=0;i<game.rooms.Count;i++){var r=game.rooms[i];Text(390+i%2*250,280+i/2*75,240,(i==game.room?"> ":"")+"ROOM "+(i+1).ToString("00"),label);Text(390+i%2*250,310+i/2*75,240,!r.visited?"Unexplored":r.cleared?"Cleared":"Combat",small);}if(Button(390,560,500,"Back to the dungeon"))mapOpen=false;}
            }
            if(settingsOpen){Panel(350,150,580,480);Text(390,185,500,"Settings",heading);Text(390,250,500,"Starting difficulty · new runs only");int choice=initial==.75f?0:initial==1?1:2;choice=GUI.SelectionGrid(new Rect(390,290,500,48),choice,new[]{"Mellow","Normal","Spicy"},3,button);initial=choice==0?.75f:choice==1?1:1.5f;Text(390,358,500,"Chiptune volume");volume=GUI.HorizontalSlider(new Rect(390,405,500,25),volume,0,1);music.Volume=volume;reducedMotion=GUI.Toggle(new Rect(390,440,500,32),reducedMotion,"Reduced motion");Text(390,490,500,"Each floor is 8% harder; difficulty stays in Settings.",small);if(Button(390,550,500,"Done")){settingsOpen=false;PlayerPrefs.SetFloat("JamesDifficulty",initial);PlayerPrefs.SetFloat("JamesVolume",volume);PlayerPrefs.SetInt("JamesReducedMotion",reducedMotion?1:0);PlayerPrefs.Save();}}
        }
        void OnApplicationPause(bool pause){if(pause)paused=true;}
    }
}
