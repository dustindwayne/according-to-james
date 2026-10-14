using UnityEngine;
using UnityEngine.EventSystems;
namespace AccordingToJames
{
 public sealed class TouchStick:MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
 {
  public GameController owner;public RectTransform knob;int pointer=int.MinValue;
  public void OnPointerDown(PointerEventData e){if(pointer!=int.MinValue)return;pointer=e.pointerId;OnDrag(e);}
  public void OnDrag(PointerEventData e){if(e.pointerId!=pointer)return;if(RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,e.position,e.pressEventCamera,out var p)){var rect=((RectTransform)transform).rect;var center=rect.center;var input=Vector2.ClampMagnitude((p-center)/55,1);owner.TouchMove=new Vector2(input.x,-input.y);knob.anchoredPosition=input*45;}}
  public void OnPointerUp(PointerEventData e){if(e.pointerId==pointer)ResetStick();}
  void Update(){if(owner&&!owner.CanTouchControl)ResetStick();}
  void OnDisable(){ResetStick();}
  void ResetStick(){pointer=int.MinValue;if(owner)owner.TouchMove=Vector2.zero;if(knob)knob.anchoredPosition=Vector2.zero;}
 }
}
