using UnityEngine;
using System.Collections.Generic;
namespace AccordingToJames
{
    public class Chiptunes : MonoBehaviour
    {
        AudioSource music,fx;readonly Dictionary<string,AudioClip> tracks=new Dictionary<string,AudioClip>();string playing;public float Volume=.35f;
        void Awake()
        {
            music=gameObject.AddComponent<AudioSource>();music.loop=true;music.playOnAwake=false;fx=gameObject.AddComponent<AudioSource>();fx.playOnAwake=false;
            Add("cellars",new[]{60,63,67,70,67,63,62,58,60,63,65,67,65,62,58,55},.24f);
            Add("pantry",new[]{62,65,69,72,71,69,65,62,60,64,67,70,69,67,64,60},.24f);
            Add("crypt",new[]{57,60,64,68,67,64,60,59,57,59,60,64,63,60,59,56},.19f);
            Add("james",new[]{67,60,64,61,69,62,66,63,72,71,67,64,60,61,59,60},.135f);
            Add("boss",new[]{48,55,60,63,62,60,55,51,48,55,60,66,65,63,62,55},.16f);
            Add("victory",new[]{60,64,67,72,76,72,67,64,65,69,72,77,76,72,67,72},.24f);
        }
        static float Frequency(float n){return 440*Mathf.Pow(2,(n-69)/12);}
        static float Triangle(float phase){return 2*Mathf.Abs(2*(phase-Mathf.Floor(phase+.5f)))-1;}
        void Add(string name,int[] notes,float beat)
        {
            const int rate=32000;int count=Mathf.CeilToInt(notes.Length*beat*rate);var samples=new float[count];
            for(int i=0;i<count;i++){
                float t=i/(float)rate;int step=Mathf.Min(notes.Length-1,Mathf.FloorToInt(t/beat));float local=t-step*beat;
                float env=Mathf.Min(1,local/.008f)*Mathf.Clamp01((beat*.8f-local)/.06f);
                float midi=notes[step];if(name=="james"&&step%4==3)midi-=3*local/beat;
                float square=Mathf.Sin(2*Mathf.PI*Frequency(midi)*local)>=0?1:-1;
                float bassTime=t-Mathf.Floor(step/2f)*beat*2;float bassEnv=Mathf.Clamp01((beat*1.8f-bassTime)/.09f);
                float bass=Triangle(Frequency(notes[(step/4)*4]-24)*bassTime);
                samples[i]=square*env*.1f+bass*bassEnv*.12f;
            }
            var clip=AudioClip.Create(name,count,1,rate,false);clip.SetData(samples,0);tracks[name]=clip;
        }
        void Update(){if(music)music.volume=Volume;if(fx)fx.volume=Volume;}
        public void Play(string name){if(playing==name&&music.isPlaying)return;if(!tracks.TryGetValue(name,out var clip))return;playing=name;music.clip=clip;music.Play();}
        public void Pause(){music.Pause();}
        public void Effect(string type)
        {
            float note=type=="fart"?25:type=="hurt"?36:type=="swing"?75:type=="heal"?79:type=="dodge"?55:67;float duration=type=="fart"?.45f:type=="hurt"?.17f:.1f;const int rate=32000;
            var data=new float[(int)(duration*rate)];for(int i=0;i<data.Length;i++){float t=i/(float)rate;float bend=type=="fart"?-12*t/duration:type=="swing"?-16*t/duration:type=="hurt"?-10*t/duration:0;data[i]=Mathf.Sin(2*Mathf.PI*Frequency(note+bend)*t)*.2f*(1-t/duration);}
            var clip=AudioClip.Create(type,data.Length,1,rate,false);clip.SetData(data,0);fx.PlayOneShot(clip);Destroy(clip,duration+1);
        }
    }
}
