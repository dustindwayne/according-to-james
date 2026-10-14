using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
namespace AccordingToJames {
 [DefaultExecutionOrder(-1000)] public sealed class ControllerBridge:MonoBehaviour {
  public bool IgnoreNativeForTest;public void ResetMappingForTest(){int[] defaults={1,0,2,3,7,6};Array.Copy(defaults,mapping,6);}
  public Gamepad VirtualPad {get;private set;} public Joystick Source {get;private set;}readonly List<ButtonControl> buttons=new List<ButtonControl>();readonly int[] mapping={1,0,2,3,7,6};
  public int Waiting {get;private set;}=-1;bool armed,neutralUntilRelease;public string Status=>Source!=null?"USB / HID: "+(string.IsNullOrEmpty(Source.description.product)?Source.displayName:Source.description.product):Gamepad.current!=null?"Gamepad: "+Gamepad.current.displayName:"No controller detected · connect USB or Bluetooth";
  void Update(){Gamepad native=null;foreach(var pad in Gamepad.all)if(!IgnoreNativeForTest&&pad!=VirtualPad&&pad.enabled){native=pad;break;}if(native!=null){Clear();return;}var joystick=Joystick.current;if(joystick==null||!joystick.enabled){Clear();return;}
   if(Source!=joystick){Clear();Source=joystick;var seen=new HashSet<long>();foreach(var control in joystick.allControls)if(control is ButtonControl button&&!button.synthetic&&button.stateBlock.sizeInBits==1&&!(button.parent is DpadControl)){long bit=(long)button.stateBlock.byteOffset*8+button.stateBlock.bitOffset;if(seen.Add(bit))buttons.Add(button);}buttons.Sort((a,b)=>((long)a.stateBlock.byteOffset*8+a.stateBlock.bitOffset).CompareTo((long)b.stateBlock.byteOffset*8+b.stateBlock.bitOffset));for(int i=0;i<mapping.Length;i++)mapping[i]=PlayerPrefs.GetInt("JamesUSBButton"+i,mapping[i]);VirtualPad=InputSystem.AddDevice<Gamepad>("SNES USB Adapter");}
   var state=new GamepadState();if(Waiting>=0){bool held=false;foreach(var button in buttons)held|=button.isPressed;if(!armed){if(!held)armed=true;}else for(int i=0;i<buttons.Count;i++)if(buttons[i].wasPressedThisFrame){mapping[Waiting]=i;PlayerPrefs.SetInt("JamesUSBButton"+Waiting,i);PlayerPrefs.Save();Waiting=-1;neutralUntilRelease=true;break;}}
   else if(neutralUntilRelease){if(!buttons.Exists(b=>b.isPressed))neutralUntilRelease=false;}
   else {var move=Source.stick.ReadValue();if(PlayerPrefs.GetInt("JamesUSBInvertY",0)==1)move.y=-move.y;var hat=Source.TryGetChildControl<DpadControl>("hat")??Source.TryGetChildControl<DpadControl>("dpad");if(hat!=null&&hat.ReadValue().sqrMagnitude>.1f)move=hat.ReadValue();if(move.magnitude>.18f)state.leftStick=move;var actions=new[]{GamepadButton.South,GamepadButton.West,GamepadButton.East,GamepadButton.North,GamepadButton.Start,GamepadButton.Select};for(int i=0;i<mapping.Length;i++)if(mapping[i]>=0&&mapping[i]<buttons.Count&&buttons[mapping[i]].isPressed)state=state.WithButton(actions[i]);}
   InputSystem.QueueStateEvent(VirtualPad,state);VirtualPad.MakeCurrent();
  }
  public string Binding(int action)=>mapping[action]>=0&&mapping[action]<buttons.Count?buttons[mapping[action]].displayName:"Button "+(mapping[action]+1);
  public void Bind(int action){if(Source==null)return;Waiting=action;armed=false;}
  public void Cancel(){Waiting=-1;}
  public void FlipY(){PlayerPrefs.SetInt("JamesUSBInvertY",1-PlayerPrefs.GetInt("JamesUSBInvertY",0));PlayerPrefs.Save();}
  void Clear(){Source=null;buttons.Clear();Waiting=-1;if(VirtualPad!=null){InputSystem.RemoveDevice(VirtualPad);VirtualPad=null;}}
  void OnDestroy(){Clear();}
 }
}
