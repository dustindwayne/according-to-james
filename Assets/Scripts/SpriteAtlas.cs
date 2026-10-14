using UnityEngine;
namespace AccordingToJames
{
    public class SpriteAtlas
    {
        public Sprite[,] Bosses,Walk,Enemies; public Sprite James,Room,Slime,Brick,White,Candy,Pin,Needle,Gas,Flour,Yarn,Heart,Chest,Shrine,Gravy,Tray,Sausage,Pit;
        public SpriteAtlas()
        {
            James=Single("James");Bosses=Read("Bosses",12,4,true,true);Walk=Read("HeroWalk",4,6,true);Enemies=Read("Enemies",3,6,true);
            var room=Resources.Load<Texture2D>("Art/Room");Room=Sprite.Create(room,new Rect(0,0,room.width,room.height),new Vector2(.5f,.5f),100);
            White=Pixel("White",8,8,(x,y)=>Color.white);
            Pit=Pixel("Pit",64,32,(x,y)=>((x-32)*(x-32)/900f+(y-16)*(y-16)/180f)<1?new Color(.025f,.025f,.035f):Color.clear);
            Chest=Pixel("Chest",48,36,(x,y)=>{if(x<3||x>44||y<3||y>33)return Color.clear;return x<7||x>40||y<7||y>29||x>20&&x<27?new Color(.92f,.67f,.2f):y<19?new Color(.4f,.22f,.1f):new Color(.65f,.35f,.16f);});
            Shrine=Pixel("Shrine",48,64,(x,y)=>{if(y<10&&x>4&&x<44)return new Color(.3f,.42f,.5f);if(y>=10&&y<44&&x>12&&x<36)return new Color(.4f,.55f,.65f);if(y>=44&&Mathf.Abs(x-24)<(64-y))return new Color(.4f,1,.85f);return Color.clear;});
            Gravy=Cloud("Gravy",new Color(.5f,.3f,.1f,.7f));
            Tray=Pixel("Tray",40,24,(x,y)=>x<2||x>37||y<2||y>21?Color.clear:x<5||x>34||y<5||y>18?new Color(.8f,.85f,.9f):new Color(.4f,.5f,.6f));
            Sausage=Pixel("Sausage",34,16,(x,y)=>y<3||y>12||x<2||x>31?Color.clear:x%11<2?new Color(.45f,.15f,.1f):new Color(.8f,.35f,.18f));
            Brick=Pixel("Brick",48,36,(x,y)=>x%24<2||y%18<2?new Color(.06f,.09f,.12f):y%18<4?new Color(.3f,.32f,.35f):new Color(.17f,.2f,.24f));
            Slime=Pixel("Slime",48,40,(x,y)=>{if(y<4||y>34||x<4||x>43)return Color.clear;if(y>28&&(x<10||x>38))return Color.clear;if(y>17&&y<24&&((x>10&&x<17)||(x>29&&x<36)))return Color.black;return new Color(.35f,.6f,.35f);});
            Candy=Pixel("Candy",24,16,(x,y)=>x<4||x>19?(y>4&&y<12?new Color(.65f,.4f,.15f):Color.clear):new Color(.95f,.72f,.3f));
            Pin=Pixel("Pin",54,16,(x,y)=>x<7||x>46?(y>5&&y<10?new Color(.42f,.25f,.12f):Color.clear):new Color(.76f,.51f,.28f));
            Needle=Pixel("Needle",32,8,(x,y)=>y>2&&y<6?new Color(.8f,.84f,.9f):Color.clear);
            Gas=Cloud("Gas",new Color(.62f,.8f,.3f,.65f));Flour=Cloud("Flour",new Color(.92f,.89f,.81f,.6f));
            Yarn=Pixel("Yarn",80,80,(x,y)=>{float r=Mathf.Sqrt((x-40)*(x-40)+(y-40)*(y-40));return r<37&&r%12<4?new Color(.75f,.45f,.9f,.7f):Color.clear;});
            Heart=Pixel("Heart",40,36,(x,y)=>{float xx=(x-20)/17f,yy=(y-16)/16f;bool inside=Mathf.Pow(xx*xx+yy*yy-1,3)-xx*xx*yy*yy*yy<0;return inside?new Color(1,.76f,.32f):Color.clear;});
        }
        public static int Direction(string dir){return dir=="left"?1:dir=="right"?2:dir=="up"?3:0;}
        static Sprite Single(string name)
        {
            var t=Resources.Load<Texture2D>("Art/"+name);if(!t)throw new System.Exception("Missing Art/"+name);var pixels=t.GetPixels32();int minX=t.width,minY=t.height,maxX=0,maxY=0;
            for(int y=0;y<t.height;y++)for(int x=0;x<t.width;x++)if(pixels[y*t.width+x].a>25){minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}
            minX=Mathf.Max(0,minX-12);minY=Mathf.Max(0,minY-12);maxX=Mathf.Min(t.width-1,maxX+12);maxY=Mathf.Min(t.height-1,maxY+12);
            return Sprite.Create(t,new Rect(minX,minY,maxX-minX+1,maxY-minY+1),new Vector2(.5f,0),100,0,SpriteMeshType.FullRect);
        }
        static Sprite Cloud(string name,Color color){return Pixel(name,80,80,(x,y)=>((x-27)*(x-27)+(y-30)*(y-30)<650||(x-52)*(x-52)+(y-38)*(y-38)<570||(x-38)*(x-38)+(y-55)*(y-55)<450)?color:Color.clear);}
        static Sprite Pixel(string name,int w,int h,System.Func<int,int,Color> color)
        {
            var t=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Point};var pixels=new Color[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)pixels[y*w+x]=color(x,y);t.SetPixels(pixels);t.Apply();return Sprite.Create(t,new Rect(0,0,w,h),new Vector2(.5f,.5f),100);
        }
        static Sprite[,] Read(string name,int rows,int cols,bool uniform,bool fitted=false)
        {
            var t=Resources.Load<Texture2D>("Art/"+name);if(!t)throw new System.Exception("Missing Art/"+name);var pix=t.GetPixels32();var result=new Sprite[rows,cols];
            int gridLeft=0,gridRight=t.width;var cuts=new int[rows+1];for(int i=0;i<=rows;i++)cuts[i]=i*t.height/rows;
            if(fitted){gridLeft=t.width;gridRight=0;var density=new int[t.height];for(int y=0;y<t.height;y++)for(int x=0;x<t.width;x++)if(pix[(t.height-1-y)*t.width+x].a>128){density[y]++;gridLeft=Mathf.Min(gridLeft,x);gridRight=Mathf.Max(gridRight,x+1);}for(int i=1;i<rows;i++){int expected=i*t.height/rows,radius=t.height/rows/3,best=expected;for(int y=expected-radius;y<=expected+radius;y++)if(density[y]<density[best]||density[y]==density[best]&&Mathf.Abs(y-expected)<Mathf.Abs(best-expected))best=y;cuts[i]=best;}}
            for(int row=0;row<rows;row++){
                int top=cuts[row],bottom=cuts[row+1];int cellWidth=(gridRight-gridLeft)/cols;int unionL=cellWidth,unionR=0,unionT=t.height,unionB=0;
                var bounds=new Rect[cols];
                for(int col=0;col<cols;col++){
                    int left=gridLeft+col*(gridRight-gridLeft)/cols,right=gridLeft+(col+1)*(gridRight-gridLeft)/cols,minX=right,minY=bottom,maxX=left,maxY=top;
                    for(int y=top;y<bottom;y++)for(int x=left;x<right;x++)if(pix[(t.height-1-y)*t.width+x].a>128){minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
                    bounds[col]=new Rect(minX,t.height-1-maxY,maxX-minX+1,maxY-minY+1);unionL=Mathf.Min(unionL,minX-left);unionR=Mathf.Max(unionR,maxX-left);unionT=Mathf.Min(unionT,minY-top);unionB=Mathf.Max(unionB,maxY-top);
                }
                for(int col=0;col<cols;col++){
                    Rect b=uniform?new Rect(gridLeft+col*(gridRight-gridLeft)/cols+unionL,t.height-(top+unionB+1),unionR-unionL+1,unionB-unionT+1):bounds[col];
                    result[row,col]=Sprite.Create(t,b,new Vector2(.5f,0),100,0,SpriteMeshType.FullRect);
                }
            }
            return result;
        }
    }
}
