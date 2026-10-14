using System;
using System.Collections.Generic;

namespace AccordingToJames
{
    [Serializable] public class Actor
    {
        public float x,y,hp,maxHp,inv,dodge,dodgeCooldown,attack,attackCooldown,slow,dx,dy,walkTime;
        public string dir="down"; public bool moving;
    }
    [Serializable] public class Enemy : Actor
    {
        public int id,kind,phase,hitSwing=-1;
        public bool boss; public float attackTimer=1.8f,flash,brainTimer; public string state="patrol"; public float targetX,targetY;
    }
    [Serializable] public class Block { public float x,y,w,h; public Block(float x,float y,float w,float h){this.x=x;this.y=y;this.w=w;this.h=h;} }
    [Serializable] public class Shot { public float x,y,vx,vy,life,age; public string type; }
    [Serializable] public class Room
    {
        public int id,advice;public string role; public float chestAnim; public bool boss,visited,cleared,loot,looted,james,chest,chestOpened,chestTrap,secret,secretKnown,secretUsed,secretTrap,giftTaken,fingerPulled,shrine;
        public List<Enemy> enemies=new List<Enemy>(); public List<Block> obstacles=new List<Block>(),spikes=new List<Block>(); public List<Shot> shots=new List<Shot>();
    }
    public class GameEvent { public string type; public int value; public GameEvent(string t,int v=0){type=t;value=v;} }
    [Serializable] public class GameModel
    {
        public int version=3,floor=1,room=0,seed=71337,swingId,randomState,checkpointFloor;
        public float initial=1,time,boost,dizzy,fall; public string status="playing",message="";
        public Actor player; public List<Room> rooms;
        [NonSerialized] public List<GameEvent> events=new List<GameEvent>();
        public static readonly string[] DungeonNames={"Butterscotch Cellars","The Scorched Pantry","Grumble's Midnight Kitchen"};
        public const float FloorLeft=95,FloorRight=865,FloorTop=125,FloorBottom=475;
        public const float SecretX=270,SecretY=145;
        public static readonly string[] BossNames={"Butterscotch Granny","Lunch Lady Ladle Fury","Chef Grumble"};
        public static readonly string[] Advice={"Drink that. It's probably fine.","Step on the trap. Builds character.","Catch the candy with your face.","Stand in the green cloud.","Fight all of them. You got this.","That cafeteria tray is a hug.","Flour is just free seasoning.","Sausages are a shortcut. Trust me.","Go down another floor. It gets easier."};
        public static readonly string[] Followup={"What could possibly go wrong?","Traps are just floor hugs.","Dentists hate this one trick.","It is probably a healing mist.","I'll watch from way over here.","Go on. Get a little closer.","Breathe deep. Tastes like pie.","Those sausages are decorative.","I have absolutely no evidence for that."};
        public int Dungeon {get{return (floor-1)/10;}}
        public float Scale {get{return initial*(float)Math.Pow(1.08,floor-1);}}
        public int Sword {get{return 4+(floor-1)/3*2+(boost>0?2:0);}}
        public Room Current {get{return rooms[room];}}
        public bool AllCleared {get{return rooms.TrueForAll(r=>r.cleared&&r.visited);}}
        public static int RoomCount(int f){return 4+(f-1)%3;}
        public GameModel(float difficulty=1,int seed=71337){initial=difficulty;this.seed=seed;randomState=seed;NewFloor();}
        public int Roll(int max){randomState=unchecked(randomState*1664525+1013904223);return (int)((uint)randomState%(uint)max);}
        public void Notice(string text){message=text;Emit("notice");}
        // James and chest mishaps never kill or remove more than 8% of maximum health.
        public void Mishap(string text){player.hp=Math.Max(1,player.hp-Math.Max(1,(float)Math.Floor(player.maxHp*.08f)));dizzy=5;Notice(text);}
        public void MysteryItem(){switch(Roll(4)){case 0:player.hp=Math.Min(player.maxHp,player.hp+Math.Max(4,(float)Math.Ceiling(player.maxHp*.25f)));Notice("You try what James handed you. Warmth spreads through you. Health restored.");break;case 1:boost=20;Notice("You try the mystery gift. Your sword arm feels unstoppable. For a little while.");break;case 2:dizzy=8;Notice("James gave you a beer. The room feels a little wobbly. That will pass.");break;default:Mishap("You try the mystery gift. A tiny stomach rebellion. James calls it character.");break;}}
        public void JamesChoice(string choice){if(status!="talking")return;var r=Current;if(choice=="gift"&&!r.giftTaken){r.giftTaken=true;MysteryItem();}else if(choice=="finger"&&!r.fingerPulled){r.fingerPulled=true;if(Roll(2)==0){dizzy=4;Notice("You pull his finger. James farts. Your movement feels sluggish for four seconds.");Emit("fart");}else MysteryItem();}else if(choice=="secret"){r.secretKnown=true;Notice("James: That loose stone on the left? Definitely a shortcut. Probably.");}else if(choice=="leave")status="playing";}
        public void Emit(string type,int value=0){if(events==null)events=new List<GameEvent>();events.Add(new GameEvent(type,value));}
        public void NewFloor()
        {
            rooms=new List<Room>();int count=RoomCount(floor);
            for(int id=0;id<count;id++)
            {
                bool boss=floor%10==0&&id==count-1,peaceful=id==0||id==2;
                Room r=new Room{id=id,role=id==0?"entry":id==2?"treasure":boss?"boss":id==count-1?"exit":"combat",boss=boss,cleared=peaceful,loot=false,james=id==0||(id==2&&floor%3==0),advice=(floor+id*3)%Advice.Length,chest=id==2,chestTrap=Roll(3)==0,secret=id==0,secretTrap=Roll(3)==0,shrine=id==0&&(floor%10==1||floor%10==5||floor%10==9)};
                if(!peaceful&&!boss){int kind=(floor+id)%3;if(kind==0)r.obstacles.Add(new Block(275,260,76,68));if(kind==1)r.obstacles.Add(new Block(605,320,68,76));if(kind==2){r.obstacles.Add(new Block(325,180,65,58));r.obstacles.Add(new Block(600,390,65,58));}if(floor>2)r.spikes.Add(new Block(445,300,70,44));}
                int number=peaceful?0:boss?1:Math.Min(4,2+Dungeon);
                for(int i=0;i<number;i++){float hp=(float)Math.Ceiling((boss?35:4)*Scale);r.enemies.Add(new Enemy{id=i,kind=boss?Dungeon:(floor+i)%3,boss=boss,x=boss?480:new float[]{210,390,610,760}[i],y=boss?235:i%2==0?225:385,hp=hp,maxHp=hp,targetX=200+i*150,targetY=350});}
                rooms.Add(r);
            }
            float health=player==null?32+floor*2:Math.Min(32+floor*2,player.hp+2);player=new Actor{x=480,y=450,dir="up",hp=health,maxHp=32+floor*2,dy=-1};boost=dizzy=fall=0;room=0;rooms[0].visited=true;Emit("floor",floor);
        }
        public Dictionary<string,int> Neighbors(int id)
        {
            var n=new Dictionary<string,int>();if(id%2==1)n["left"]=id-1;else if(id+1<rooms.Count)n["right"]=id+1;if(id>=2)n["up"]=id-2;if(id+2<rooms.Count)n["down"]=id+2;return n;
        }
        public bool EnterRoom(int id,string side="")
        {
            if(id<0||id>=rooms.Count)return false;bool previouslyCleared=AllCleared;room=id;
            player.x=side=="left"?865:side=="right"?95:480;player.y=side=="left"||side=="right"?310:side=="up"?470:side=="down"?125:450;
            player.inv=.8f;player.attack=0;player.moving=false;Current.visited=true;Emit("room",id);if(!Current.cleared)Notice("The portcullises drop. Clear this room to lift them.");if(!previouslyCleared&&AllCleared)Emit("stairsOpen");return true;
        }
        public static float Distance(float x,float y){return (float)Math.Sqrt(x*x+y*y);}
        public int DoorState(string side){return !Neighbors(room).ContainsKey(side)?0:Current.cleared?3:2;}
        public bool DoorOpen(string side){return DoorState(side)==3;}
        public bool FloorAllows(float x,float y)
        {
            if(x>=FloorLeft&&x<=FloorRight&&y>=FloorTop&&y<=FloorBottom)return true;
            if(DoorOpen("up")&&x>=438&&x<=522&&y>=30&&y<FloorTop)return true;
            if(DoorOpen("down")&&x>=438&&x<=522&&y>FloorBottom&&y<=565)return true;
            if(DoorOpen("left")&&y>=276&&y<=344&&x>=30&&x<FloorLeft)return true;
            if(DoorOpen("right")&&y>=276&&y<=344&&x>FloorRight&&x<=930)return true;return false;
        }
        public bool Blocked(float x,float y)
        {
            if(!FloorAllows(x,y))return true;
            if(Current.chest&&Math.Abs(x-205)<40&&Math.Abs(y-235)<21)return true;
            if(Current.shrine&&Math.Abs(x-710)<31&&Math.Abs(y-420)<17)return true;
            if(Current.james&&Distance(x-480,y-235)<23)return true;
            if(room==rooms.Count-1&&!AllCleared&&x>762&&x<848&&y>155&&y<233)return true;
            return Current.obstacles.Exists(o=>x+13>o.x&&x-13<o.x+o.w&&y+8>o.y&&y-8<o.y+o.h);
        }
        public void Move(Actor a,float dx,float dy)
        {
            float ox=a.x,oy=a.y;if(!Blocked(a.x+dx,a.y))a.x+=dx;if(!Blocked(a.x,a.y+dy))a.y+=dy;
            float dist=Distance(a.x-ox,a.y-oy);a.moving=dist>.01f;if(a.moving)a.walkTime+=dist/(a is Enemy?70:100);
        }
        public bool Attack()
        {
            if(status!="playing"||player.attackCooldown>0)return false;player.attack=.17f;player.attackCooldown=.34f;swingId++;Emit("swing");return true;
        }
        public bool Dodge(float dx=0,float dy=0)
        {
            if(status!="playing"||player.dodgeCooldown>0)return false;if(dx==0&&dy==0){dx=player.dir=="left"?-1:player.dir=="right"?1:0;dy=player.dir=="up"?-1:player.dir=="down"?1:0;}
            float len=Distance(dx,dy);player.dx=dx/len;player.dy=dy/len;player.dodge=.22f;player.dodgeCooldown=1.25f;player.slow=0;Emit("dodge");return true;
        }
        public bool Hurt(float damage)
        {
            if(status!="playing"||player.inv>0||player.dodge>0)return false;player.hp=Math.Max(0,player.hp-damage);player.inv=.72f;Emit("hurt");if(player.hp==0){status="dead";Emit("dead");}return true;
        }
        public string Context()
        {
            if(Current.shrine&&Distance(player.x-710,player.y-420)<75)return "checkpoint";
            if(Current.chest&&!Current.chestOpened&&Distance(player.x-205,player.y-235)<85)return "chest";
            if(Current.secret&&Current.secretKnown&&!Current.secretUsed&&Distance(player.x-SecretX,player.y-SecretY)<75)return "secret";
            if(Current.james&&Distance(player.x-480,player.y-235)<105)return "james";
            if(Current.loot&&!Current.looted&&Distance(player.x-195,player.y-210)<100)return "loot";
            if(room==rooms.Count-1&&Distance(player.x-805,player.y-200)<75)return AllCleared?"stairs":"locked";
            if(!Current.cleared&&((player.x<160&&Math.Abs(player.y-310)<65)||(player.x>800&&Math.Abs(player.y-310)<65)||(player.y<170&&Math.Abs(player.x-480)<65)||(player.y>430&&Math.Abs(player.x-480)<65)))return "barred";return "";
        }
        public bool Interact()
        {
            if(status!="playing")return false;string c=Context();if(c=="james"){message="";status="talking";Emit("james",Current.advice);return true;}
            if(c=="checkpoint"){checkpointFloor=floor;player.hp=player.maxHp;Emit("checkpoint");Notice("Rest shrine: health restored and progress saved here.");return true;}
            if(c=="chest"){Current.chestOpened=true;Current.chestAnim=.8f;if(Current.chestTrap){Mishap("The chest sneezes pepper in your face. A small sting, then something falls out.");boost=10;}else{MysteryItem();message=message.Replace("James handed you","you found in the chest").Replace("James gave you a beer.","The chest held something drinkable.");}return true;}
            if(c=="barred"){Notice("Defeat this room's enemies to lift the portcullises.");return false;}
            if(c=="secret"){bool trap=Current.secretTrap;Current.secretUsed=true;int destination=rooms.Count-1;EnterRoom(destination);player.x=740;player.y=450;if(trap){fall=.8f;Mishap("James's shortcut works. The landing is a shallow pit. A small bruise, same destination.");}else{boost=15;Notice("The secret passage works! You found the final room and a brief sword boost.");}return true;}
            if(c=="loot"){player.hp=player.maxHp;Current.looted=true;Emit("heal");return true;}
            if(c=="locked"){Emit("locked");return false;}
            if(c=="stairs"){if(floor==30){status="won";Emit("won");}else{floor++;NewFloor();}return true;}return false;
        }
        public bool SwingHits(Enemy e)
        {
            float x=e.x-player.x,y=e.y-player.y,dx=player.dir=="left"?-1:player.dir=="right"?1:0,dy=player.dir=="up"?-1:player.dir=="down"?1:0;
            return Distance(x,y)<90&&x*dx+y*dy>-12;
        }
        public void AddShot(float x,float y,float vx,float vy,string type,float life=3)
        {
            if(Current.shots.Count>30)return;Current.shots.Add(new Shot{x=x,y=y,vx=vx,vy=vy,type=type,life=life});
        }
        public void BossAttack(Enemy e)
        {
            e.phase++;float distance=Distance(player.x-e.x,player.y-e.y);if(distance<.01f)distance=1;
            float ax=(player.x-e.x)/distance,ay=(player.y-e.y)/distance;
            if(e.kind==0){if(e.phase%3==0){AddShot(e.x-20,e.y+20,0,0,"gas",4.5f);AddShot(e.x+25,e.y-5,0,0,"gas",4.5f);}else foreach(float a in new[]{-.3f,0,.3f})AddShot(e.x,e.y,(ax*(float)Math.Cos(a)-ay*(float)Math.Sin(a))*170,(ax*(float)Math.Sin(a)+ay*(float)Math.Cos(a))*170,"candy");}
            if(e.kind==1){foreach(float a in new[]{-.18f,.18f})AddShot(e.x,e.y,(ax*(float)Math.Cos(a)-ay*(float)Math.Sin(a))*230,(ax*(float)Math.Sin(a)+ay*(float)Math.Cos(a))*230,"tray");if(e.phase%2==0)AddShot(player.x,player.y,0,0,"gravy",3.8f);}
            if(e.kind==2){for(int i=0;i<6;i++){float a=i*(float)Math.PI/3+e.phase*.3f;AddShot(e.x,e.y,(float)Math.Cos(a)*240,(float)Math.Sin(a)*240,"sausage");}if(e.phase%2==0){AddShot(player.x,player.y,0,0,"flour",4);AddShot(player.x+60,player.y-25,0,0,"flour",4);}}
            Emit("bossAttack",e.kind);
        }
        public void Tick(float dt,float dx=0,float dy=0)
        {
            if(status!="playing")return;dt=Math.Max(0,Math.Min(.04f,dt));time+=dt;Actor p=player;Room r=Current;
            if(fall>0){fall=Math.Max(0,fall-dt);p.moving=false;return;}
            p.inv=Math.Max(0,p.inv-dt);p.attack=Math.Max(0,p.attack-dt);p.attackCooldown=Math.Max(0,p.attackCooldown-dt);p.dodge=Math.Max(0,p.dodge-dt);p.dodgeCooldown=Math.Max(0,p.dodgeCooldown-dt);p.slow=Math.Max(0,p.slow-dt);
            float len=Distance(dx,dy);if(len>1){dx/=len;dy/=len;}if(len>.15f&&p.dodge<=0)p.dir=Math.Abs(dx)>Math.Abs(dy)?dx<0?"left":"right":dy<0?"up":"down";
            boost=Math.Max(0,boost-dt);dizzy=Math.Max(0,dizzy-dt);
            if(p.dodge>0){dx=p.dx;dy=p.dy;}float speed=p.dodge>0?380:p.slow>0||dizzy>0?145:190;Move(p,dx*speed*dt,dy*speed*dt);
            var n=Neighbors(room);
            r.chestAnim=Math.Max(0,r.chestAnim-dt);
            if(p.x<45&&Math.Abs(p.y-310)<34&&dx<0&&DoorOpen("left")){EnterRoom(n["left"],"left");return;}
            if(p.x>915&&Math.Abs(p.y-310)<34&&dx>0&&DoorOpen("right")){EnterRoom(n["right"],"right");return;}
            if(p.y<50&&Math.Abs(p.x-480)<42&&dy<0&&DoorOpen("up")){EnterRoom(n["up"],"up");return;}
            if(p.y>550&&Math.Abs(p.x-480)<42&&dy>0&&DoorOpen("down")){EnterRoom(n["down"],"down");return;}
            foreach(Enemy e in r.enemies)
            {
                e.moving=false;if(e.hp<=0)continue;e.flash=Math.Max(0,e.flash-dt);
                if(p.attack>0.04f&&p.attack<=.125f&&e.hitSwing!=swingId&&SwingHits(e)){e.hitSwing=swingId;e.hp=Math.Max(0,e.hp-Sword);e.flash=.16f;Emit("hit");if(e.hp==0)Emit("kill",e.boss?1:0);}
                if(e.hp<=0)continue;float dist=Distance(p.x-e.x,p.y-e.y);if(dist<.01f)dist=1;
                e.dir=Math.Abs(p.x-e.x)>Math.Abs(p.y-e.y)?p.x<e.x?"left":"right":p.y<e.y?"up":"down";
                Think(e,dt,dist);
                if(dist<33&&e.state=="strike")Hurt(Math.Max(1,(float)Math.Round(2*Math.Sqrt(Scale))));
                if(e.boss){e.attackTimer-=dt;if(e.attackTimer<=0){BossAttack(e);e.attackTimer=Math.Max(.9f,2.5f/(float)Math.Pow(Scale,.25));}}
            }
            if(!r.cleared&&r.enemies.TrueForAll(e=>e.hp<=0)){r.cleared=true;r.shots.Clear();Emit("clear",r.boss?1:0);if(AllCleared)Emit("stairsOpen");}
            foreach(Shot s in r.shots){s.age+=dt;s.life-=dt;s.x+=s.vx*dt;s.y+=s.vy*dt;if(s.x<25||s.x>935||s.y<55||s.y>555)s.life=0;bool cloud=s.type=="gas"||s.type=="flour"||s.type=="gravy";if(s.age>(cloud ? 0.45f : 0)&&Distance(p.x-s.x,p.y-s.y)<(cloud?38:18)){if(s.type=="gravy"&&p.dodge<=0)p.slow=1.2f;Hurt(Math.Max(1,(float)Math.Round((cloud?2:3)*Math.Sqrt(Scale))));if(!cloud)s.life=0;}}
            r.shots.RemoveAll(s=>s.life<=0);foreach(Block s in r.spikes)if(p.x>s.x&&p.x<s.x+s.w&&p.y>s.y-6&&p.y<s.y+s.h)Hurt(3);
        }
        // Breadth-first navigation around room props; enemies make decisions every 0.4s.
        void Route(Enemy e,float tx,float ty)
        {
            const int w=23,h=11;int sx=Math.Max(0,Math.Min(w-1,(int)((e.x-95)/35))),sy=Math.Max(0,Math.Min(h-1,(int)((e.y-125)/35))),gx=Math.Max(0,Math.Min(w-1,(int)((tx-95)/35))),gy=Math.Max(0,Math.Min(h-1,(int)((ty-125)/35)));
            int start=sy*w+sx,goal=gy*w+gx;var prev=new int[w*h];for(int i=0;i<prev.Length;i++)prev[i]=-1;var q=new Queue<int>();q.Enqueue(start);prev[start]=start;
            while(q.Count>0){int c=q.Dequeue();if(c==goal)break;foreach(int d in new[]{-1,1,-w,w}){int n=c+d;if(n<0||n>=prev.Length||prev[n]>=0||(Math.Abs(d)==1&&n/w!=c/w)||Blocked(95+n%w*35,125+n/w*35))continue;prev[n]=c;q.Enqueue(n);}}
            if(prev[goal]<0){e.targetX=e.x;e.targetY=e.y;return;}int next=goal;while(prev[next]!=start&&next!=start)next=prev[next];e.targetX=95+next%w*35;e.targetY=125+next/w*35;
        }
        void Think(Enemy e,float dt,float dist)
        {
            e.brainTimer-=dt;
            if(e.state=="windup"){if(e.brainTimer<=0){e.state="strike";e.brainTimer=.2f;if(!e.boss&&e.kind==1){float len=Math.Max(1,dist);AddShot(e.x,e.y,(player.x-e.x)/len*180,(player.y-e.y)/len*180,"candy");}}return;}
            if(e.state=="strike"){if(e.brainTimer<=0){e.state="recover";e.brainTimer=.75f;}return;}
            if(e.state=="recover"){if(e.brainTimer<=0)e.state="chase";return;}
            bool sees=dist<390||e.boss;
            if(sees&&(dist<47||(!e.boss&&e.kind==1&&dist<260))){e.state="windup";e.brainTimer=.5f;return;}
            if(e.brainTimer<=0){e.brainTimer=.4f;if(!sees){e.state="patrol";Route(e,150+Roll(650),130+Roll(350));}else if(!e.boss&&e.kind==2&&e.hp<e.maxHp*.5f){e.state="retreat";Route(e,e.x+(e.x-player.x),e.y+(e.y-player.y));}else{e.state="chase";Route(e,player.x,player.y);}}
            float vx=e.targetX-e.x,vy=e.targetY-e.y,len2=Distance(vx,vy);if(len2>6){float speed=e.boss?28:55+Dungeon*10;Move(e,vx/len2*speed*dt,vy/len2*speed*dt);}
        }
        public bool Validate()
        {
            if((version!=2&&version!=3)||floor<1||floor>30||rooms==null||rooms.Count!=RoomCount(floor)||room<0||room>=rooms.Count||player==null||player.hp<=0||float.IsNaN(player.x)||float.IsNaN(player.y))return false;
            version=3;foreach(var r in rooms)if(string.IsNullOrEmpty(r.role))r.role=r.id==0?"entry":r.id==2?"treasure":r.boss?"boss":r.id==rooms.Count-1?"exit":"combat";
            if(initial!=.75f&&initial!=1&&initial!=1.5f)return false;status="playing";events=new List<GameEvent>();player.inv=.8f;player.moving=false;return true;
        }
    }
}

