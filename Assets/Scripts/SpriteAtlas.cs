using System;
using System.Collections.Generic;
using UnityEngine;

namespace AccordingToJames
{
    // Only compiled Unity sprites are used at runtime. No procedural substitute artwork.
    public sealed class SpriteAtlas
    {
        public readonly Sprite[,] JamesDanceParty,JamesDancePortraits,FloorDepth,Hero,JamesGoofy,JamesGoofyPortraits,JamesFrames,JamesPortraits,Enemies,Bosses,Props,Menus,Effects;
        public readonly Sprite[] Rooms;
        public Sprite James=>JamesFrames[0,0];
        public SpriteAtlas()
        {
            JamesDanceParty=Sheet("JamesDanceParty",3,8);JamesDancePortraits=Sheet("JamesDanceParty",3,8,"_portrait");FloorDepth=Sheet("FloorDepth",3,4);Hero=Sheet("Hero",4,8);JamesGoofy=Sheet("JamesGoofy",4,6);JamesGoofyPortraits=Sheet("JamesGoofy",4,6,"_portrait");JamesFrames=Sheet("JamesAnimated",2,4);JamesPortraits=Sheet("JamesAnimated",2,4,"_portrait");Enemies=Sheet("Enemies",3,6);
            Props=Sheet("Props",4,4);Menus=Sheet("Menus",3,2);Effects=Sheet("Effects",2,4);Bosses=new Sprite[12,4];
            string[] bosses={"BossGranny","BossLunchLady","BossChef"};for(int b=0;b<3;b++){var sheet=Sheet(bosses[b],4,4);for(int row=0;row<4;row++)for(int col=0;col<4;col++)Bosses[b*4+row,col]=sheet[row,col];}
            Rooms=new Sprite[3];string[] rooms={"RoomCellars","RoomPantry","RoomKitchen"};for(int i=0;i<3;i++){var texture=Resources.Load<Texture2D>("Art/Compiled/"+rooms[i]);if(!texture)throw new Exception("Missing dungeon art: "+rooms[i]);Rooms[i]=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);}
        }
        public static int Direction(string dir)=>dir=="left"?1:dir=="right"?2:dir=="up"?3:0;
        static Sprite[,] Sheet(string name,int rows,int columns,string suffix="")
        {
            var assets=Resources.LoadAll<Sprite>("Art/Compiled/"+name);var dictionary=new Dictionary<string,Sprite>();foreach(var sprite in assets)dictionary[sprite.name]=sprite;
            var frames=new Sprite[rows,columns];for(int row=0;row<rows;row++)for(int col=0;col<columns;col++){string key=name+"_"+row+"_"+col+suffix;if(!dictionary.TryGetValue(key,out frames[row,col]))throw new Exception("Missing imported animation frame: "+key);}return frames;
        }
    }
}
