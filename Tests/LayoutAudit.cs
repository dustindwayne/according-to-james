using AccordingToJames;

static class LayoutAudit
{
    static void Assert(bool condition,string message){if(!condition)throw new Exception(message);}
    static bool Reach(GameModel g,float targetX,float targetY)
    {
        const int width=78,height=36;var reached=new bool[width*height];var queue=new Queue<int>();int start=32*width+38;queue.Enqueue(start);reached[start]=true;
        while(queue.Count>0){int cell=queue.Dequeue(),x=cell%width,y=cell/width;if(GameModel.Distance(95+x*10-targetX,125+y*10-targetY)<15)return true;
            foreach(int step in new[]{-1,1,-width,width}){int next=cell+step;if(next<0||next>=reached.Length||reached[next]||Math.Abs(step)==1&&next/width!=y||g.Blocked(95+next%width*10,125+next/width*10))continue;reached[next]=true;queue.Enqueue(next);}
        }return false;
    }
    public static void Run()
    {
        int checkedRooms=0;
        foreach(float difficulty in new[]{.75f,1f,1.5f})foreach(int seed in new[]{1,13,73,71337})for(int floor=1;floor<=30;floor++){
            var g=new GameModel(difficulty,seed);g.floor=floor;g.NewFloor();Assert(g.rooms[0].role=="entry"&&g.rooms[0].cleared,"safe entry room");Assert(g.rooms[2].chest&&g.rooms[2].role=="treasure","treasure room exists");Assert(g.rooms.Count>=4&&g.rooms.Count<=6,"room count");
            Assert(g.rooms.Count(r=>r.boss)==(floor%10==0?1:0),"boss only at dungeon finale");
            for(int room=0;room<g.rooms.Count;room++){
                g.EnterRoom(room);checkedRooms++;Assert(!g.Blocked(480,450),"arrival spawn clear");foreach(var enemy in g.Current.enemies){Assert(!g.Blocked(enemy.x,enemy.y),"enemy spawn inside walkable floor");Assert(!g.Current.spikes.Any(p=>enemy.x>=p.x&&enemy.x<=p.x+p.w&&enemy.y>=p.y&&enemy.y<=p.y+p.h),"enemy not spawned on spikes");}
                foreach(var pair in g.Neighbors(room)){Assert(g.Neighbors(pair.Value).Values.Contains(room),"reciprocal room links");Assert(g.DoorState(pair.Key)==(g.Current.cleared?3:2),"gate matches combat state");}
                foreach(string side in new[]{"up","down","left","right"})if(!g.Neighbors(room).ContainsKey(side))Assert(g.DoorState(side)==0,"nonexistent exit is sealed");
                var targets=new List<(float x,float y)>{(480,450)};foreach(string side in g.Neighbors(room).Keys)targets.Add(side=="left"?(110,310):side=="right"?(850,310):side=="up"?(480,145):(480,455));
                if(g.Current.chest)targets.Add((205,285));if(g.Current.james)targets.Add((480,300));if(g.Current.shrine)targets.Add((710,465));if(room==g.rooms.Count-1)targets.Add((805,260));if(g.Current.secret)targets.Add((GameModel.SecretX,GameModel.SecretY));
                foreach(var target in targets)Assert(Reach(g,target.x,target.y),$"unreachable interaction/door on floor {floor}, room {room}, target {target}");
            }
        }
        var a=new GameModel();a.EnterRoom(1);a.player.x=95;a.player.y=310;for(int i=0;i<5;i++)a.Tick(.04f,-1,0);Assert(a.room==1&&a.player.x>=95,"barred door has real collision");foreach(var enemy in a.Current.enemies)enemy.hp=0;a.Tick(.01f);Assert(a.DoorOpen("left"),"combat clear raises gate");for(int i=0;i<20&&a.room==1;i++)a.Tick(.04f,-1,0);Assert(a.room==0,"open doorway actually transitions");
        a=new GameModel();a.player.x=480;a.player.y=145;for(int i=0;i<20;i++)a.Tick(.04f,0,-1);Assert(a.room==0&&a.player.y>=GameModel.FloorTop,"sealed wall blocks movement");
        a=new GameModel();a.EnterRoom(a.rooms.Count-1);a.player.x=805;a.player.y=260;Assert(a.Context()=="locked"&&!a.Interact()&&a.floor==1,"exit cannot skip uncleared rooms");
        Console.WriteLine($"PASS: physical layout audit — {checkedRooms} rooms across all 30 floors, 3 difficulties and 4 seeds; collisions, reciprocal doors, safe spawns and reachable interactions.");
    }
}
