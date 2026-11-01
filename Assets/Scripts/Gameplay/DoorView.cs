using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class DoorView : MonoBehaviour
    {
        [SerializeField] private Transform PartsRoot;
        [SerializeField] private MeshRenderer DoorMesh;
        [SerializeField] private MeshRenderer DoorArrow;
        [SerializeField] private ExitParticle Particle;
        [SerializeField] private float MeshCellSize;
        [SerializeField] private float PressDepth;

        private Tween _pressTween;
        private int _activeExits;

        public void Prepare(Door door, Material material, float cellSize)
        {
            var angle = door.Side switch
            {
                BoardSide.Top => 90f,
                BoardSide.Bottom => 270f,
                BoardSide.Right => 0f,
                BoardSide.Left => 180f,
                _ => throw new System.ArgumentOutOfRangeException()
            };
            var rotationOffset = door.Side switch
            {
                BoardSide.Top => new Vector3(90f, 0f, 0f),
                BoardSide.Bottom => new Vector3(90f, 0f, 0f),
                BoardSide.Right => new Vector3(-90f, -90f, -90f),
                BoardSide.Left => new Vector3(90f, 90f, -90f),
                _ => throw new System.ArgumentOutOfRangeException()
            };
            var horizontal = door.Side == BoardSide.Top || door.Side == BoardSide.Bottom;
            for (var index = 0; index < door.Length; index++)
            {
                var part = Instantiate(DoorMesh, PartsRoot, false);
                part.name = $"Segment_{index}";
                var offset = (index - (door.Length - 1) * 0.5f) * cellSize;
                part.transform.localPosition = horizontal
                    ? new Vector3(offset, 0f, 0f)
                    : new Vector3(0f, offset, 0f);
                part.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward)
                    * Quaternion.Euler(90f, 0f, 0f);
                part.transform.localEulerAngles += rotationOffset;
                part.transform.localScale = Vector3.one * (cellSize / MeshCellSize);
                part.sharedMaterial = material;
            }

            var arrow = Instantiate(DoorArrow, transform, false);
            arrow.transform.localPosition = new Vector3(0f, 0f, -2f);
            arrow.transform.localEulerAngles += door.Side switch
            {
                BoardSide.Top => new Vector3(-90f, 90f, 90f),
                BoardSide.Bottom => new Vector3(90f, 90f, 90f),
                BoardSide.Right => new Vector3(0f, 90f, 90f),
                BoardSide.Left => new Vector3(180f, 90f, 90f),
                _ => throw new System.ArgumentOutOfRangeException()
            };
            arrow.transform.localScale = DoorArrow.transform.localScale * (cellSize / MeshCellSize);
            Particle.transform.localRotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            Particle.Prepare(material, door.Length * cellSize / MeshCellSize);
        }

        public void BeginExit(float duration)
        {
            Particle.Play();
            _activeExits++;
            if (_activeExits > 1)
            {
                return;
            }

            _pressTween?.Kill();
            _pressTween = PartsRoot.DOLocalMoveZ(PressDepth, duration * 0.2f)
                .SetEase(Ease.OutSine);
        }

        public void EndExit(float duration)
        {
            _activeExits--;
            if (_activeExits > 0)
            {
                return;
            }

            _pressTween?.Kill();
            _pressTween = PartsRoot.DOLocalMoveZ(0f, duration * 0.2f)
                .SetEase(Ease.OutSine);
        }

        private void OnDisable()
        {
            _pressTween?.Kill();
            _pressTween = null;
            _activeExits = 0;
            Particle.Reset();
            PartsRoot.localPosition = Vector3.zero;
        }
    }
}
