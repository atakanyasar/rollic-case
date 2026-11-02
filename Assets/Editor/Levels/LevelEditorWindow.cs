using System.Collections.Generic;
using Data.Levels;
using Gameplay.Model;
using UnityEditor;
using UnityEngine;

namespace LevelEditing
{
    public class LevelEditorWindow : EditorWindow
    {
        [SerializeField] private LevelEditorState State;
        [SerializeField] private LevelData Asset;
        [SerializeField] private LevelCatalog Catalog;

        private Vector2 _sidebarScroll;
        private LevelBoardPreview _preview;
        private LevelBoardPreview _stagingPreview;
        private LevelEditorState _stagingState;
        private int _stagingRevision = -1;
        private int _revision;
        private float _zoom = 1f;
        private float _stagingScale = 0.6f;
        private Vector2 _dragPointerOffset;
        private int _selectedBlock = -1;
        private int _selectedDoor = -1;
        private int _dragId = -1;
        private int _dragControlId;
        private bool _dragFromPending;
        private Vector2Int _grab;
        private BoardSide _doorSide;
        private BlockColor _doorColor;
        private int _doorStart;
        private int _doorEnd;
        private string _message;

        [MenuItem("Tools/Color Block Jam/Level Editor")]
        public static void Open()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(800f, 600f);
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            if (Catalog == null)
            {
                Catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/Levels/LevelCatalog.asset");
            }
            if (State == null)
            {
                CreateState();
            }
            saveChangesMessage = "Save the working level before closing? Staged blocks must be placed or deleted before saving.";
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            ClearPreviews();
        }

        private void ClearPreviews()
        {
            _preview?.Dispose();
            _preview = null;
            ClearStagingPreview();
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode && state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }
            RefreshPreviews();
        }

        private void OnFocus()
        {
            if (State != null && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                RefreshPreviews();
            }
        }

        private void RefreshPreviews()
        {
            ClearPreviews();
            State.InvalidateModel();
            _revision++;
            _dragId = -1;
            if (GUIUtility.hotControl == _dragControlId)
            {
                GUIUtility.hotControl = 0;
            }
            Repaint();
        }

        private void ClearStagingPreview()
        {
            _stagingPreview?.Dispose();
            _stagingPreview = null;
            DestroyImmediate(_stagingState);
            _stagingState = null;
        }

        private void OnDestroy()
        {
            if (State != null)
            {
                Undo.ClearUndo(State);
                DestroyImmediate(State);
            }
        }

        public override void SaveChanges()
        {
            if (Save(false))
            {
                base.SaveChanges();
            }
        }

        private void OnUndoRedo()
        {
            State.InvalidateModel();
            _revision++;
            _dragId = -1;
            _selectedDoor = -1;
            if (GUIUtility.hotControl == _dragControlId)
            {
                GUIUtility.hotControl = 0;
            }
            _message = null;
            Repaint();
        }

        private void CreateState()
        {
            State = CreateInstance<LevelEditorState>();
            State.hideFlags = HideFlags.HideAndDontSave;
            State.Id = 1;
            if (Catalog != null)
            {
                foreach (var level in Catalog.Levels)
                {
                    if (level != null)
                    {
                        State.Id = Mathf.Max(State.Id, level.Id + 1);
                    }
                }
            }
            State.Width = 8;
            State.Height = 8;
            State.TimeLimitSeconds = 120;
        }

        private void Change(string label)
        {
            State.InvalidateModel();
            _revision++;
            Undo.RecordObject(State, label);
            EditorUtility.SetDirty(State);
            _message = null;
        }

        private bool ConfirmDiscard()
        {
            return !EditorUtility.IsDirty(State) || EditorUtility.DisplayDialog("Unsaved level", "Discard the current working copy, including staged blocks?", "Discard", "Cancel");
        }

        private void OnGUI()
        {
            hasUnsavedChanges = EditorUtility.IsDirty(State);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox("Exit Play Mode to continue editing the level.", MessageType.Info);
                return;
            }
            _dragControlId = GUIUtility.GetControlID(FocusType.Passive);
            DrawToolbar();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(270f)))
                {
                    _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);
                    DrawProperties();
                    DrawBlocks();
                    DrawDoors();
                    EditorGUILayout.EndScrollView();
                    
                    if (!string.IsNullOrEmpty(_message))
                    {
                        EditorGUILayout.HelpBox(_message, MessageType.Warning);
                    }
                }
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.HelpBox("Create a block, draw its shape in the popup, then drag it from the staging area onto the board. Drag placed blocks to move them. Click a border cell to configure a door. Origin: bottom left.", MessageType.Info);
                    _zoom = EditorGUILayout.Slider("Zoom", _zoom, 0.5f, 1.5f);
                    DrawBoard();
                    var valid = LevelEditorRules.Validate(State, out var validation);
                    EditorGUILayout.HelpBox(validation, valid ? MessageType.Info : MessageType.Warning);
                }
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var selected = (LevelData)EditorGUILayout.ObjectField("Open level", Asset, typeof(LevelData), false, GUILayout.Width(300f));
                if (selected != Asset && selected != null && ConfirmDiscard())
                {
                    Load(selected);
                }
                if (GUILayout.Button("New", EditorStyles.toolbarButton))
                {
                    if (ConfirmDiscard())
                    {
                        Load(null);
                    }
                }
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    Save(false);
                }
                if (GUILayout.Button("Save As", EditorStyles.toolbarButton))
                {
                    Save(true);
                }
                if (GUILayout.Button("Test Level", EditorStyles.toolbarButton) && Save(false))
                {
                    LevelEditorPlayTest.Start(Asset, Catalog);
                }
            }
            Catalog = (LevelCatalog)EditorGUILayout.ObjectField("Level catalog", Catalog, typeof(LevelCatalog), false);
            using (new EditorGUI.DisabledScope(Asset == null || Catalog == null))
            {
                if (GUILayout.Button("Add Saved Level to Catalog"))
                {
                    if (Save(false) && !Catalog.Levels.Contains(Asset))
                    {
                        Undo.RecordObject(Catalog, "Add level to catalog");
                        Catalog.Levels.Add(Asset);
                        EditorUtility.SetDirty(Catalog);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        private void DrawProperties()
        {
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var id = EditorGUILayout.IntField("ID", State.Id);
            var minimum = LevelEditorRules.GetMinimumSize(State);
            var width = EditorGUILayout.IntSlider("Columns", State.Width, Mathf.Max(4, minimum.x), Mathf.Max(12, minimum.x));
            var height = EditorGUILayout.IntSlider("Rows", State.Height, Mathf.Max(4, minimum.y), Mathf.Max(12, minimum.y));
            var time = EditorGUILayout.IntField("Time (seconds)", State.TimeLimitSeconds);
            if (EditorGUI.EndChangeCheck())
            {
                Change("Change level properties");
                State.Id = id;
                State.Width = width;
                State.Height = height;
                State.TimeLimitSeconds = time;
            }
        }

        private void DrawBlocks()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Blocks", EditorStyles.boldLabel);
            if (GUILayout.Button("Create New Block"))
            {
                CreateBlock();
            }
            var selected = FindBlock(_selectedBlock);
            EditorGUILayout.LabelField(selected == null ? "Click a block to select it." : $"Selected: #{selected.Id} ({selected.Color})");
            using (new EditorGUI.DisabledScope(selected == null))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Edit"))
                    {
                        EditBlock(selected);
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        Change("Delete block");
                        State.Blocks.RemoveAll(b => b.Id == _selectedBlock);
                        State.Pending.RemoveAll(b => b.Id == _selectedBlock);
                        _selectedBlock = -1;
                    }
                }
            }
            DrawStagingArea();
        }

        private void CreateBlock()
        {
            var state = State;
            BlockShapeWindow.Open(null, block =>
            {
                if (this == null || State != state)
                {
                    return false;
                }
                Change("Create staged block");
                block.Id = NextBlockId();
                block.Position = Vector2Int.zero;
                State.Pending.Add(block);
                _selectedBlock = block.Id;
                Repaint();
                return true;
            });
        }

        private void DrawStagingArea()
        {
            EditorGUILayout.LabelField($"Staging area ({State.Pending.Count})", EditorStyles.boldLabel);
            if (State.Pending.Count == 0)
            {
                ClearStagingPreview();
                return;
            }
            EditorGUILayout.HelpBox("Drag blocks onto the board. Use the zoom slider to adjust their size.", MessageType.Info);
            if (_stagingState == null || _stagingRevision != _revision)
            {
                ClearStagingPreview();
                _stagingState = CreateInstance<LevelEditorState>();
                _stagingState.hideFlags = HideFlags.HideAndDontSave;
                _stagingState.Width = 1;
                _stagingState.Height = 0;
                foreach (var pending in State.Pending)
                {
                    var block = LevelEditorRules.Copy(pending);
                    block.Position = new Vector2Int(0, _stagingState.Height);
                    _stagingState.Blocks.Add(block);
                    var size = LevelEditorRules.GetMinimumSize(_stagingState);
                    _stagingState.Width = size.x;
                    _stagingState.Height = size.y + 1;
                }
                _stagingRevision = _revision;
                _stagingPreview = new LevelBoardPreview();
            }
            _stagingScale = EditorGUILayout.Slider("Zoom", _stagingScale, 0.5f, 1.5f);
            var area = GUILayoutUtility.GetRect(240f, 100f);
            _stagingPreview.Prepare(area, _stagingState, _stagingRevision, _stagingScale);
            if (!string.IsNullOrEmpty(_stagingPreview.Error))
            {
                EditorGUI.HelpBox(area, _stagingPreview.Error, MessageType.Error);
                return;
            }
            var hasPosition = _stagingPreview.TryGetPosition(area, Event.current.mousePosition, out var pointer);
            var stagingDrag = _dragFromPending && hasPosition ? _dragId : -1;
            _stagingPreview.Draw(area, stagingDrag, pointer - _dragPointerOffset, true);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                _stagingPreview.TryGetCell(area, Event.current.mousePosition, out var cell))
            {
                var block = _stagingState.Blocks.Find(b => b.Offsets.Contains(cell - b.Position));
                if (block != null)
                {
                    _selectedBlock = _dragId = block.Id;
                    GUIUtility.hotControl = _dragControlId;
                    _dragFromPending = true;
                    _grab = cell - block.Position;
                    _dragPointerOffset = pointer - (Vector2)block.Position;
                    Event.current.Use();
                    Repaint();
                }
            }
        }

        private int NextBlockId()
        {
            var id = 1;
            foreach (var block in State.Blocks)
            {
                id = Mathf.Max(id, block.Id + 1);
            }
            foreach (var block in State.Pending)
            {
                id = Mathf.Max(id, block.Id + 1);
            }
            return id;
        }

        private BlockData FindBlock(int id)
        {
            return State.Blocks.Find(b => b.Id == id) ?? State.Pending.Find(b => b.Id == id);
        }

        private void EditBlock(BlockData source)
        {
            var state = State;
            var id = source.Id;
            BlockShapeWindow.Open(source, block =>
            {
                if (this == null || State != state)
                {
                    return false;
                }
                var current = FindBlock(id);
                if (current == null)
                {
                    return false;
                }
                block.Position = current.Position;
                var index = State.Blocks.FindIndex(b => b.Id == id);
                if (index >= 0 && !LevelEditorRules.CanPlace(State, block, block.Position, id))
                {
                    return false;
                }
                Change("Edit block");
                if (index >= 0)
                {
                    State.Blocks[index] = block;
                }
                else
                {
                    State.Pending[State.Pending.FindIndex(b => b.Id == id)] = block;
                }
                Repaint();
                return true;
            });
        }

        private void DrawDoors()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Doors", EditorStyles.boldLabel);
            _doorSide = (BoardSide)EditorGUILayout.EnumPopup("Side", _doorSide);
            _doorColor = (BlockColor)EditorGUILayout.EnumPopup("Color", _doorColor);
            _doorStart = EditorGUILayout.IntField("Start (inclusive)", _doorStart);
            _doorEnd = EditorGUILayout.IntField("End (inclusive)", _doorEnd);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(_selectedDoor >= 0 ? "Apply Door" : "Add Door"))
                {
                    var door = new DoorData { Side = _doorSide, Color = _doorColor, Start = _doorStart, End = _doorEnd };
                    if (LevelEditorRules.CanPlaceDoor(State, door, _selectedDoor))
                    {
                        Change("Edit door");
                        if (_selectedDoor >= 0 && _selectedDoor < State.Doors.Count)
                        {
                            State.Doors[_selectedDoor] = door;
                        }
                        else
                        {
                            State.Doors.Add(door);
                        }
                    }
                    else
                    {
                        _message = "Door must fit the selected edge and must not overlap another door.";
                    }
                }
                if (GUILayout.Button("New Door"))
                {
                    _selectedDoor = -1;
                }
            }
            for (var i = 0; i < State.Doors.Count; i++)
            {
                var door = State.Doors[i];
                if (GUILayout.Button($"{door.Side} {door.Start}–{door.End} · {door.Color}"))
                {
                    SelectDoor(i);
                }
            }
            using (new EditorGUI.DisabledScope(_selectedDoor < 0 || _selectedDoor >= State.Doors.Count))
            {
                if (GUILayout.Button("Delete Selected Door"))
                {
                    Change("Delete door");
                    State.Doors.RemoveAt(_selectedDoor);
                    _selectedDoor = -1;
                }
            }
        }

        private void SelectDoor(int index)
        {
            _selectedDoor = index;
            var door = State.Doors[index];
            _doorSide = door.Side;
            _doorColor = door.Color;
            _doorStart = door.Start;
            _doorEnd = door.End;
        }

        private void DrawBoard()
        {
            if (_preview == null)
            {
                _preview = new LevelBoardPreview();
            }
            var viewport = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinHeight(240f));
            _preview.Prepare(viewport, State, _revision, _zoom);
            if (!string.IsNullOrEmpty(_preview.Error))
            {
                EditorGUI.HelpBox(viewport, _preview.Error, MessageType.Error);
                return;
            }
            var hasCell = _preview.TryGetPosition(viewport, Event.current.mousePosition, out var pointer);
            var cell = new Vector2Int(Mathf.FloorToInt(pointer.x), Mathf.FloorToInt(pointer.y));
            var block = FindBlock(_dragId);
            var position = cell - _grab;
            var valid = block != null && hasCell && LevelEditorRules.CanPlace(State, block, position, _dragFromPending ? -1 : block.Id);
            _preview.Draw(viewport, hasCell ? _dragId : -1, pointer - _dragPointerOffset, valid);
            foreach (var placed in State.Blocks)
            {
                if (placed.Offsets.Count == 0)
                {
                    continue;
                }
                var visualPosition = placed.Id == _dragId && hasCell
                    ? pointer - _dragPointerOffset
                    : (Vector2)placed.Position;
                var point = _preview.Project(viewport, visualPosition + placed.Offsets[0]);
                if (viewport.Contains(point))
                {
                    GUI.Label(new Rect(point.x - 15f, point.y - 10f, 70f, 22f),
                        placed.Id == _selectedBlock ? $"▶ #{placed.Id}" : $"#{placed.Id}", EditorStyles.whiteLabel);
                }
            }
            HandlePreviewMouse(hasCell, cell, block, position, valid, pointer);
        }

        private void HandlePreviewMouse(bool hasCell, Vector2Int cell, BlockData draggedBlock, Vector2Int position, bool valid, Vector2 pointer)
        {
            var evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0 && hasCell)
            {
                if (cell.x >= 0 && cell.x < State.Width && cell.y >= 0 && cell.y < State.Height)
                {
                    var block = State.Blocks.Find(b => b.Offsets.Contains(cell - b.Position));
                    _selectedBlock = block == null ? -1 : block.Id;
                    if (block != null)
                    {
                        _dragId = block.Id;
                        GUIUtility.hotControl = _dragControlId;
                        _dragFromPending = false;
                        _grab = cell - block.Position;
                        _dragPointerOffset = pointer - (Vector2)block.Position;
                    }
                }
                else
                {
                    SelectEdge(cell);
                }
                evt.Use();
                Repaint();
            }
            if (evt.type == EventType.MouseDrag && _dragId >= 0)
            {
                evt.Use();
                Repaint();
            }
            if (evt.type == EventType.MouseUp && _dragId >= 0)
            {
                var block = draggedBlock;
                if (valid)
                {
                    if (_dragFromPending || block.Position != position)
                    {
                        Change("Place or move block");
                        block.Position = position;
                        if (_dragFromPending)
                        {
                            State.Pending.Remove(block);
                            State.Blocks.Add(block);
                        }
                    }
                }
                else
                {
                    _message = "Invalid drop. Block remains in its previous position or staging area.";
                }
                _dragId = -1;
                GUIUtility.hotControl = 0;
                evt.Use();
                Repaint();
            }
        }

        private void SelectEdge(Vector2Int cell)
        {
            BoardSide side;
            int index;
            if ((cell.y == -1 || cell.y == State.Height) && cell.x >= 0 && cell.x < State.Width)
            {
                side = cell.y < 0 ? BoardSide.Bottom : BoardSide.Top;
                index = cell.x;
            }
            else if ((cell.x == -1 || cell.x == State.Width) && cell.y >= 0 && cell.y < State.Height)
            {
                side = cell.x < 0 ? BoardSide.Left : BoardSide.Right;
                index = cell.y;
            }
            else
            {
                return;
            }
            var door = State.Doors.FindIndex(d => d.Side == side && d.Start <= index && d.End >= index);
            if (door >= 0)
            {
                SelectDoor(door);
            }
            else
            {
                _selectedDoor = -1;
                _doorSide = side;
                _doorStart = _doorEnd = index;
            }
        }

        private void Load(LevelData asset)
        {
            _revision++;
            if (State != null)
            {
                Undo.ClearUndo(State);
                DestroyImmediate(State);
            }
            CreateState();
            Asset = asset;
            if (asset != null)
            {
                State.Id = asset.Id;
                State.Width = asset.Width;
                State.Height = asset.Height;
                State.TimeLimitSeconds = asset.TimeLimitSeconds;
                foreach (var block in asset.Blocks)
                {
                    State.Blocks.Add(LevelEditorRules.Copy(block));
                }
                foreach (var door in asset.Doors)
                {
                    State.Doors.Add(LevelEditorRules.Copy(door));
                }
            }
            _selectedBlock = _selectedDoor = _dragId = -1;
            _message = null;
            EditorUtility.ClearDirty(State);
        }

        private bool Save(bool saveAs)
        {
            if (!LevelEditorRules.Validate(State, out _message))
            {
                return false;
            }
            if (Catalog != null && Catalog.Levels.Exists(level => level != Asset && level != null && level.Id == State.Id))
            {
                _message = "Another level in the catalog already uses this ID.";
                return false;
            }
            var target = Asset;
            if (target == null || saveAs)
            {
                var path = EditorUtility.SaveFilePanelInProject("Save Level", $"Level_{State.Id:000}", "asset", "Choose where to save this level.", "Assets/Levels");
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }
                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                {
                    _message = "Choose a new filename; use Save to update the currently open asset.";
                    return false;
                }
                target = CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(target, path);
            }
            State.WriteTo(target);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            Asset = target;
            State.InvalidateModel();
            _revision++;
            EditorUtility.ClearDirty(State);
            _message = "Saved.";
            return true;
        }

    }
}
