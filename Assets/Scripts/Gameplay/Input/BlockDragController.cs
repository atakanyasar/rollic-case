using System.Collections.Generic;
using System.Collections;
using Gameplay.Flow;
using Gameplay.Model;
using Gameplay.View;
using UnityEngine.EventSystems;
using UnityEngine;
using Grid = Gameplay.Model.Grid;
using UnityInput = UnityEngine.Input;
namespace Gameplay.Input
{
    public class BlockDragController : MonoBehaviour
    {
        [SerializeField] private LevelController LevelController;
        [SerializeField] private Camera BoardCamera;
        [SerializeField] private BoardView BoardView;
        [SerializeField] private float SnapDuration;
        [SerializeField] private float ExitThreshold;

        private readonly List<RaycastResult> _uiHits = new();
        private BlockView _selectedBlock;
        private BlockDragMovement _movement;
        private Plane _dragPlane;
        private Vector3 _grabOffset;
        private Vector3 _boardOrigin;
        private int _pointerId;
        private BlockView _snappingBlock;
        private Vector3 _snapTarget;
        private Coroutine _snapRoutine;

        private void Update()
        {
            if (!TryReadPointer(out var pointer))
            {
                EndDrag();
                return;
            }

            if (pointer.phase == TouchPhase.Began)
            {
                BeginDrag(pointer.position, pointer.fingerId);
            }
            if (_selectedBlock == null)
            {
                return;
            }
            if (!_selectedBlock.gameObject.activeInHierarchy || pointer.phase == TouchPhase.Canceled)
            {
                EndDrag();
                return;
            }

            Drag(pointer.position);
            if (pointer.phase == TouchPhase.Ended)
            {
                EndDrag();
            }
        }

        private bool TryReadPointer(out Touch pointer)
        {
            for (var index = 0; index < UnityInput.touchCount; index++)
            {
                pointer = UnityInput.GetTouch(index);
                if (_selectedBlock == null ? pointer.phase == TouchPhase.Began : pointer.fingerId == _pointerId)
                {
                    return true;
                }
            }

            pointer = default;
            if (UnityInput.touchCount > 0 || (_selectedBlock != null && _pointerId >= 0))
            {
                return false;
            }

            pointer = new Touch
            {
                fingerId = -1,
                position = UnityInput.mousePosition,
                phase = UnityInput.GetMouseButtonDown(0) ? TouchPhase.Began
                    : UnityInput.GetMouseButtonUp(0) ? TouchPhase.Ended : TouchPhase.Moved
            };
            return UnityInput.GetMouseButton(0) || pointer.phase == TouchPhase.Began || pointer.phase == TouchPhase.Ended;
        }

        private void BeginDrag(Vector3 screenPosition, int pointerId)
        {
            if (EventSystem.current != null)
            {
                var pointer = new PointerEventData(EventSystem.current)
                {
                    position = screenPosition,
                    pointerId = pointerId
                };
                _uiHits.Clear();
                EventSystem.current.RaycastAll(pointer, _uiHits);
                if (_uiHits.Count > 0)
                {
                    return;
                }
            }

            CompleteSnap();
            Physics.SyncTransforms();
            var ray = BoardCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out var hit))
            {
                return;
            }

            if (!BoardView.TryGetBlockView(hit.collider, out var view))
            {
                return;
            }

            _dragPlane = new Plane(BoardView.transform.forward, BoardView.transform.position);
            if (!_dragPlane.Raycast(ray, out var distance))
            {
                return;
            }

            _selectedBlock = view;
            _pointerId = pointerId;
            _movement = new BlockDragMovement(LevelController.CurrentLevel.Grid, view.Block);
            _grabOffset = BoardView.transform.InverseTransformPoint(ray.GetPoint(distance)) - view.transform.localPosition;
            _boardOrigin = view.transform.localPosition
                - new Vector3(_movement.Position.x, _movement.Position.y, 0f) * view.LogicalCellSize;
        }

        private void Drag(Vector3 screenPosition)
        {
            var ray = BoardCamera.ScreenPointToRay(screenPosition);
            if (!_dragPlane.Raycast(ray, out var distance))
            {
                return;
            }

            var position = BoardView.transform.InverseTransformPoint(ray.GetPoint(distance));
            Vector2 target = (position - _grabOffset - _boardOrigin) / _selectedBlock.LogicalCellSize;
            _movement.MoveTo(target);
            _selectedBlock.transform.localPosition = _boardOrigin
                + new Vector3(_movement.Position.x, _movement.Position.y, 0f) * _selectedBlock.LogicalCellSize;

            var pivot = _selectedBlock.Block.PivotCell;
            var intent = target - new Vector2(pivot.Column, pivot.Row);
            if (Mathf.Max(Mathf.Abs(intent.x), Mathf.Abs(intent.y)) < ExitThreshold)
            {
                return;
            }

            var direction = Mathf.Abs(intent.x) >= Mathf.Abs(intent.y)
                ? new Vector2Int(System.Math.Sign(intent.x), 0)
                : new Vector2Int(0, System.Math.Sign(intent.y));
            if (LevelController.TryExit(_selectedBlock, direction))
            {
                _selectedBlock = null;
                _movement = null;
            }
        }

        private void EndDrag()
        {
            if (_selectedBlock != null && _selectedBlock.gameObject.activeInHierarchy)
            {
                _snappingBlock = _selectedBlock;
                _snapTarget = _boardOrigin + new Vector3(
                    _selectedBlock.Block.PivotCell.Column,
                    _selectedBlock.Block.PivotCell.Row,
                    0f) * _selectedBlock.LogicalCellSize;
                if (SnapDuration > 0f)
                {
                    _snapRoutine = StartCoroutine(Snap());
                }
                else
                {
                    CompleteSnap();
                }
            }

            _selectedBlock = null;
            _movement = null;
        }

        private IEnumerator Snap()
        {
            var start = _snappingBlock.transform.localPosition;
            for (var elapsed = 0f; elapsed < SnapDuration;)
            {
                if (_snappingBlock == null || !_snappingBlock.gameObject.activeInHierarchy)
                {
                    break;
                }

                elapsed += Time.unscaledDeltaTime;
                _snappingBlock.transform.localPosition = Vector3.Lerp(
                    start, _snapTarget, Mathf.SmoothStep(0f, 1f, elapsed / SnapDuration));
                yield return null;
            }

            _snapRoutine = null;
            CompleteSnap();
        }

        private void CompleteSnap()
        {
            if (_snapRoutine != null)
            {
                StopCoroutine(_snapRoutine);
                _snapRoutine = null;
            }
            if (_snappingBlock != null && _snappingBlock.gameObject.activeInHierarchy)
            {
                _snappingBlock.transform.localPosition = _snapTarget;
            }
            _snappingBlock = null;
        }

        private void OnDisable()
        {
            EndDrag();
            CompleteSnap();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                EndDrag();
            }
        }
    }
}
