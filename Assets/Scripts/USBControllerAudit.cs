using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
namespace AccordingToJames {
 public class USBControllerAudit:MonoBehaviour {
  [StructLayout(LayoutKind.Sequential)] struct USBState:IInputStateTypeInfo {public int buttons;public Vector2 stick;public FourCC format=>new FourCC('J','O','Y');}
  bool restore,existed;int oldBinding;
  public void Run(GameController owner){StartCoroutine(Check(owner));}
  static void Require(bool ok,string message){if(!ok)throw new Exception("USB controller check: "+message);}
  static IEnumerator State(Joystick pad,int buttons){InputSystem.QueueStateEvent(pad,new USBState{buttons=buttons});for(int i=0;i<4;i++)yield return null;}
  IEnumerator Check(GameController owner){InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;owner.ControllerBridge.IgnoreNativeForTest=true;string controls="";for(int i=1;i<8;i++){if(i>1)controls+=",";controls+="{\"name\":\"button"+(i+1)+"\",\"layout\":\"Button\",\"offset\":0,\"bit\":"+(i+4)+",\"sizeInBits\":1,\"format\":\"BIT\"}";}InputSystem.RegisterLayout("{\"name\":\"JamesTestUSB\",\"extend\":\"Joystick\",\"controls\":["+controls+"]}");var usb=(Joystick)InputSystem.AddDevice("JamesTestUSB");for(int i=0;i<3;i++)yield return null;Require(owner.ControllerBridge.Source==usb&&owner.ControllerConnected,"generic joystick detection");owner.ControllerBridge.ResetMappingForTest();owner.NewRun();owner.Resume();float start=owner.Game.player.x;InputSystem.QueueStateEvent(usb,new USBState{stick=Vector2.right});yield return new WaitForSeconds(.25f);Require(owner.Game.player.x>start+2,"USB movement");yield return State(usb,0);
   yield return State(usb,1<<5);Require(owner.Game.player.attack>0,"USB B swing");yield return State(usb,0);
   yield return State(usb,1<<4);Require(owner.Game.player.dodge>0,"USB Y dodge");yield return State(usb,0);
   owner.Game.player.dodge=0;owner.Game.player.x=owner.Game.JamesX;owner.Game.player.y=owner.Game.JamesY+65;yield return State(usb,1<<6);Require(owner.Game.status=="talking","USB A interact");yield return State(usb,0);yield return State(usb,1<<5);Require(owner.Game.status=="playing","USB B back");yield return State(usb,0);
   yield return State(usb,1<<7);Require(owner.MapOpen,"USB X map");yield return State(usb,0);yield return State(usb,1<<5);Require(!owner.MapOpen,"USB map back");yield return State(usb,0);
   yield return State(usb,1<<11);Require(owner.IsPaused,"USB Start pause");yield return State(usb,0);yield return State(usb,1<<6);Require(!owner.IsPaused,"USB A menu confirm");yield return State(usb,0);
   existed=PlayerPrefs.HasKey("JamesUSBButton0");oldBinding=PlayerPrefs.GetInt("JamesUSBButton0");restore=true;owner.ControllerBridge.Bind(0);yield return State(usb,0);yield return State(usb,1<<8);Require(owner.ControllerBridge.Waiting<0&&PlayerPrefs.GetInt("JamesUSBButton0")==4,"USB button rebinding");yield return State(usb,0);Restore();InputSystem.RemoveDevice(usb);for(int i=0;i<3;i++)yield return null;Require(!owner.ControllerConnected,"USB disconnect cleanup");Debug.Log("ACCORDING_TO_JAMES_USB_OK: generic joystick movement, SNES buttons, menu confirmation, saved rebinding and disconnect");Application.Quit(0);
  }
  void Restore(){if(!restore)return;if(existed)PlayerPrefs.SetInt("JamesUSBButton0",oldBinding);else PlayerPrefs.DeleteKey("JamesUSBButton0");PlayerPrefs.Save();restore=false;}
  void OnDestroy(){Restore();}
 }
}
