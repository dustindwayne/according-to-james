using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace AccordingToJames {
 public sealed class ControllerAudit:MonoBehaviour {
  public void Run(GameController owner){StartCoroutine(Check(owner));}
  static void Require(bool ok,string message){if(!ok)throw new Exception("SNES controller check: "+message);}
  static IEnumerator Button(Gamepad pad,GamepadButton button){InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(button));yield return null;yield return null;}
  static IEnumerator Release(Gamepad pad){InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;yield return null;}
  IEnumerator Check(GameController owner){InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;var pad=InputSystem.AddDevice<Gamepad>();pad.MakeCurrent();yield return null;owner.NewRun();yield return null;owner.Resume();float start=owner.Game.player.x;
   InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.right});yield return new WaitForSeconds(.25f);Require(owner.Game.player.x>start+2,"stick movement x="+owner.Game.player.x+" pad="+pad.leftStick.ReadValue()+" paused="+owner.IsPaused+" enabled="+pad.enabled);yield return Release(pad);
   yield return Button(pad,GamepadButton.South);Require(owner.Game.player.attack>0,"B swing");yield return Release(pad);
   yield return Button(pad,GamepadButton.West);Require(owner.Game.player.dodge>0,"Y dodge");yield return Release(pad);
   owner.Game.player.x=owner.Game.JamesX;owner.Game.player.y=owner.Game.JamesY+65;yield return Button(pad,GamepadButton.East);Require(owner.Game.status=="talking","A interact");yield return Release(pad);yield return Button(pad,GamepadButton.South);Require(owner.Game.status=="playing","B dialogue back");yield return Release(pad);
   yield return Button(pad,GamepadButton.North);Require(owner.MapOpen,"X map");yield return Release(pad);yield return Button(pad,GamepadButton.South);Require(!owner.MapOpen,"B map back");yield return Release(pad);
   yield return Button(pad,GamepadButton.Start);Require(owner.IsPaused,"Start pause");yield return Release(pad);yield return Button(pad,GamepadButton.East);Require(!owner.IsPaused,"A confirms selected resume action");yield return Release(pad);
   owner.ReturnToTitle();yield return null;var first=EventSystem.current.currentSelectedGameObject;InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadDown));yield return null;yield return null;Require(EventSystem.current.currentSelectedGameObject&&EventSystem.current.currentSelectedGameObject!=first,"D-pad menu navigation");yield return Release(pad);
   InputSystem.RemoveDevice(pad);Debug.Log("ACCORDING_TO_JAMES_CONTROLLER_OK: SNES stick/B/Y/A/X/Start/back/menu navigation and confirmation");Application.Quit(0);
  }
 }
}
