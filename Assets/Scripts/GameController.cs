using UnityEngine;
using System;
using System.Collections.Generic;

namespace AccordingToJames
{
    public class GameController : MonoBehaviour
    {
        GameModel game;SpriteAtlas art;Chiptunes music;Camera cam;DungeonUI ui;
        public GameModel Game=>game;public SpriteAtlas Art=>art;public bool TitleScreen=>panel=="title";public bool IsPaused=>paused;public bool SettingsOpen=>settingsOpen;public bool MapOpen=>mapOpen;public bool HasCheckpoint=>PlayerPrefs.HasKey("JamesCheckpointV2");public bool ToastVisible=>toastLife>0;public string ToastMessage=>toast;public float Initial=>initial;public float Volume=>volume;public bool ReducedMotion=>reducedMotion;
        public void NewRun(){StartRun();}public void ContinueRun(){if(HasCheckpoint)StartRun(true);}public void OpenSettings(){settingsOpen=true;}public void CloseSettings(){settingsOpen=false;PlayerPrefs.SetFloat("JamesDifficulty",initial);PlayerPrefs.SetFloat("JamesVolume",volume);PlayerPrefs.SetInt("JamesReducedMotion",reducedMotion?1:0);PlayerPrefs.Save();}
        public void SetDifficulty(float value){initial=value;}public void SetVolume(float value){volume=Mathf.Clamp01(value);music.Volume=volume;}public void SetReducedMotion(bool value){reducedMotion=value;}
        public void Resume(){paused=false;music.Play(Tune());}public void TogglePause(){if(game==null||game.status!="playing")return;paused=!paused;if(paused)music.Pause();else music.Play(Tune());}public void ReturnToTitle(){panel="title";paused=settingsOpen=mapOpen=false;music.Play("menu");}public void OpenMap(){if(game!=null&&game.status=="playing")mapOpen=true;}public void CloseMap(){mapOpen=false;}public void TalkChoice(string choice){game.JamesChoice(choice);if(choice=="gift"||choice=="finger"){npcGestureFrame=choice=="gift"?2:3;npcGestureUntil=Time.unscaledTime+1.5f;}ProcessEvents();if(choice=="leave")music.Play(Tune());}
        float npcGestureUntil;int npcGestureFrame;
        int JamesRow=>game!=null&&game.status=="talking"?3:game!=null&&game.Current.jamesWalking?(game.Current.jamesFacing=="left"?0:1):game!=null&&game.Current.jamesClock%18>=6&&game.Current.jamesClock%18<14?2:3;
        int JamesFrame=>reducedMotion?0:JamesRow==3?(Time.unscaledTime<npcGestureUntil?npcGestureFrame:Mathf.FloorToInt(Time.unscaledTime*.7f)%6):Mathf.FloorToInt(game.Current.jamesClock*(JamesRow==2?3:8))%6;
        int JamesDance=>game==null?0:(int)(game.Current.jamesClock/18)%4;
        int DanceFrame=>reducedMotion?0:Mathf.FloorToInt((game.Current.jamesClock%18-6)*2)%8;
        public Sprite NpcSprite=>JamesRow==2&&JamesDance>0?art.JamesDanceParty[JamesDance-1,DanceFrame]:art.JamesGoofy[JamesRow,JamesFrame];
        public Sprite NpcPortrait=>JamesRow==2&&JamesDance>0?art.JamesDancePortraits[JamesDance-1,DanceFrame]:art.JamesGoofyPortraits[JamesRow,JamesFrame];
        readonly List<SpriteRenderer> pool=new List<SpriteRenderer>();int used;
        bool paused,settingsOpen,mapOpen;string toast="",panel="title";float toastLife;
        float initial=1,volume=.35f;bool reducedMotion;float guiScale,offsetX,offsetY;
        int smokeFrames;bool smoke;string captureDir;
        void Start(){var args=Environment.GetCommandLineArgs();smoke=Array.IndexOf(args,"--self-test")>=0;int capture=Array.IndexOf(args,"--capture-dir");if(capture>=0&&capture+1<args.Length){captureDir=args[capture+1];System.IO.Directory.CreateDirectory(captureDir);}if(smoke&&captureDir==null)StartRun();}
        void CaptureWorld(string name){var target=new RenderTexture(960,600,24);var previous=RenderTexture.active;var oldRect=cam.pixelRect;cam.targetTexture=target;cam.rect=new Rect(0,0,1,1);cam.Render();RenderTexture.active=target;var image=new Texture2D(960,600,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,600),0,0);image.Apply();System.IO.File.WriteAllBytes(System.IO.Path.Combine(captureDir,name),image.EncodeToPNG());cam.targetTexture=null;cam.pixelRect=oldRect;RenderTexture.active=previous;target.Release();Destroy(target);Destroy(image);}
        void CapturePresentation(string name){
            var target=new RenderTexture(1280,800,24);var previous=RenderTexture.active;RenderTexture.active=target;GL.Clear(true,true,new Color(.02f,.025f,.035f));
            var oldRect=cam.pixelRect;cam.targetTexture=target;cam.rect=new Rect(160/1280f,90/800f,960/1280f,600/800f);cam.Render();cam.targetTexture=null;cam.pixelRect=oldRect;
            ui.RenderTo(target);ui.ValidateTextFit();RenderTexture.active=target;var image=new Texture2D(1280,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,800),0,0);image.Apply();System.IO.File.WriteAllBytes(System.IO.Path.Combine(captureDir,name),image.EncodeToPNG());RenderTexture.active=previous;target.Release();Destroy(target);Destroy(image);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(!FindFirstObjectByType<GameController>())new GameObject("According to James").AddComponent<GameController>();}
        Sprite softLight,softShadow;
        void Awake()
        {
            Application.targetFrameRate=60;Application.runInBackground=true;initial=PlayerPrefs.GetFloat("JamesDifficulty",1);volume=PlayerPrefs.GetFloat("JamesVolume",.35f);reducedMotion=PlayerPrefs.GetInt("JamesReducedMotion",0)==1;
            softLight=Radial(false);softShadow=Radial(true);art=new SpriteAtlas();music=gameObject.AddComponent<Chiptunes>();music.Volume=volume;
            var c=new GameObject("Dungeon Camera");cam=c.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=3;cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.025f,.04f,.065f);cam.cullingMask=~(1<<5);c.AddComponent<AudioListener>();music.Play("menu");ui=gameObject.AddComponent<DungeonUI>();ui.Initialize(this);
        }
        void StartRun(bool resume=false)
        {
            if(resume){try{game=JsonUtility.FromJson<GameModel>(PlayerPrefs.GetString("JamesCheckpointV2"));if(game==null||!game.Validate())throw new Exception("Invalid save");}catch{game=new GameModel(initial);}}
            else game=new GameModel(initial,UnityEngine.Random.Range(1,1000000));
            panel="";paused=false;settingsOpen=false;mapOpen=false;ProcessEvents();music.Play(Tune());
        }
        string Tune(){if(TitleScreen||game==null)return "menu";if(game.status=="talking"||game.status=="dead")return "james";if(game.status=="won")return "victory";return game.Current.enemies.Exists(e=>e.hp>0)?"combat":"exploration";}
        void Save(){if(game!=null&&game.status!="dead"&&game.status!="won"){PlayerPrefs.SetString("JamesCheckpointV2",JsonUtility.ToJson(game));PlayerPrefs.Save();}}
        void Update()
        {
            guiScale=Mathf.Min(Screen.width/1280f,Screen.height/800f);offsetX=(Screen.width-1280*guiScale)/2;offsetY=(Screen.height-800*guiScale)/2;
            cam.pixelRect=new Rect(offsetX+160*guiScale,Screen.height-(offsetY+710*guiScale),960*guiScale,600*guiScale);cam.aspect=1.6f;
            if(toastLife>0)toastLife-=Time.unscaledDeltaTime;
            if(Input.GetKeyDown(KeyCode.Escape)){if(settingsOpen)CloseSettings();else if(mapOpen)CloseMap();else if(game!=null&&game.status=="talking")TalkChoice("leave");else if(!TitleScreen)TogglePause();}
            if(mapOpen&&Input.GetKeyDown(KeyCode.M))CloseMap();
            if(game!=null&&!TitleScreen&&!settingsOpen&&!mapOpen&&!paused){
                if(game.status=="talking"&&Input.GetKeyDown(KeyCode.Return))TalkChoice("leave");
                else if(game.status=="playing"){
                    float dx=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
                    float dy=(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0)-(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0);
                    if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.J))game.Attack();
                    if(Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift)||Input.GetKeyDown(KeyCode.K))game.Dodge(dx,dy);
                    if(Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.X))game.Interact();
                    if(Input.GetKeyDown(KeyCode.M))OpenMap();
                    if(!mapOpen)game.Tick(Time.deltaTime,dx,dy);
                }
            }
            ProcessEvents();
            RenderWorld();
            if(smoke){
                smokeFrames++;
                if(captureDir==null){game.Tick(.02f,1,0);if(smokeFrames==140){Debug.Log("ACCORDING_TO_JAMES_RUNTIME_OK");Application.Quit(0);}}
                else{
                    if(smokeFrames==20){music.VerifySuppliedTracks();music.VerifyTrack("menu");CapturePresentation("title-screen.png");}
                    if(smokeFrames==40)ui.PressForCheck("Start Game");
                    if(smokeFrames==60){music.VerifyTrack("exploration");CapturePresentation("game-hud.png");}
                    if(smokeFrames==80){game.player.x=480;game.player.y=290;game.Interact();ui.PressForCheck("Accept mystery gift");}
                    if(smokeFrames==110)CapturePresentation("james-menu.png");
                    if(smokeFrames==130){ui.PressForCheck("Thanks, James. I guess.");ui.PressForCheck("Map");}
                    if(smokeFrames==160)CapturePresentation("room-map.png");
                    if(smokeFrames==180){ui.PressForCheck("Back to the dungeon");ui.PressForCheck("Pause");}
                    if(smokeFrames==210)CapturePresentation("pause-menu.png");
                    if(smokeFrames==230){ui.PressForCheck("Settings");float oldVolume=volume;bool oldMotion=reducedMotion;ui.PressForCheck("Mellow");ui.PressForCheck("Normal");ui.PressForCheck("Quieter");ui.PressForCheck("Louder");ui.PressForCheck("Reduced motion: "+(reducedMotion?"On":"Off"));ui.PressForCheck("Reduced motion: "+(reducedMotion?"On":"Off"));SetVolume(oldVolume);SetReducedMotion(oldMotion);SetDifficulty(game.initial);}
                    if(smokeFrames==260)CapturePresentation("settings-menu.png");
                    if(smokeFrames==280){ui.PressForCheck("Done");ui.PressForCheck("Back to the dungeon");game.floor=20;game.NewFloor();game.EnterRoom(game.rooms.Count-1);game.player.inv=99;ProcessEvents();}
                    if(smokeFrames==320){music.VerifyTrack("combat");CapturePresentation("boss-hud.png");}
                    if(smokeFrames==340)game.status="dead";
                    if(smokeFrames==360)CapturePresentation("death-menu.png");
                    if(smokeFrames==380)game.status="won";
                    if(smokeFrames==400)CapturePresentation("victory-menu.png");
                    if(smokeFrames==420){ui.PressForCheck("Another bad idea");ui.PressForCheck("Pause");ui.PressForCheck("Return to title");}
                    if(smokeFrames==440){NewRun();game.Current.jamesClock=8;}
                    if(smokeFrames==450)CapturePresentation("james-dance.png");
                    if(smokeFrames==460){game.Current.jamesClock=2;}
                    if(smokeFrames==480)CapturePresentation("james-pacing.png");
                    if(smokeFrames==500){game.EnterRoom(2);game.player.x=250;game.player.y=285;}
                    if(smokeFrames==510){music.VerifyTrack("exploration");CapturePresentation("treasure-room.png");}
                    if(smokeFrames==520)game.Interact();
                    if(smokeFrames==530)CapturePresentation("chest-opening.png");
                    if(smokeFrames==550){game.floor=30;game.NewFloor();game.EnterRoom(game.rooms.Count-1);game.player.inv=99;}
                    if(smokeFrames==580)CapturePresentation("kitchen-boss.png");
                    if(smokeFrames==620){NewRun();game.Current.jamesClock=26;}
                    if(smokeFrames==630)CapturePresentation("james-ymca.png");
                    if(smokeFrames==640)game.Current.jamesClock=44;
                    if(smokeFrames==650)CapturePresentation("james-macarena.png");
                    if(smokeFrames==660)game.Current.jamesClock=62;
                    if(smokeFrames==670)CapturePresentation("james-robot.png");
                    if(smokeFrames==700){Debug.Log("ACCORDING_TO_JAMES_PRESENTATION_OK: title, HUD, dialogue, map, pause, settings, boss, death, victory and actual button actions");Application.Quit(0);}
                }
            }
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
                if(e.type=="fart"){music.Effect("fart");npcGestureFrame=3;npcGestureUntil=Time.unscaledTime+1.5f;}
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
        void Door(string side)
        {
            float x=side=="left"?65:side=="right"?895:480,y=side=="up"?62:side=="down"?538:GameModel.DoorCenterY;
            float rotation=side=="left"?90:side=="right"?-90:side=="down"?180:0;
            Draw(art.Props[1,game.DoorState(side)],x,y,142,-600,null,132,rotation);
        }
        void RenderWorld()
        {
            used=0;
            if(game!=null&&!TitleScreen){
                var room=game.Current;Draw(art.Rooms[game.Dungeon],480,300,600,-1000,null,960);
                for(int fy=0;fy<7;fy++)for(int fx=0;fx<14;fx++)Draw(art.FloorDepth[game.Dungeon,FloorVariant(fx,fy)],95+(fx+.5f)*55,125+(fy+.5f)*50,50,-900,new Color(.82f,.85f,.9f),55);
                foreach(string side in new[]{"up","down","left","right"})Door(side);
                foreach(var obstacle in room.obstacles)Draw(art.Props[3,(game.floor+room.id)%2],obstacle.x+obstacle.w/2,obstacle.y+obstacle.h,obstacle.h*1.4f,200+(int)(obstacle.y+obstacle.h),null,obstacle.w*1.18f);
                foreach(var trap in room.spikes)Draw(art.Props[3,2],trap.x+trap.w/2,trap.y+trap.h,trap.h*1.6f,130+(int)(trap.y+trap.h),null,trap.w*1.2f);
                if(game.room==game.rooms.Count-1)Draw(art.Props[2,game.AllCleared?3:2],805,235,118,190,null,118);
                if(room.chest){Shadow(205,235,62,86);int frame=!room.chestOpened?0:room.chestAnim>.55f?1:room.chestTrap&&room.chestAnim>.05f?3:2;Draw(art.Props[0,frame],205,235,86,435,null,90);}
                if(room.shrine){Shadow(710,420,55,108);Draw(art.Props[2,1],710,420,108,620,Color.Lerp(Color.white,new Color(.7f,1,.9f),reducedMotion?.2f:(Mathf.Sin(game.time*3)+1)*.2f));}
                if(room.secret&&room.secretKnown)Draw(art.Props[1,room.secretUsed?0:3],GameModel.SecretX,62,126,-590,null,108);
                if(room.james){float hop=!reducedMotion&&JamesRow==2?Mathf.Abs(Mathf.Sin(room.jamesClock*7))*4:0;Shadow(game.JamesX,game.JamesY,52,112);Draw(NpcSprite,game.JamesX,game.JamesY-hop,112,200+(int)game.JamesY,ActorLight(game.JamesX,game.JamesY));}
                foreach(var enemy in room.enemies){
                    if(enemy.hp<=0)continue;int order=200+(int)enemy.y,frame=enemy.moving&&!reducedMotion?Mathf.FloorToInt(enemy.walkTime*4)%4:0;
                    Shadow(enemy.x,enemy.y,enemy.boss?66:42,enemy.boss?124:90);
                    Color tint=enemy.flash>0?new Color(1,1,1,.65f):ActorLight(enemy.x,enemy.y);
                    if(enemy.boss)Draw(art.Bosses[enemy.kind*4+SpriteAtlas.Direction(enemy.dir),frame],enemy.x,enemy.y,(enemy.kind==0?112:enemy.kind==1?124:128)*(1+(reducedMotion?0:Mathf.Sin(game.time*3+enemy.id)*.012f)),order,tint);
                    else {Draw(art.Enemies[enemy.kind,enemy.state=="windup"?4:enemy.state=="strike"?5:frame],enemy.x,enemy.y,90,order,tint);pool[used-1].flipX=enemy.dir=="left";}
                    if(!reducedMotion){var body=pool[used-1].transform;float swell=enemy.state=="windup"?1.06f:enemy.state=="strike"?.94f:1;body.localScale=new Vector3(body.localScale.x/swell,body.localScale.y*swell,1);}
                    if(enemy.state=="windup"||enemy.boss&&enemy.attackTimer<.5f)Draw(art.Effects[1,3],enemy.x,enemy.y-92,32,950);
                }
                foreach(var shot in room.shots){
                    bool cloud=shot.type=="gas"||shot.type=="gravy"||shot.type=="flour";Sprite sprite=cloud?art.Effects[1,shot.type=="gas"?0:shot.type=="gravy"?1:2]:art.Effects[0,shot.type=="tray"?1:shot.type=="sausage"?2:0];
                    Color tint=cloud?new Color(1,1,1,Mathf.Min(1,shot.age/.45f)*Mathf.Min(1,shot.life)):Color.white;Draw(sprite,shot.x,shot.y-10,cloud?100*(1+(reducedMotion?0:Mathf.Sin(shot.age*4)*.07f)):shot.type=="tray"?38:26,150+(int)shot.y,tint,0,reducedMotion?0:cloud?Mathf.Sin(shot.age*3)*8:game.time*150);
                }
                var player=game.player;Shadow(player.x,player.y,42,96);
                if(game.fall>0)Draw(art.Props[3,3],player.x,player.y+18,68,190+(int)player.y,null,104);
                int heroFrame=player.attack>0?player.attack>.125f?5:player.attack>.04f?6:7:player.moving&&!reducedMotion?1+Mathf.FloorToInt(player.walkTime*4)%4:0;
                Draw(art.Hero[SpriteAtlas.Direction(player.dir),heroFrame],player.x,player.y,game.fall>0?96*(.45f+game.fall*.6f):96,200+(int)player.y,player.inv>0&&Mathf.FloorToInt(player.inv*20)%2==0?new Color(1,1,1,.65f):ActorLight(player.x,player.y));
                Lighting();
            }
            for(int i=used;i<pool.Count;i++)pool[i].enabled=false;
        }
        int FloorVariant(int x,int y){unchecked{uint hash=(uint)(game.seed^game.floor*7919^game.room*104729^x*73856093^y*19349663);hash^=hash>>16;hash*=0x7feb352d;hash^=hash>>15;return (int)(hash%4);}}
        Sprite Radial(bool shadow){int n=128;var texture=new Texture2D(n,n,TextureFormat.RGBA32,false);var colors=new Color[n*n];for(int y=0;y<n;y++)for(int x=0;x<n;x++){float dx=(x+.5f)/n*2-1,dy=(y+.5f)/n*2-1,r=Mathf.Sqrt(dx*dx+dy*dy);float alpha=Mathf.Pow(Mathf.Max(0,1-r),shadow?1.6f:2.4f);colors[y*n+x]=new Color(shadow?0:1,shadow?0:1,shadow?0:1,alpha);}texture.SetPixels(colors);texture.Apply();texture.filterMode=FilterMode.Bilinear;return Sprite.Create(texture,new Rect(0,0,n,n),new Vector2(.5f,.5f),100);}
        void Shadow(float x,float y,float size,float height){Draw(softShadow,x+6,y+3,size*.36f,180+(int)y,new Color(0,0,0,.55f),size*1.6f);}
        Color ActorLight(float x,float y){float distance=999;foreach(float tx in new[]{95f,865f})foreach(float ty in new[]{125f,450f})distance=Mathf.Min(distance,Vector2.Distance(new Vector2(x,y),new Vector2(tx,ty)));float near=Mathf.Max(0,1-distance/300);return Color.Lerp(new Color(.78f,.84f,.96f),new Color(1,.91f,.76f),near*.8f);}
        void Lighting(){float flicker=reducedMotion?1:.92f+Mathf.Sin(game.time*8)*.04f+Mathf.Sin(game.time*13)*.04f;foreach(float x in new[]{95f,865f})foreach(float y in new[]{125f,450f})Draw(softLight,x,y,260,-650,new Color(1,.52f,.12f,.23f*flicker),300);if(game.Current.shrine)Draw(softLight,710,400,160,-645,new Color(.1f,1,.8f,.24f),200);}
        void OnApplicationPause(bool pause){if(pause&&game!=null&&game.status=="playing"&&!TitleScreen){paused=true;music.Pause();}}
    }
}
