using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace QbGameLib_UI
{
    public class DragAndDropManipulator : PointerManipulator
    {
        private EventCallback<PointerDownEvent> _downEvent;
        private EventCallback<PointerMoveEvent> _moveEvent;
        private EventCallback<PointerUpEvent> _upEvent;
        private EventCallback<PointerCaptureOutEvent> _captureOutEvent;

        public DragAndDropManipulator(VisualElement target,
            EventCallback<PointerDownEvent> downEvent = null,
            EventCallback<PointerMoveEvent> moveEvent = null,
            EventCallback<PointerUpEvent> upEvent = null,
            EventCallback<PointerCaptureOutEvent> captureOutEvent  = null)
        {
            _downEvent = downEvent;
            _moveEvent = moveEvent;
            _upEvent = upEvent;
            _captureOutEvent = captureOutEvent;
            if (_downEvent == null) _downEvent = PointerDownHandler;
            if (_moveEvent == null) _moveEvent = PointerMoveHandler;
            if (_upEvent == null) _upEvent = PointerUpHandler;
            if (_captureOutEvent == null) _captureOutEvent = PointerCaptureOutHandler;
            _active = true;
            this.target = target;
            
            root = target.parent;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(_downEvent);
            target.RegisterCallback<PointerMoveEvent>(_moveEvent);
            target.RegisterCallback<PointerUpEvent>(_upEvent);
            target.RegisterCallback<PointerCaptureOutEvent>(_captureOutEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(_downEvent);
            target.UnregisterCallback<PointerMoveEvent>(_moveEvent);
            target.UnregisterCallback<PointerUpEvent>(_upEvent);
            target.UnregisterCallback<PointerCaptureOutEvent>(_captureOutEvent);
        }

        private Vector2 targetStartPosition { get; set; }

        private Vector3 pointerStartPosition { get; set; }

        private bool enabled { get; set; }
        private bool _active;
        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                if (_active) RegisterCallbacksOnTarget();
                else UnregisterCallbacksFromTarget();
            }
        }

        private VisualElement root { get; }

        // This method stores the starting position of target and the pointer,
        // makes target capture the pointer, and denotes that a drag is now in progress.
        public void PointerDownHandler(PointerDownEvent evt)
        {
            targetStartPosition = target.resolvedStyle.translate;
            pointerStartPosition = evt.position;
            target.CapturePointer(evt.pointerId);
            enabled = true;
        }

        // This method checks whether a drag is in progress and whether target has captured the pointer.
        // If both are true, calculates a new position for target within the bounds of the window.
        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - pointerStartPosition;

                target.style.translate = new Vector2(
                    targetStartPosition.x + pointerDelta.x,
                    targetStartPosition.y + pointerDelta.y);
            }
        }

        // This method checks whether a drag is in progress and whether target has captured the pointer.
        // If both are true, makes target release the pointer.
        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
            }
        }

        // This method checks whether a drag is in progress. If true, queries the root
        // of the visual tree to find all slots, decides which slot is the closest one
        // that overlaps target, and sets the position of target so that it rests on top
        // of that slot. Sets the position of target back to its original position
        // if there is no overlapping slot.
        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
        {
            if (enabled)
            {
                VisualElement slotsContainer = root.Q<VisualElement>("slots");
                UQueryBuilder<VisualElement> allSlots =
                    slotsContainer.Query<VisualElement>(className: "slot");
                UQueryBuilder<VisualElement> overlappingSlots =
                    allSlots.Where(OverlapsTarget);
                VisualElement closestOverlappingSlot =
                    FindClosestSlot(overlappingSlots);
                Vector3 closestPos = Vector3.zero;
                if (closestOverlappingSlot != null)
                {
                    closestPos = RootSpaceOfSlot(closestOverlappingSlot);
                    closestPos = new Vector2(closestPos.x - 5, closestPos.y - 5);
                }
                target.style.translate =
                    closestOverlappingSlot != null ?
                        closestPos :
                        targetStartPosition;

                enabled = false;
            }
        }

        private bool OverlapsTarget(VisualElement slot)
        {
            return target.worldBound.Overlaps(slot.worldBound);
        }

        private VisualElement FindClosestSlot(UQueryBuilder<VisualElement> slots)
        {
            List<VisualElement> slotsList = slots.ToList();
            float bestDistanceSq = float.MaxValue;
            VisualElement closest = null;
            foreach (VisualElement slot in slotsList)
            {
                Vector3 displacement =
                    RootSpaceOfSlot(slot) - target.resolvedStyle.translate;
                float distanceSq = displacement.sqrMagnitude;
                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    closest = slot;
                }
            }
            return closest;
        }

        private Vector3 RootSpaceOfSlot(VisualElement slot)
        {
            Vector2 slotWorldSpace = slot.parent.LocalToWorld(slot.layout.position);
            return root.WorldToLocal(slotWorldSpace);
        }
    }
}