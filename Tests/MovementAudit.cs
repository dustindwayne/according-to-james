using System;
using AccordingToJames;
public static class MovementAudit
{
 public static void Run(){
  foreach(var direction in new[]{1,-1}){var g=new GameModel();g.EnterRoom(1);g.Current.enemies.Clear();g.Current.obstacles.Clear();var e=new Enemy{x=direction>0?210:650,y=225,hp=10,maxHp=10,kind=0};g.Current.enemies.Add(e);g.player.x=direction>0?500:350;g.player.y=225;g.player.inv=999;float start=e.x;for(int i=0;i<70;i++)g.Tick(.04f);if(Math.Abs(e.x-start)<100)throw new Exception("Enemy stalled before navigation waypoint");if(e.walkTime<=1||!e.moving)throw new Exception("Walking animation did not advance during chase");}
  int scenarios=0;for(int floor=1;floor<=30;floor++)for(int room=1;room<GameModel.RoomCount(floor);room++){var g=new GameModel();g.floor=floor;g.NewFloor();g.EnterRoom(room);if(g.Current.cleared||g.Current.boss)continue;g.Current.enemies.Clear();var e=new Enemy{x=210,y=225,hp=10,maxHp=10,kind=0};g.Current.enemies.Add(e);g.player.x=480;g.player.y=430;g.player.inv=999;float stalled=0;for(int i=0;i<600;i++){g.Tick(.04f);float distance=GameModel.Distance(g.player.x-e.x,g.player.y-e.y);if(e.state=="chase"&&distance>65&&!e.moving)stalled+=.04f;else stalled=0;if(stalled>1)throw new Exception("Enemy chase stalled on floor "+floor+" room "+room);}scenarios++;}
  Console.WriteLine("PASS: waypoint-freeze regression in both directions, walking-frame progress and "+scenarios+" obstacle-room chase scenarios.");
 }
}
