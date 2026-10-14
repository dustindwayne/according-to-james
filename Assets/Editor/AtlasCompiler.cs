using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AccordingToJames.Editor
{
    // Turns generated source art into rigid, padded production sheets and real Unity sub-sprites.
    public static class AtlasCompiler
    {
        const string Sources="Assets/ArtSources/",Output="Assets/Resources/Art/Compiled/";
        sealed class Piece {public HashSet<int> cells=new HashSet<int>();public int x0,y0,x1,y1,count;public float cx=>(x0+x1)*.5f;public float cy=>(y0+y1)*.5f;public void Include(Piece p){cells.UnionWith(p.cells);x0=Math.Min(x0,p.x0);y0=Math.Min(y0,p.y0);x1=Math.Max(x1,p.x1);y1=Math.Max(y1,p.y1);}}
        [Serializable] sealed class Entry {public string name;public int sourceX,sourceY,sourceWidth,sourceHeight;}
        [Serializable] sealed class Report {public string source;public int rows,columns,cellWidth,cellHeight;public List<Entry> frames=new List<Entry>();}
        public static void CompileAll()
        {
            Directory.CreateDirectory(Output);
            Compile("FloorDepth",3,4,320,320,true);Compile("Props",4,4,320,320,false);Compile("Menus",3,2,640,512,true);
            Compile("Hero",4,8,384,320,false);Compile("JamesGoofy",4,6,384,320,false);Compile("JamesDanceParty",3,8,384,320,false);Compile("JamesAnimated",2,4,384,320,false);
            Compile("BossGranny",4,4,384,320,false);Compile("BossLunchLady",4,4,384,320,false);Compile("BossChef",4,4,384,320,false);
            Compile("Enemies",3,6,384,320,false);Compile("Effects",2,4,320,320,false);
            foreach(var name in new[]{"RoomCellars","RoomPantry","RoomKitchen"}){File.Copy(Sources+name+".png",Output+name+".png",true);AssetDatabase.ImportAsset(Output+name+".png",ImportAssetOptions.ForceSynchronousImport);}
            AssetDatabase.SaveAssets();Validate();Debug.Log("ACCORDING_TO_JAMES_ATLASES_OK: 172 frames plus 32 safe portrait views");
        }
        static List<Piece> Components(Color32[] pixels,int w,int h)
        {
            var seen=new bool[pixels.Length];var result=new List<Piece>();var queue=new Queue<int>();
            for(int i=0;i<pixels.Length;i++){
                if(seen[i]||pixels[i].a<32)continue;var p=new Piece{x0=i%w,y0=i/w,x1=i%w,y1=i/w};seen[i]=true;queue.Enqueue(i);
                while(queue.Count>0){int cell=queue.Dequeue(),x=cell%w,y=cell/w;p.cells.Add(cell);p.count++;p.x0=Math.Min(p.x0,x);p.x1=Math.Max(p.x1,x);p.y0=Math.Min(p.y0,y);p.y1=Math.Max(p.y1,y);
                    for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int xx=x+dx,yy=y+dy;if(xx<0||xx>=w||yy<0||yy>=h)continue;int next=yy*w+xx;if(!seen[next]&&pixels[next].a>=32){seen[next]=true;queue.Enqueue(next);}}
                }
                if(p.count>=8)result.Add(p);
            }
            return result;
        }
        static void Compile(string name,int rows,int cols,int cw,int ch,bool ui)
        {
            string path=Sources+name+".png";if(!File.Exists(path))throw new Exception("Missing source atlas: "+path);
            var source=new Texture2D(2,2,TextureFormat.RGBA32,false);source.LoadImage(File.ReadAllBytes(path));var pixels=source.GetPixels32();var pieces=Components(pixels,source.width,source.height);
            if(pieces.Count<rows*cols)throw new Exception(name+": expected "+rows*cols+" isolated subjects, found "+pieces.Count);
            var anchors=pieces.OrderByDescending(p=>p.count).Take(rows*cols).ToList();var originalCenters=anchors.Select(p=>new Vector2(p.cx,p.cy)).ToArray();
            foreach(var fragment in pieces.Except(anchors).ToArray()){int nearest=0;float best=float.MaxValue;for(int i=0;i<anchors.Count;i++){float distance=Vector2.SqrMagnitude(new Vector2(fragment.cx,fragment.cy)-originalCenters[i]);if(distance<best){nearest=i;best=distance;}}if(best<Math.Pow(Math.Max(source.width/cols,source.height/rows)*.75f,2))anchors[nearest].Include(fragment);}
            anchors=anchors.OrderByDescending(p=>p.cy).ToList();var frames=new List<Piece>();for(int row=0;row<rows;row++)frames.AddRange(anchors.Skip(row*cols).Take(cols).OrderBy(p=>p.cx));
            var target=new Texture2D(cols*cw,rows*ch,TextureFormat.RGBA32,false);var output=new Color32[target.width*target.height];var metas=new List<SpriteMetaData>();var report=new Report{source=path,rows=rows,columns=cols,cellWidth=cw,cellHeight=ch};
            for(int row=0;row<rows;row++){
                float scale=Math.Min((cw-40f)/frames.Skip(row*cols).Take(cols).Max(p=>p.x1-p.x0+1),(ch-64f)/frames.Skip(row*cols).Take(cols).Max(p=>p.y1-p.y0+1));
                if(name=="JamesGoofy"||name=="JamesDanceParty")scale=Math.Min((cw-40f)/frames.Max(p=>p.x1-p.x0+1),(ch-64f)/frames.Max(p=>p.y1-p.y0+1));
                for(int col=0;col<cols;col++){
                    var p=frames[row*cols+col];int sw=p.x1-p.x0+1,sh=p.y1-p.y0+1,dw=Mathf.CeilToInt(sw*scale),dh=Mathf.CeilToInt(sh*scale),baseX=col*cw,baseY=(rows-1-row)*ch;
                    int left=baseX+(cw-dw)/2,bottom=baseY+(ui?(ch-dh)/2:16);
                    for(int y=0;y<dh;y++)for(int x=0;x<dw;x++){int sx=p.x0+Math.Min(sw-1,(int)(x/scale)),sy=p.y0+Math.Min(sh-1,(int)(y/scale));if(p.cells.Contains(sy*source.width+sx))output[(bottom+y)*target.width+left+x]=pixels[sy*source.width+sx];}
                    var rect=ui?new Rect(left-4,bottom-4,dw+8,dh+8):new Rect(baseX,baseY,cw,ch);
                    Vector2 pivot=ui||name=="Effects"||name=="Props"&&row==1?new Vector2(.5f,.5f):new Vector2(.5f,16f/ch);
                    metas.Add(new SpriteMetaData{name=name+"_"+row+"_"+col,rect=rect,alignment=(int)SpriteAlignment.Custom,pivot=pivot,border=ui?new Vector4(32,32,32,row==0&&col==0?96:32):Vector4.zero});
                    if(name.StartsWith("James"))metas.Add(new SpriteMetaData{name=name+"_"+row+"_"+col+"_portrait",rect=new Rect(left-8,bottom-8,dw+16,dh+16),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.5f,.5f)});
                    report.frames.Add(new Entry{name=name+"_"+row+"_"+col,sourceX=p.x0,sourceY=p.y0,sourceWidth=sw,sourceHeight=sh});
                }
            }
            target.SetPixels32(output);target.Apply();string destination=Output+name+".png";File.WriteAllBytes(destination,target.EncodeToPNG());File.WriteAllText(Output+name+".frames.json",JsonUtility.ToJson(report,true));AssetDatabase.ImportAsset(destination,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(destination);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.isReadable=true;importer.alphaIsTransparency=true;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;
#pragma warning disable 618
            importer.spritesheet=metas.ToArray();
#pragma warning restore 618
            importer.SaveAndReimport();UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(target);
        }
        public static void Validate()
        {
            foreach(var spec in new[]{("FloorDepth",12),("Props",16),("Menus",6),("Hero",32),("JamesAnimated",16),("JamesGoofy",48),("JamesDanceParty",48),("BossGranny",16),("BossLunchLady",16),("BossChef",16),("Enemies",18),("Effects",8)}){
                var sprites=AssetDatabase.LoadAllAssetsAtPath(Output+spec.Item1+".png").OfType<Sprite>().ToArray();if(sprites.Length!=spec.Item2)throw new Exception(spec.Item1+": imported "+sprites.Length+" sprites, expected "+spec.Item2);
                foreach(var sprite in sprites){if(sprite.rect.width<=0||sprite.rect.height<=0||sprite.rect.xMin<0||sprite.rect.yMin<0||sprite.rect.xMax>sprite.texture.width||sprite.rect.yMax>sprite.texture.height)throw new Exception("Invalid sprite rectangle: "+sprite.name);}
            }
        }
    }
}
