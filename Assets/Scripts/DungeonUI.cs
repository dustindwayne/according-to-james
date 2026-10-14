using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System;
using System.Collections.Generic;

namespace AccordingToJames
{
    // Native canvas UI. Reference coordinates keep the room, HUD and modal cards aligned.
    public sealed class DungeonUI : MonoBehaviour
    {
        GameController owner;Canvas canvas;Camera uiCamera;RectTransform root;
        RectTransform navigationRoot;Font font;string screenKey="";readonly Dictionary<string,Text> labels=new Dictionary<string,Text>();
        Image healthFill,dodgeFill,bossFill,portrait;RectTransform toastBox,contextBox;Text notice,context;
        static readonly Color Ink=new Color(.035f,.045f,.06f),Panel=new Color(.065f,.08f,.10f),Gold=new Color(.87f,.66f,.32f),Cream=new Color(.95f,.9f,.77f),Muted=new Color(.61f,.64f,.65f),Edge=new Color(.35f,.29f,.2f),Red=new Color(.79f,.32f,.27f),Green=new Color(.34f,.67f,.55f);
        public void Initialize(GameController controller)
        {
            owner=controller;font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var cameraObject=new GameObject("Interface Camera");uiCamera=cameraObject.AddComponent<Camera>();uiCamera.orthographic=true;uiCamera.cullingMask=1<<5;uiCamera.clearFlags=CameraClearFlags.Depth;uiCamera.depth=10;uiCamera.transform.position=new Vector3(0,0,-50);
            var go=new GameObject("Dungeon Interface",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.layer=5;canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=uiCamera;canvas.planeDistance=1;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,800);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var design=Node(go.transform,"1280 x 800 interface",0,0,1280,800);design.anchorMin=design.anchorMax=design.pivot=new Vector2(.5f,.5f);design.anchoredPosition=Vector2.zero;root=design;
            if(!FindFirstObjectByType<EventSystem>()){
                var inputObject=new GameObject("Menu Input",typeof(EventSystem),typeof(InputSystemUIInputModule));var module=inputObject.GetComponent<InputSystemUIInputModule>();module.AssignDefaultActions();
                var snesAsset=ScriptableObject.CreateInstance<InputActionAsset>();var snesMap=snesAsset.AddActionMap("SNES UI");var submit=snesMap.AddAction("SNES A Confirm",InputActionType.Button);submit.AddBinding("<Gamepad>/buttonEast");submit.AddBinding("<Keyboard>/enter");
                var cancel=snesMap.AddAction("SNES B Back",InputActionType.Button);cancel.AddBinding("<Gamepad>/buttonSouth");cancel.AddBinding("<Keyboard>/escape");module.submit=InputActionReference.Create(submit);module.cancel=InputActionReference.Create(cancel);snesMap.Enable();
            }
            Rebuild();
        }
        public void Invalidate(){screenKey="";}
        public void PressForCheck(string title){Refresh();foreach(var button in root.GetComponentsInChildren<Button>()){var text=button.GetComponentInChildren<Text>();if(text&&text.text==title&&button.interactable){button.onClick.Invoke();Refresh();return;}}throw new Exception("Missing active menu action: "+title);}
        void LateUpdate(){Refresh();}
        public void Refresh()
        {
            if(!owner)return;if(owner.MobileControlsEnabled){var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/1280f,safe.height/800f);root.localScale=Vector3.one*(scale/canvas.scaleFactor);root.anchoredPosition=new Vector2(safe.center.x-Screen.width/2f,safe.center.y-Screen.height/2f)/canvas.scaleFactor;}var g=owner.Game;string key=owner.TitleScreen?"title":"game";
            if(g!=null&&!owner.TitleScreen)key+=":"+g.floor+":"+g.room+":"+(g.Current.boss&&g.Current.enemies.Exists(e=>e.hp>0))+":"+g.status+":"+g.Current.giftTaken+":"+g.Current.fingerPulled;
            key+=":"+owner.IsPaused+":"+owner.SettingsOpen+":"+owner.MapOpen+":"+owner.Fullscreen+":"+owner.Updater.State+":"+owner.ControllerConnected;
            if(key!=screenKey){screenKey=key;Rebuild();}Set("update-status",owner.Updater.Message);
            if(g!=null&&!owner.TitleScreen){Set("dungeon",GameModel.DungeonNames[g.Dungeon]);Set("floor",g.floor.ToString("00")+" / 30");Set("room",(g.room+1).ToString("00")+" / "+g.rooms.Count.ToString("00"));Set("health",Mathf.CeilToInt(g.player.hp)+" / "+Mathf.CeilToInt(g.player.maxHp));Set("sword",g.Sword.ToString("00"));
                Fill(healthFill,g.player.hp/g.player.maxHp,218);Fill(dodgeFill,1-g.player.dodgeCooldown/1.25f,96);Set("dodge",g.player.dodgeCooldown>0?"RECHARGING":"DODGE READY");
                Set("checkpoint",g.checkpointFloor>0?"LAST REST SHRINE · FLOOR "+g.checkpointFloor.ToString("00"):"NO SHRINE SAVE YET · REST ON FLOORS 1, 5, 9 OF EACH DUNGEON");
                Set("objective",g.AllCleared?"STAIRS OPEN · FINAL ROOM":g.rooms.FindAll(r=>r.cleared&&r.visited).Count+" / "+g.rooms.Count+" ROOMS CLEARED");Set("effect",g.boost>0?"SWORD BOOST · "+Mathf.CeilToInt(g.boost)+"s":g.dizzy>0?"A LITTLE WOBBLY · "+Mathf.CeilToInt(g.dizzy)+"s":"");Set("james-result",g.message.Length>0?g.message:GameModel.Followup[g.Current.advice]);
                if(toastBox){toastBox.gameObject.SetActive(owner.ToastVisible&&g.status=="playing"&&!owner.IsPaused&&!owner.SettingsOpen&&!owner.MapOpen);notice.text=owner.ToastMessage;}
                if(contextBox){string c=g.Context();contextBox.gameObject.SetActive(c.Length>0&&g.status=="playing"&&!owner.IsPaused&&!owner.SettingsOpen&&!owner.MapOpen);context.text=c=="james"?"Talk to James":c=="checkpoint"?"Rest & save at shrine":c=="chest"?"Open chest · trust optional":c=="secret"?"Try James’s secret entrance":c=="stairs"?"Descend to the next floor":c=="barred"?"Clear this room to lift the gates":"Stairs locked · clear every room";}
                if(portrait)portrait.sprite=owner.NpcPortrait;
                if(bossFill){foreach(var e in g.Current.enemies)if(e.boss&&e.hp>0)Fill(bossFill,e.hp/e.maxHp,400);}
            }
        }
        void Set(string key,string value){if(labels.TryGetValue(key,out var text)&&text.text!=value)text.text=value;}
        static void Fill(Image image,float value,float width){if(image)image.rectTransform.sizeDelta=new Vector2(width*Mathf.Clamp01(value),image.rectTransform.sizeDelta.y);}
        RectTransform Node(Transform parent,string name,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform));go.layer=5;var rect=go.GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(w,h);return rect;
        }
        Image Box(Transform parent,float x,float y,float w,float h,Color color,bool hit=false)
        {
            var n=Node(parent,"Panel",x,y,w,h);var image=n.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=hit;return image;
        }
        RectTransform Frame(Transform parent,float x,float y,float w,float h,Color? fill=null)
        {
            var image=Box(parent,x,y,w,h,Color.white);image.sprite=h<115?owner.Art.Menus[2,0]:owner.Art.Menus[0,0];image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=h<115?2:1;image.color=new Color(1,1,1,fill.HasValue?fill.Value.a:1);return image.rectTransform;
        }
        Text Text(Transform parent,float x,float y,float w,float h,string value,int size=18,Color? color=null,bool bold=false,TextAnchor align=TextAnchor.UpperLeft,string key=null)
        {
            var n=Node(parent,"Text "+(key??value),x,y,w,h);var text=n.gameObject.AddComponent<Text>();text.font=font;text.fontSize=size;text.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;text.color=color??Cream;text.text=value;text.alignment=align;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;text.raycastTarget=false;if(key=="dungeon"||key=="effect"||key=="james-result"||key=="update-status"){text.resizeTextForBestFit=true;text.resizeTextMinSize=Mathf.Max(12,size-3);text.resizeTextMaxSize=size;}if(key!=null)labels[key]=text;return text;
        }
        Image Picture(Transform parent,Sprite sprite,float x,float y,float w,float h){var n=Node(parent,"Portrait",x,y,w,h);var image=n.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return image;}
        void Button(Transform parent,float x,float y,float w,string title,Action action,bool primary=false,bool enabled=true,float height=54)
        {
            var image=Box(parent,x,y,w,height,Color.white,true);var n=image.rectTransform;image.sprite=owner.Art.Menus[1,primary?1:0];image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=3;
            var button=n.gameObject.AddComponent<Button>();button.targetGraphic=image;var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.2f,1.15f,1.05f);colors.pressedColor=new Color(.7f,.7f,.7f);colors.disabledColor=new Color(.5f,.5f,.5f,.7f);button.colors=colors;button.interactable=enabled;button.onClick.AddListener(()=>{action();Invalidate();});
            button.transition=Selectable.Transition.SpriteSwap;button.spriteState=new SpriteState{highlightedSprite=primary?owner.Art.Menus[1,1]:owner.Art.Menus[2,1],pressedSprite=owner.Art.Menus[1,1],selectedSprite=primary?owner.Art.Menus[1,1]:owner.Art.Menus[2,1],disabledSprite=owner.Art.Menus[2,0]};Text(n,24,6,w-48,height-12,title,17,primary&&enabled?Ink:Cream,true,TextAnchor.MiddleCenter);
        }
        void Rebuild()
        {
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(null);for(int i=root.childCount-1;i>=0;i--){var child=root.GetChild(i).gameObject;child.SetActive(false);Destroy(child);}labels.Clear();navigationRoot=null;healthFill=dodgeFill=bossFill=portrait=null;toastBox=contextBox=null;
            if(owner.TitleScreen)Title();else if(owner.Game!=null)HUD();
            if(owner.SettingsOpen)Settings();else if(owner.MapOpen)Map();else if(owner.IsPaused)Pause();else if(owner.Game!=null&&!owner.TitleScreen){if(owner.Game.status=="talking")James();else if(owner.Game.status=="dead"||owner.Game.status=="won")Ending();}
            if(owner.ControllerConnected&&navigationRoot&&EventSystem.current){foreach(var button in navigationRoot.GetComponentsInChildren<Button>())if(button.interactable){EventSystem.current.SetSelectedGameObject(button.gameObject);break;}}
        }
        void Title()
        {
            navigationRoot=root;Box(root,0,0,1280,800,Ink);var tex=Resources.Load<Texture2D>("Art/TitleScreen");if(tex){var image=Node(root,"Generated title artwork",0,0,1280,800).gameObject.AddComponent<RawImage>();image.texture=tex;image.raycastTarget=false;}
            var p=Frame(root,806,125,390,548);Text(p,34,103,322,24,"3 DUNGEONS · 30 FLOORS",15,Cream,true,TextAnchor.MiddleCenter);
            Button(p,34,152,322,"Start Game",()=>owner.NewRun(),true,true,60);Button(p,34,229,322,"Continue",()=>owner.ContinueRun(),false,owner.HasCheckpoint,60);Button(p,34,306,322,"Settings",()=>owner.OpenSettings(),false,true,60);
            if(owner.DesktopFeatures)Button(p,34,383,322,"Exit to Desktop",()=>owner.ExitToDesktop(),false,true,60);
            Text(p,34,464,322,26,owner.HasCheckpoint?"Continue from your last rest shrine.":"No saved adventure yet.",13,Cream,false,TextAnchor.MiddleCenter);
            Frame(root,28,744,560,36,new Color(1,1,1,.95f));Text(root,44,754,532,24,owner.ControllerConnected?"D-PAD MOVE   B SWING   Y DODGE   A INTERACT   START PAUSE":owner.MobileControlsEnabled?"DRAG TO MOVE     TAP SWING, DODGE OR INTERACT":"WASD MOVE    SPACE SWING    SHIFT DODGE    E INTERACT",12,Cream,true);
        }
        void HUD()
        {
            var g=owner.Game;if(owner.MobileControlsEnabled)TouchControls();var p=Frame(root,160,18,960,84);Text(p,24,22,280,17,"DUNGEON "+new[]{"I","II","III"}[g.Dungeon],11,Gold,true);Text(p,24,43,300,28,"",21,Cream,true,key:"dungeon");
            Box(p,326,18,1,49,Edge);Text(p,348,22,100,17,"FLOOR",11,Muted,true);Text(p,348,43,100,28,"",22,Cream,true,key:"floor");Text(p,458,22,100,17,"ROOM",11,Muted,true);Text(p,458,43,100,28,"",22,Cream,true,key:"room");
            Text(p,570,22,130,18,"HEALTH",11,Muted,true);Text(p,680,22,108,18,"",12,Cream,true,TextAnchor.UpperRight,"health");Box(p,570,51,218,12,new Color(.2f,.12f,.12f));healthFill=Box(p,570,51,218,12,Red);Box(p,570,52,218,2,new Color(1,.7f,.5f,.28f));
            Box(p,815,18,1,49,Edge);Text(p,842,22,90,18,"SWORD",11,Muted,true);Text(p,842,43,90,28,"",22,Gold,true,key:"sword");
            Box(root,157,107,966,2,Edge);Box(root,157,710,966,2,Edge);Box(root,157,107,2,605,Edge);Box(root,1121,107,2,605,Edge);
            var controls=Frame(root,160,724,960,56);Text(controls,26,17,365,24,owner.ControllerConnected?"B Swing     Y Dodge     A Interact":owner.MobileControlsEnabled?"DRAG TO MOVE · TAP TO ACT":"SPACE  Swing     SHIFT  Dodge     E  Interact",14,Cream);Text(controls,414,11,140,18,"",10,Muted,true,key:"dodge");Box(controls,414,33,96,5,Edge);dodgeFill=Box(controls,414,33,96,5,Green);
            Text(controls,542,18,200,20,"",12,Gold,true,key:"effect");Button(controls,752,8,84,"Map",()=>owner.OpenMap(),false,true,40);Button(controls,846,8,96,"Pause",()=>owner.TogglePause(),false,true,40);
            Text(root,160,784,700,16,"",10,Muted,true,key:"checkpoint");Text(root,850,784,270,16,"",10,Gold,true,TextAnchor.UpperRight,"objective");
            toastBox=Frame(root,275,570,730,64,new Color(.05f,.055f,.065f,.95f));notice=Text(toastBox,40,12,650,40,"",16,Cream,false,TextAnchor.MiddleCenter);
            contextBox=Frame(root,370,657,540,40,new Color(.05f,.055f,.065f,.96f));Text(contextBox,26,9,34,24,owner.ControllerConnected?"[A]":"[E]",15,Gold,true);context=Text(contextBox,66,9,446,24,"",15,Cream);
            if(g.Current.boss&&g.Current.enemies.Exists(e=>e.hp>0)){var b=Frame(root,415,125,450,66,new Color(.045f,.055f,.07f,.95f));Text(b,25,12,400,24,GameModel.BossNames[g.Dungeon],16,Gold,true,TextAnchor.MiddleCenter);Box(b,25,45,400,6,Edge);bossFill=Box(b,25,45,400,6,Red);}
        }
        void TouchControls()
        {
            var stickPanel=Frame(root,8,472,144,144,new Color(1,1,1,.85f));stickPanel.GetComponent<Image>().raycastTarget=true;
            var stick=stickPanel.gameObject.AddComponent<TouchStick>();stick.owner=owner;var knob=Box(stickPanel,0,0,48,48,Gold,true);knob.sprite=owner.Art.Menus[1,1];knob.type=Image.Type.Sliced;knob.rectTransform.anchorMin=knob.rectTransform.anchorMax=knob.rectTransform.pivot=new Vector2(.5f,.5f);knob.rectTransform.anchoredPosition=Vector2.zero;knob.raycastTarget=false;stick.knob=knob.rectTransform;
            Text(root,12,626,136,24,"MOVE",16,Gold,true,TextAnchor.MiddleCenter);
            Button(root,1128,442,144,"Swing",()=>owner.TouchAction("swing"),true,true,64);
            Button(root,1128,520,144,"Dodge",()=>owner.TouchAction("dodge"),false,true,64);
            Button(root,1128,598,144,"Interact",()=>owner.TouchAction("interact"),false,true,64);
        }
        RectTransform Modal(string eyebrow,string title,float width,float height)
        {
            Box(root,0,0,1280,800,new Color(.015f,.02f,.03f,.82f),true);var p=Frame(root,(1280-width)/2,(800-height-56)/2,width,height+56);Text(p,48,82,width-96,20,eyebrow,12,Gold,true);Text(p,48,118,width-96,46,title,32,Cream,true);Box(p,36,170,width-72,1,Edge);navigationRoot=p;return Node(p,"Menu content",0,38,width,height);
        }
        void James()
        {
            var g=owner.Game;var p=Modal("ACCORDING TO JAMES…","Advice of questionable quality.",860,548);portrait=Picture(p,owner.NpcPortrait,38,155,120,188);
            var paper=Box(p,178,150,646,102,Color.white);paper.sprite=owner.Art.Menus[0,1];paper.type=Image.Type.Sliced;Text(p,218,170,566,62,"“"+GameModel.Advice[g.Current.advice]+"”",21,new Color(.2f,.12f,.07f),true,TextAnchor.MiddleCenter);Text(p,190,269,628,66,"",17,Muted,key:"james-result");
            Button(p,36,354,384,g.Current.giftTaken?"Gift already accepted":"Accept mystery gift",()=>owner.TalkChoice("gift"),true,!g.Current.giftTaken);Button(p,440,354,384,g.Current.fingerPulled?"Finger already pulled":"Pull his finger",()=>owner.TalkChoice("finger"),false,!g.Current.fingerPulled);
            Button(p,36,422,384,"Ask about a secret",()=>owner.TalkChoice("secret"),false,g.Current.secret);Button(p,440,422,384,"Thanks, James. I guess.",()=>owner.TalkChoice("leave"));Text(p,36,502,788,22,"No item names. No guarantees. Usually only a little regret.",12,Muted,false,TextAnchor.MiddleCenter);
        }
        void Pause()
        {
            var p=Modal("A MOMENT OF QUESTIONABLE JUDGMENT","Take a breather.",600,560);Text(p,36,154,528,55,"James is probably preparing another suggestion.\nYou may wish to remain paused.",17,Muted);Button(p,36,227,528,"Back to the dungeon",()=>owner.Resume(),true);Button(p,36,293,528,"Settings",()=>owner.OpenSettings());Button(p,36,359,528,"Return to title",()=>owner.ReturnToTitle());if(owner.DesktopFeatures)Button(p,36,425,528,"Exit to Desktop",()=>owner.ExitToDesktop());Text(p,36,504,528,28,"Only rest shrines save progress.",13,Gold,false,TextAnchor.MiddleCenter);
        }
        void Settings()
        {
            var p=Modal("MAKE YOUR OWN BAD DECISIONS","Settings",800,640);
            Text(p,36,145,728,20,"STARTING DIFFICULTY",12,Gold,true);Text(p,36,170,728,24,"Applies to new runs. Every floor grows harder.",16,Muted);
            float[] values={.75f,1,1.5f};string[] names={"Mellow","Normal","Spicy"};for(int i=0;i<3;i++){float v=values[i];Button(p,36+i*244,194,232,names[i],()=>{owner.SetDifficulty(v);Invalidate();},owner.Initial==v,true,44);}
            Text(p,36,251,728,20,"MUSIC VOLUME",12,Gold,true);Text(p,184,279,432,24,Mathf.RoundToInt(owner.Volume*100)+"%",20,Cream,true,TextAnchor.MiddleCenter);
            Button(p,36,273,132,"Quieter",()=>{owner.SetVolume(owner.Volume-.1f);Invalidate();},false,true,42);Button(p,632,273,132,"Louder",()=>{owner.SetVolume(owner.Volume+.1f);Invalidate();},false,true,42);Box(p,208,314,384,5,Edge);Box(p,208,314,384*owner.Volume,5,Gold);
            Button(p,36,334,354,"Reduced motion: "+(owner.ReducedMotion?"On":"Off"),()=>{owner.SetReducedMotion(!owner.ReducedMotion);Invalidate();},false,true,46);
            if(owner.DesktopFeatures)Button(p,410,334,354,"Display: "+(owner.Fullscreen?"Fullscreen":"Windowed"),()=>owner.SetFullscreen(!owner.Fullscreen),false,true,46);
            else Text(p,410,345,354,24,"Landscape touch controls",16,Muted,false,TextAnchor.MiddleCenter);
            Text(p,36,398,728,20,"GAME UPDATES · GITHUB",12,Gold,true);
            if(owner.DesktopFeatures){Button(p,36,426,354,owner.Updater.Busy?"Please wait…":"Check for updates",()=>owner.Updater.Check(),false,!owner.Updater.Busy,46);
                Button(p,410,426,354,owner.Updater.Downloaded?"Open download folder":"Download update",()=>{if(owner.Updater.Downloaded)owner.Updater.OpenDownload();else owner.Updater.Download();},owner.Updater.CanDownload,owner.Updater.CanDownload||owner.Updater.Downloaded,46);
                Text(p,44,489,712,50,"",16,Cream,false,TextAnchor.MiddleCenter,"update-status");}
            else {Button(p,36,426,728,"View GitHub releases",()=>Application.OpenURL("https://github.com/dustindwayne/according-to-james/releases/latest"),false,true,46);Text(p,44,489,712,50,"Version "+BuildVersion.Current,16,Cream,false,TextAnchor.MiddleCenter);}
            Text(p,36,533,728,20,"SNES: B Swing · Y Dodge · A Interact · X Map · Start Pause",13,Muted,false,TextAnchor.MiddleCenter);
            Button(p,36,561,728,"Done",()=>owner.CloseSettings(),true,true,50);
        }
        void Map()
        {
            var g=owner.Game;var p=Modal("YOUR BAD IDEA, MAPPED","Floor "+g.floor.ToString("00")+" · Room map",700,560);Text(p,36,147,628,25,"Clear every room to open the stairs.",16,Muted);
            for(int i=0;i<g.rooms.Count;i++){float x=75+i%2*310,y=196+i/2*88;foreach(var neighbor in g.Neighbors(i))if(neighbor.Value>i){if(neighbor.Key=="right")Box(p,x+240,y+33,70,2,Edge);if(neighbor.Key=="down")Box(p,x+119,y+68,2,20,Edge);}}
            for(int i=0;i<g.rooms.Count;i++){var room=g.rooms[i];float x=75+i%2*310,y=196+i/2*88;var node=Frame(p,x,y,240,68,i==g.room?new Color(.23f,.18f,.10f):Panel);Text(node,28,18,184,19,"ROOM "+(i+1).ToString("00")+(i==g.room?" · YOU":""),14,i==g.room?Gold:Cream,true,TextAnchor.MiddleCenter);Text(node,28,39,184,18,!room.visited?"Unexplored":room.cleared?"Cleared":"Enemies remain",12,room.visited&&room.cleared?Green:Muted,false,TextAnchor.MiddleCenter);}
            Button(p,36,475,628,"Back to the dungeon",()=>owner.CloseMap(),true);
        }
        void Ending()
        {
            var g=owner.Game;bool won=g.status=="won";var p=Modal(won?"THIRTY FLOORS OF TERRIBLE ADVICE":"JAMES WOULD CALL THIS A LEARNING EXPERIENCE",won?"You survived James.":"James was wrong.",680,490);Text(p,36,160,608,54,won?"Three dungeons. Three ridiculous bosses.\nSomehow, you made it.":"You reached floor "+g.floor+".\n"+(owner.HasCheckpoint?"The last rest shrine remembers your progress.":"No rest shrine was saved. James forgot to mention that."),18,Muted);
            if(won)Button(p,36,239,608,"Another bad idea",()=>owner.NewRun(),true);
            else if(owner.HasCheckpoint){Button(p,36,239,608,"Return to last rest shrine",()=>owner.ContinueRun(),true);Button(p,36,305,608,"Start a new run",()=>owner.NewRun());}
            else Button(p,36,239,608,"Start a new run",()=>owner.NewRun(),true);
            Button(p,36,!won&&owner.HasCheckpoint?371:305,608,"Return to title",()=>owner.ReturnToTitle());
        }
        public void VerifyTouchControls()
        {
            Refresh();Canvas.ForceUpdateCanvases();var stick=root.GetComponentInChildren<TouchStick>();if(!stick)throw new Exception("Missing touch stick");var rect=(RectTransform)stick.transform;var point=rect.TransformPoint(rect.rect.center+new Vector2(40,40));var e=new PointerEventData(EventSystem.current){pointerId=41,position=RectTransformUtility.WorldToScreenPoint(uiCamera,point),pointerPressRaycast=new RaycastResult{module=canvas.GetComponent<GraphicRaycaster>()}};
            stick.OnPointerDown(e);if(owner.TouchMove.x<.3f||owner.TouchMove.y>-.3f)throw new Exception("Touch stick direction mismatch");stick.OnPointerUp(e);if(owner.TouchMove!=Vector2.zero)throw new Exception("Touch movement did not release");
            PressForCheck("Swing");if(owner.Game.player.attack<=0)throw new Exception("Touch swing failed");PressForCheck("Dodge");if(owner.Game.player.dodge<=0)throw new Exception("Touch dodge failed");Debug.Log("ACCORDING_TO_JAMES_TOUCH_OK: directional input, release, swing and dodge");
        }
        public void ValidateTextFit()
        {
            Refresh();Canvas.ForceUpdateCanvases();var failures=new List<string>();
            foreach(var label in root.GetComponentsInChildren<Text>())
            {
                if(!label.gameObject.activeInHierarchy||string.IsNullOrWhiteSpace(label.text))continue;
                var rect=label.rectTransform.rect;var settings=label.GetGenerationSettings(rect.size);
                if(label.resizeTextForBestFit){label.cachedTextGenerator.Populate(label.text,settings);settings.fontSize=label.cachedTextGenerator.fontSizeUsedForBestFit;settings.resizeTextForBestFit=false;}
                float required=label.cachedTextGeneratorForLayout.GetPreferredHeight(label.text,settings)/label.pixelsPerUnit;
                if(required>rect.height+2)failures.Add(label.gameObject.name+" needs "+required.ToString("0.0")+"px; has "+rect.height.ToString("0.0")+"px");
                var parent=label.transform.parent as RectTransform;
                if(parent){var position=label.rectTransform.anchoredPosition;if(position.x<-.5f||-position.y<-.5f||position.x+rect.width>parent.rect.width+.5f||-position.y+rect.height>parent.rect.height+.5f)failures.Add(label.gameObject.name+" lies outside its panel");}
            }
            if(failures.Count>0)throw new Exception("UI text fit: "+string.Join("; ",failures));
        }
        public void RenderTo(RenderTexture target){Refresh();Canvas.ForceUpdateCanvases();uiCamera.targetTexture=target;uiCamera.rect=new Rect(0,0,1,1);uiCamera.Render();uiCamera.targetTexture=null;Canvas.ForceUpdateCanvases();}
        void OnDestroy(){if(canvas)Destroy(canvas.gameObject);if(uiCamera)Destroy(uiCamera.gameObject);}
    }
}
