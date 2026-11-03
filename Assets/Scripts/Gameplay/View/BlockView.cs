using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Gameplay.Model;
using UnityEngine;

namespace Gameplay.View
{
    public class BlockView : MonoBehaviour
    {

        [SerializeField] private MeshRenderer Center;
        [SerializeField] private MeshRenderer Edge;
        [SerializeField] private MeshRenderer InnerCorner;
        [SerializeField] private MeshRenderer OuterCorner;

        [SerializeField] private MeshRenderer Arrow1;
        [SerializeField] private MeshRenderer Arrow2;
        [SerializeField] private MeshRenderer Arrow3;
        [SerializeField] private float ArrowDepth;

        [SerializeField] private Texture2D IceTexture;
        [SerializeField] private TextMeshPro IceCounter;

        [SerializeField] private float CellSize;
        [SerializeField] private float ColliderDepth;
        [SerializeField] private float ExitDuration;

        private readonly List<GameObject> _parts = new();
        private readonly List<Collider> _colliders = new();
        private GameObject _collidersRoot;
        private readonly List<MeshRenderer> _bodyRenderers = new();
        private MaterialPropertyBlock _iceProperties;
        private MeshRenderer _arrow;
        private Vector2 _center;

        public float LogicalCellSize => CellSize;
        public float ExitAnimationDuration => ExitDuration;
        public Block Block { get; private set; }
        public IReadOnlyList<Collider> Colliders => _colliders;

        public void Prepare(Block block, Material material)
        {
            _iceProperties ??= new MaterialPropertyBlock();
            if (Block != null)
            {
                Block.IceChanged -= OnIceChanged;
            }
            ClearParts();
            _arrow = null;
            _bodyRenderers.Clear();
            Block = block;
            Block.IceChanged += OnIceChanged;
            var min = block.Offsets[0];
            var max = min;
            foreach (var cell in block.Offsets)
            {
                min = Vector2Int.Min(min, cell);
                max = Vector2Int.Max(max, cell);
            }
            _center = (Vector2)(min + max) * 0.5f;
            CreateColliders();

            var layout = BlockMeshLayout.Build(block.Offsets);
            var spacing = CellSize * 0.5f;
            foreach (var entry in layout)
            {
                var source = entry.Value.Type switch
                {
                    BlockMeshLayout.PartType.Center => Center,
                    BlockMeshLayout.PartType.Edge => Edge,
                    BlockMeshLayout.PartType.InnerCorner => InnerCorner,
                    BlockMeshLayout.PartType.OuterCorner => OuterCorner,
                    _ => throw new System.ArgumentOutOfRangeException()
                };
                var part = Instantiate(source, transform);
                var rotation = Quaternion.Euler(0f, 0f, entry.Value.Angle);
                var position = new Vector3(
                    (entry.Key.x - 0.5f) * spacing,
                    (entry.Key.y - 0.5f) * spacing,
                    0f);
                var pivotOffset = Vector3.zero;
                if (source == OuterCorner || source == Edge || source == Center)
                {
                    pivotOffset = new Vector3(-spacing * 0.5f, -spacing * 0.5f, 0f);
                }
                else if (source == InnerCorner)
                {
                    pivotOffset = new Vector3(spacing * 0.5f, spacing * 0.5f, 0f);
                }

                part.transform.localPosition = position + rotation * pivotOffset;
                part.transform.localRotation = rotation * source.transform.localRotation;
                part.sharedMaterial = material;
                part.gameObject.SetActive(true);
                _parts.Add(part.gameObject);
                _bodyRenderers.Add(part);
            }
            if (block.Movement != BlockMovement.Free)
            {
                CreateArrow(material);
            }
            IceCounter.transform.localPosition = new Vector3(_center.x * CellSize, _center.y * CellSize, ArrowDepth - 0.1f);
            IceCounter.rectTransform.sizeDelta = Vector2.one * CellSize;
            IceCounter.SetText("{0}", Block.IceRemaining);
            UpdateIceAppearance();
        }

        private void OnIceChanged()
        {
            IceCounter.SetText("{0}", Block.IceRemaining);
            if (!Block.IsFrozen)
            {
                UpdateIceAppearance();
            }
        }

        private void UpdateIceAppearance()
        {
            _iceProperties.Clear();
            if (Block.IsFrozen)
            {
                _iceProperties.SetTexture("_MainTex", IceTexture);
                _iceProperties.SetColor("_Color", Color.white);
            }
            foreach (var renderer in _bodyRenderers)
            {
                renderer.SetPropertyBlock(Block.IsFrozen ? _iceProperties : null);
            }
            IceCounter.gameObject.SetActive(Block.IsFrozen);
            if (_arrow != null)
            {
                _arrow.gameObject.SetActive(!Block.IsFrozen);
            }
        }

        private void OnDestroy()
        {
            if (Block != null)
            {
                Block.IceChanged -= OnIceChanged;
            }
        }

        private void CreateArrow(Material material)
        {
            var vertical = Block.Movement == BlockMovement.UpDown;
            var axis = vertical ? Vector2Int.up : Vector2Int.right;
            var length = 0;
            foreach (var cell in Block.Offsets)
            {
                var run = 1;
                while (run < 3 && Block.Offsets.Contains(cell + axis * run))
                {
                    run++;
                }
                length = Mathf.Max(length, run);
            }
            var source = length == 3 ? Arrow3 : length == 2 ? Arrow2 : Arrow1;
            var arrow = Instantiate(source, transform, false);
            _arrow = arrow;
            arrow.transform.localPosition = new Vector3(_center.x * CellSize, _center.y * CellSize, ArrowDepth);
            arrow.transform.localRotation = Quaternion.Euler(0f, 0f, vertical ? 0f : 90f);
            arrow.transform.localScale = Vector3.one * (CellSize * 0.5f);
            arrow.sharedMaterial = material;
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_Color", Color.white);
            arrow.SetPropertyBlock(properties);
            arrow.gameObject.SetActive(true);
            _parts.Add(arrow.gameObject);
        }

        public Sequence PlayExit(Vector3 alignedPosition, Vector2Int direction)
        {
            var min = int.MaxValue;
            var max = int.MinValue;
            foreach (var offset in Block.Offsets)
            {
                var projection = offset.x * direction.x + offset.y * direction.y;
                min = Mathf.Min(min, projection);
                max = Mathf.Max(max, projection);
            }

            var target = alignedPosition
                + new Vector3(direction.x, direction.y, 0f) * ((max - min + 2) * CellSize);
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOLocalMove(alignedPosition, ExitDuration * 0.2f)
                .SetEase(Ease.InOutSine));
            sequence.Append(transform.DOLocalMove(target, ExitDuration * 0.8f)
                .SetEase(Ease.InOutSine));
            sequence.Insert(ExitDuration * 0.75f,
                transform.DOScale(Vector3.zero, ExitDuration * 0.25f).SetEase(Ease.Linear));
            sequence.SetLink(gameObject, LinkBehaviour.KillOnDisable);
            return sequence;
        }

        private void CreateColliders()
        {
            _colliders.Clear();
            if (_collidersRoot != null)
            {
                _collidersRoot.SetActive(false);
                Destroy(_collidersRoot);
            }

            _collidersRoot = new GameObject("HitArea");
            _collidersRoot.layer = gameObject.layer;
            _collidersRoot.transform.SetParent(transform, false);
            foreach (var offset in Block.Offsets)
            {
                var collider = _collidersRoot.AddComponent<BoxCollider>();
                collider.center = new Vector3(offset.x * CellSize, offset.y * CellSize, 0f);
                collider.size = new Vector3(CellSize, CellSize, ColliderDepth);
                _colliders.Add(collider);
            }
        }

        private void ClearParts()
        {
            foreach (var part in _parts)
            {
                part.SetActive(false);
                Destroy(part);
            }

            _parts.Clear();
        }
    }
}
