using System;
using System.Collections.Generic;
using Data.Levels;
using Gameplay.Model;
using Gameplay.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace LevelEditing
{
    public class LevelBoardPreview : IDisposable
    {
        private const string GameplayPath = "Assets/Scenes/GameplayScene.unity";
        private PreviewRenderUtility _preview;
        private GameObject _root;
        private BlockView _blockPrefab;
        private readonly Dictionary<int, BlockView> _blocks = new();
        private readonly Dictionary<BlockColor, Material> _materials = new();
        private LevelEditorState _state;
        private int _revision = -1;
        private float _cellSize;
        private BlockView _ghost;
        private int _ghostId = -1;
        private MeshRenderer[] _ghostRenderers;
        private Material _invalidMaterial;
        private readonly MaterialPropertyBlock _ghostProperties = new();
        private Quaternion _rotation;
        private string _error;

        public string Error => _error;

        public void Prepare(Rect rect, LevelEditorState state, int revision, float zoom)
        {
            if (_state != state || _revision != revision || (_preview != null && _root == null))
            {
                Rebuild(state, revision);
            }
            if (_preview == null)
            {
                return;
            }
            var camera = _preview.camera;
            camera.aspect = rect.width / Mathf.Max(1f, rect.height);
            camera.orthographicSize = Mathf.Max(
                (state.Width + 2) * _cellSize / (2f * camera.aspect),
                (state.Height + 2) * _cellSize * 0.5f) / zoom;
            camera.transform.rotation = _rotation;
            camera.transform.position = -camera.transform.forward * 100f;
        }

        public void Draw(Rect rect, int dragId, Vector2 dragPosition, bool valid)
        {
            if (_preview == null)
            {
                return;
            }
            UpdateGhost(dragId, dragPosition, valid);
            if (Event.current.type == EventType.Repaint)
            {
                _preview.BeginPreview(rect, GUIStyle.none);
                _preview.Render();
                var texture = _preview.EndPreview();
                GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, false);
            }
        }

        private void Rebuild(LevelEditorState state, int revision)
        {
            Dispose();
            _state = state;
            _revision = revision;
            _error = null;
            try
            {
                _preview = new PreviewRenderUtility();
                _preview.camera.orthographic = true;
                _preview.camera.nearClipPlane = 0.1f;
                _preview.camera.farClipPlane = 500f;
                _preview.camera.clearFlags = CameraClearFlags.Color;
                _preview.camera.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
                _preview.lights[0].intensity = 1.2f;
                _preview.lights[1].intensity = 0.4f;
                _root = new GameObject("Level Editor Preview");
                _root.hideFlags = HideFlags.HideAndDontSave;
                _preview.AddSingleGO(_root);
                var board = CopyViews();
                var level = state.Model;
                board.Prepare(level);
                var views = _root.GetComponentsInChildren<BlockView>(true);
                foreach (var entry in level.Blocks)
                {
                    _blocks.Add(entry.Key, Array.Find(views, view => view.Block == entry.Value));
                }
            }
            catch (Exception exception)
            {
                Dispose();
                _error = "Cannot build gameplay preview: " + exception.Message;
            }
        }

        private BoardView CopyViews()
        {
            var active = SceneManager.GetActiveScene();
            var sourceScene = SceneManager.GetSceneByPath(GameplayPath);
            var opened = !sourceScene.isLoaded;
            if (opened)
            {
                sourceScene = EditorSceneManager.OpenScene(GameplayPath, OpenSceneMode.Additive);
            }
            try
            {
                BoardView source = null;
                foreach (var root in sourceScene.GetRootGameObjects())
                {
                    source = root.GetComponentInChildren<BoardView>(true);
                    if (source != null)
                    {
                        break;
                    }
                }
                if (source == null)
                {
                    throw new InvalidOperationException("GameplayScene has no BoardView.");
                }
                var sourceFields = new SerializedObject(source);
                _blockPrefab = (BlockView)sourceFields.FindProperty("BlockPrefab").objectReferenceValue;
                _cellSize = _blockPrefab.LogicalCellSize;
                var camera = (Camera)sourceFields.FindProperty("BoardCamera").objectReferenceValue;
                _rotation = camera.transform.rotation;
                foreach (var root in sourceScene.GetRootGameObjects())
                {
                    var light = root.GetComponentInChildren<Light>(true);
                    if (light != null && light.type == LightType.Directional)
                    {
                        _preview.lights[0].transform.rotation = light.transform.rotation;
                        _preview.lights[0].color = light.color;
                        _preview.lights[0].intensity = light.intensity;
                        break;
                    }
                }
                var grid = _root.AddComponent<GridView>();
                var walls = _root.AddComponent<WallView>();
                var board = _root.AddComponent<BoardView>();
                EditorUtility.CopySerialized(sourceFields.FindProperty("GridView").objectReferenceValue, grid);
                EditorUtility.CopySerialized(sourceFields.FindProperty("WallView").objectReferenceValue, walls);
                EditorUtility.CopySerialized(source, board);
                foreach (BlockColor color in Enum.GetValues(typeof(BlockColor)))
                {
                    _materials[color] = (Material)sourceFields.FindProperty("BlockMaterials." + color + "Material").objectReferenceValue;
                }
                var fields = new SerializedObject(board);
                fields.FindProperty("GridView").objectReferenceValue = grid;
                fields.FindProperty("WallView").objectReferenceValue = walls;
                fields.FindProperty("BoardCamera").objectReferenceValue = _preview.camera;
                fields.FindProperty("BoardScreenFraction").vector2Value = Vector2.one;
                fields.ApplyModifiedPropertiesWithoutUndo();
                return board;
            }
            finally
            {
                if (opened)
                {
                    EditorSceneManager.CloseScene(sourceScene, true);
                }
                if (active.IsValid() && active.isLoaded)
                {
                    SceneManager.SetActiveScene(active);
                }
            }
        }

        private BlockView CreateBlock(BlockData data)
        {
            var view = Object.Instantiate(_blockPrefab, _root.transform, false);
            view.Prepare(new Block(data.Color, new List<Vector2Int>(data.Offsets), data.Movement), _materials[data.Color]);
            view.transform.localPosition = Position(data.Position);
            return view;
        }

        private void UpdateGhost(int id, Vector2 position, bool valid)
        {
            foreach (var entry in _blocks)
            {
                entry.Value.gameObject.SetActive(entry.Key != id);
            }
            if (_ghostId != id)
            {
                if (_ghost != null)
                {
                    Object.DestroyImmediate(_ghost.gameObject);
                }
                Object.DestroyImmediate(_invalidMaterial);
                _invalidMaterial = null;
                _ghost = null;
                _ghostId = id;
                var data = _state.Blocks.Find(b => b.Id == id) ?? _state.Pending.Find(b => b.Id == id);
                if (data != null)
                {
                    _ghost = CreateBlock(data);
                    _ghostRenderers = _ghost.GetComponentsInChildren<MeshRenderer>();
                    _invalidMaterial = new Material(_materials[data.Color]);
                    var color = _invalidMaterial.color;
                    color.a = 0.35f;
                    _invalidMaterial.color = color;
                    _invalidMaterial.SetFloat("_Mode", 2f);
                    _invalidMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    _invalidMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    _invalidMaterial.SetInt("_ZWrite", 0);
                    _invalidMaterial.DisableKeyword("_ALPHATEST_ON");
                    _invalidMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    _invalidMaterial.EnableKeyword("_ALPHABLEND_ON");
                    _invalidMaterial.renderQueue = (int)RenderQueue.Transparent;
                }
            }
            if (_ghost != null)
            {
                _ghost.transform.localPosition = Position(position);
                foreach (var renderer in _ghostRenderers)
                {
                    renderer.sharedMaterial = valid ? _materials[_ghost.Block.Color] : _invalidMaterial;
                    if (renderer.HasPropertyBlock())
                    {
                        renderer.GetPropertyBlock(_ghostProperties);
                        var color = _ghostProperties.GetColor("_Color");
                        color.a = valid ? 1f : 0.35f;
                        _ghostProperties.SetColor("_Color", color);
                        renderer.SetPropertyBlock(_ghostProperties);
                    }
                }
            }
        }

        private Vector3 Position(Vector2 cell)
        {
            return new Vector3(cell.x - (_state.Width - 1) * 0.5f, cell.y - (_state.Height - 1) * 0.5f, 0f) * _cellSize;
        }

        public bool TryGetCell(Rect rect, Vector2 mouse, out Vector2Int cell)
        {
            var found = TryGetPosition(rect, mouse, out var position);
            cell = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
            return found;
        }

        public bool TryGetPosition(Rect rect, Vector2 mouse, out Vector2 position)
        {
            position = Vector2.zero;
            if (_preview == null || !rect.Contains(mouse))
            {
                return false;
            }
            var viewport = new Vector3((mouse.x - rect.x) / rect.width, 1f - (mouse.y - rect.y) / rect.height, 0f);
            var ray = _preview.camera.ViewportPointToRay(viewport);
            var plane = new Plane(Vector3.forward, Vector3.zero);
            if (!plane.Raycast(ray, out var distance))
            {
                return false;
            }
            var point = ray.GetPoint(distance);
            position = new Vector2(point.x / _cellSize + _state.Width * 0.5f, point.y / _cellSize + _state.Height * 0.5f);
            return true;
        }

        public Vector2 Project(Rect rect, Vector2 cell)
        {
            var point = _preview.camera.WorldToViewportPoint(Position(cell));
            return new Vector2(rect.x + point.x * rect.width, rect.y + (1f - point.y) * rect.height);
        }

        public void Dispose()
        {
            if (_preview != null)
            {
                _preview.Cleanup();
                _preview = null;
            }
            Object.DestroyImmediate(_invalidMaterial);
            _invalidMaterial = null;
            _root = null;
            _ghost = null;
            _ghostId = -1;
            _ghostRenderers = null;
            _blocks.Clear();
            _materials.Clear();
        }
    }
}
