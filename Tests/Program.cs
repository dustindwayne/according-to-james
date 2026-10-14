using AccordingToJames;
static void Check(bool value,string name){if(!value)throw new Exception(name);}
var g=new GameModel();float prev=0;int bosses=0;
for(int floor=1;floor<=30;floor++){
 Check(g.floor==floor,"floor progression");Check(g.Scale>prev,"monotonic scale");prev=g.Scale;
 var seen=new HashSet<int>{0};var q=new Queue<int>();q.Enqueue(0);while(q.Count>0)foreach(var id in g.Neighbors(q.Dequeue()).Values)if(seen.Add(id))q.Enqueue(id);Check(seen.Count==g.rooms.Count,"connected rooms");
 for(int room=0;room<g.rooms.Count;room++){
  g.EnterRoom(room);g.player.inv=9999;
  if(g.Current.boss){bosses++;var e=g.Current.enemies[0];for(int i=0;i<3;i++)g.BossAttack(e);var types=g.Current.shots.Select(s=>s.type).ToHashSet();string[] expected=e.kind==0?new[]{"candy","gas"}:e.kind==1?new[]{"tray","gravy"}:new[]{"sausage","flour"};foreach(var type in expected)Check(types.Contains(type),"distinct boss attack");}
  foreach(var e in g.Current.enemies){int hits=0;while(e.hp>0){g.player.x=480;g.player.y=360;g.player.dir="up";g.player.inv=9999;g.player.attackCooldown=0;e.x=480;e.y=310;g.Attack();g.Tick(.01f);g.Tick(.04f);Check(++hits<100,"defeat enemies");}}
  g.Tick(.01f);Check(g.Current.cleared,"room clear");
 }
 Check(g.AllCleared,"stairs gate");g.player.x=825;g.player.y=145;Check(g.Interact(),"stairs interact");
}
Check(g.status=="won"&&bosses==3,"campaign victory");
var a=new GameModel();a.player.x=480;a.player.y=210;Check(a.Interact()&&a.status=="talking","James conversation");a.JamesChoice("gift");Check(a.Current.giftTaken,"gift consumed once");int rng=a.randomState;a.JamesChoice("gift");Check(a.randomState==rng,"no gift farming");a.JamesChoice("finger");Check(a.Current.fingerPulled,"finger interaction");rng=a.randomState;a.JamesChoice("finger");Check(a.randomState==rng,"no finger farming");a.JamesChoice("secret");Check(a.Current.secretKnown,"secret reveal");a.JamesChoice("leave");
a=new GameModel();a.Dodge(1,0);Check(!a.Hurt(5),"dodge invulnerability");a.player.dodge=0;a.player.inv=0;Check(a.Hurt(5),"contact damage");
a=new GameModel();a.Tick(.04f,1,0);Check(a.player.moving&&a.player.walkTime>0,"walk cycle advances");var phase=a.player.walkTime;a.Tick(.04f);Check(!a.player.moving&&a.player.walkTime==phase,"idle stops walk cycle");
for(int floor=1;floor<=30;floor++){a=new GameModel();a.floor=floor;a.NewFloor();Check(a.Current.shrine==(floor%10==1||floor%10==5||floor%10==9),"limited checkpoint schedule");if(a.Current.shrine){a.player.x=710;a.player.y=420;Check(a.Interact()&&a.checkpointFloor==floor&&a.events.Any(e=>e.type=="checkpoint"),"explicit checkpoint interaction");}}
for(int seed=1;seed<=24;seed++){a=new GameModel(1,seed);a.player.hp=1;for(int i=0;i<30;i++)a.MysteryItem();Check(a.player.hp>=1&&a.status=="playing","mystery items cannot kill");}
foreach(bool trap in new[]{false,true}){a=new GameModel();a.EnterRoom(2);a.Current.chestTrap=trap;a.player.x=195;a.player.y=210;a.player.hp=1;Check(a.Interact()&&a.Current.chestOpened&&a.player.hp>=1,"trapped and normal chests");rng=a.randomState;Check(!a.Interact()&&a.randomState==rng,"one chest use");a=new GameModel();a.Current.secretKnown=true;a.Current.secretTrap=trap;a.player.x=GameModel.SecretX;a.player.y=GameModel.SecretY;a.player.hp=1;Check(a.Interact()&&a.room==a.rooms.Count-1&&a.player.hp>=1,"real and pitfall shortcuts");Check(!a.AllCleared,"shortcut does not bypass combat gate");}
a=new GameModel();a.EnterRoom(1);a.Current.enemies.Clear();var chaser=new Enemy{x=300,y=380,hp=10,maxHp=10,kind=0};a.Current.enemies.Add(chaser);a.Current.obstacles.Clear();a.Current.obstacles.Add(new Block(260,240,90,80));a.player.x=300;a.player.y=155;a.player.inv=999;for(int i=0;i<320;i++)a.Tick(.04f);Check(chaser.y<240,"AI navigates around obstacle");
a=new GameModel();a.EnterRoom(1);a.Current.enemies.Clear();var ranged=new Enemy{x=300,y=200,hp=10,maxHp=10,kind=1};a.Current.enemies.Add(ranged);a.player.x=480;a.player.y=200;for(int i=0;i<15;i++)a.Tick(.04f);Check(a.Current.shots.Count>0,"ranged enemy telegraphs then shoots");
a=new GameModel();a.EnterRoom(1);a.Current.enemies.Clear();a.Current.obstacles.Clear();var coward=new Enemy{x=350,y=200,hp=1,maxHp=10,kind=2};a.Current.enemies.Add(coward);a.player.x=270;a.player.y=200;a.Tick(.04f);Check(coward.state=="retreat"&&coward.x>350,"wounded enemy retreats");
var fingerResults=new HashSet<bool>();var itemResults=new HashSet<string>();for(int seed=1;seed<=16;seed++){a=new GameModel(1,seed);a.status="talking";a.JamesChoice("finger");fingerResults.Add(a.events.Any(e=>e.type=="fart"));a=new GameModel(1,seed);a.MysteryItem();itemResults.Add(a.message);a.player.hp=a.player.maxHp;a.Mishap("test");Check(a.player.maxHp-a.player.hp<=a.player.maxHp*.08f,"mishap damage cap");}Check(fingerResults.Count==2&&itemResults.Count==4,"all finger and item outcomes");
Console.WriteLine("PASS: 30-floor campaign, bosses, checkpoints, safe gifts, finger pulls, chests, secret pitfalls, pathfinding, ranged AI, retreat, walking and victory.");
LayoutAudit.Run();

a=new GameModel();float minJames=480,maxJames=480;for(int i=0;i<150;i++){a.Tick(.04f);minJames=Math.Min(minJames,a.JamesX);maxJames=Math.Max(maxJames,a.JamesX);}Check(maxJames-minJames>60,"James paces");Check(minJames>=425&&maxJames<=535,"James stays in safe pacing area");a.player.x=a.JamesX;a.player.y=a.JamesY+65;float stop=a.JamesX;for(int i=0;i<50;i++)a.Tick(.04f);Check(a.JamesX==stop,"James stops near player");Check(a.Context()=="james"&&a.Interact(),"moving James remains interactable");Console.WriteLine("PASS: James pacing, safe bounds, proximity stop and conversation interaction.");

foreach(var side in new[]{"left","right"}){a=new GameModel();a.rooms.ForEach(r=>{r.cleared=true;r.visited=true;});int source=side=="right"?0:1;a.EnterRoom(source);a.player.x=side=="right"?916:44;a.player.y=GameModel.DoorCenterY;Check(a.FloorAllows(a.player.x,a.player.y),"side doorway center is passable");a.Tick(.04f,side=="right"?1:-1,0);Check(a.room!=source,"aligned side doorway transitions");Check(a.player.y==GameModel.DoorCenterY,"arrival uses doorway center");}Console.WriteLine("PASS: side doorway artwork/collision/arrival alignment.");
MovementAudit.Run();
