using System;
using System.Collections.Generic;
using Data.Levels;
using Gameplay.Model;
using UnityEditor;
using UnityEngine;

namespace LevelEditing
{
    public class BlockShapeWindow : EditorWindow
    {
        private Func<BlockData, bool> _apply;
        private BlockData _source;
        private LevelEditorState _state;
        private LevelBoardPreview _preview;
        private int _revision;
        private string _message;

        public static void Open(BlockData source, Func<BlockData, bool> apply)
        {
            var window = CreateInstance<BlockShapeWindow>();
            window.titleContent = new GUIContent(source == null ? "Create Block" : "Edit Block");
            window.minSize = new Vector2(370f, 430f);
            window._apply = apply;
            window._source = source == null ? new BlockData() : LevelEditorRules.Copy(source);
            if (source == null)
            {
                window._source.Offsets.Clear();
            }
            window._state = CreateInstance<LevelEditorState>();
            window._state.hideFlags = HideFlags.HideAndDontSave;
            window._source.Position = Vector2Int.zero;
            window.RefreshPreview();
            var minimum = LevelEditorRules.GetMinimumSize(window._state);
            window._state.Width = Mathf.Max(8, minimum.x);
            window._state.Height = Mathf.Max(8, minimum.y);
            window.ShowUtility();
        }

        private void OnDisable()
        {
            _preview?.Dispose();
            _preview = null;
        }

        private void OnDestroy()
        {
            DestroyImmediate(_state);
        }

        private void RefreshPreview()
        {
            _state.Blocks.Clear();
            if (_source.Offsets.Count > 0)
            {
                _state.Blocks.Add(_source);
            }
            _state.InvalidateModel();
            _revision++;
        }

        private void OnGUI()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || _state == null)
            {
                _preview?.Dispose();
                _preview = null;
                EditorGUILayout.HelpBox("Reopen this window in Edit Mode to draw a block.", MessageType.Info);
                return;
            }
            EditorGUILayout.HelpBox("Click cells to add/remove them. The shape must stay connected horizontally or vertically.", MessageType.Info);
            EditorGUI.BeginChangeCheck();
            _source.Color = (BlockColor)EditorGUILayout.EnumPopup("Color", _source.Color);
            var minimum = LevelEditorRules.GetMinimumSize(_state);
            _state.Width = EditorGUILayout.IntSlider("Drawing width", _state.Width, minimum.x, Mathf.Max(32, minimum.x));
            _state.Height = EditorGUILayout.IntSlider("Drawing height", _state.Height, minimum.y, Mathf.Max(32, minimum.y));
            if (EditorGUI.EndChangeCheck())
            {
                RefreshPreview();
            }
            _preview ??= new LevelBoardPreview();
            var area = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinHeight(160f));
            _preview.Prepare(area, _state, _revision, 1f);
            if (!string.IsNullOrEmpty(_preview.Error))
            {
                EditorGUI.HelpBox(area, _preview.Error, MessageType.Error);
            }
            else
            {
                _preview.Draw(area, -1, Vector2Int.zero, true);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                    _preview.TryGetCell(area, Event.current.mousePosition, out var cell) &&
                    cell.x >= 0 && cell.y >= 0 && cell.x < _state.Width && cell.y < _state.Height)
                {
                    var candidate = new List<Vector2Int>(_source.Offsets);
                    if (!candidate.Remove(cell))
                    {
                        candidate.Add(cell);
                    }
                    if (candidate.Count == 0 || LevelEditorRules.IsConnected(candidate))
                    {
                        _source.Offsets = candidate;
                        _message = null;
                        RefreshPreview();
                    }
                    else
                    {
                        _message = "Cells must form one connected shape.";
                    }
                    Event.current.Use();
                    Repaint();
                }
            }
            if (!string.IsNullOrEmpty(_message))
            {
                EditorGUILayout.HelpBox(_message, MessageType.Warning);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }
                using (new EditorGUI.DisabledScope(_source.Offsets.Count == 0))
                {
                    if (GUILayout.Button(_source.Id == 0 ? "Create" : "Apply"))
                    {
                        var block = LevelEditorRules.Copy(_source);
                        block.Offsets = LevelEditorRules.Normalize(block.Offsets);
                        if (_apply != null && _apply(block))
                        {
                            Close();
                        }
                        else
                        {
                            _message = "Cannot apply: shape overlaps another block, exceeds the board, or the level was closed/changed.";
                        }
                    }
                }
            }
        }
    }
}
