using System;
using System.Text;
using PlayableTools.Nodes.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayableTools.GraphView.Editor
{
    public class PlayableNodeGraphWindow:EditorWindow
    {
        private PlayableNodeGraphView _graphView;
        [SerializeField] public StyleSheet GridStyleSheet = default;

        [MenuItem("PlayableTools/PlayableNodeGraphEditor")]
        public static void OpenPlayableNodeGraphWindow()
        {
            var window = GetWindow<PlayableNodeGraphWindow>();
            window.titleContent = new GUIContent("PlayableNodeGraph Editor");
        }

        private void OnEnable()
        {
            rootVisualElement.style.flexDirection = FlexDirection.Row;
            
            if (_graphView == null)
            {
                _graphView = new PlayableNodeGraphView();
            }
            _graphView.style.flexGrow = 1;
            
            CreateInspector();
            
            VisualElement graphViewContainer = new VisualElement();
            graphViewContainer.style.flexGrow = 1;
            graphViewContainer.Add(_graphView);
            rootVisualElement.Add(graphViewContainer);
            
            _graphView.onNeedSave += () =>
            {
                hasUnsavedChanges = true;
                saveChangesMessage = "Save Changes?";
            };
            
            _graphView.onSaveChange += () =>
            {
                hasUnsavedChanges = false;
                saveChangesMessage = "";
            };
            
            _graphView.styleSheets.Add(GridStyleSheet);
        }

        private void CreateInspector()
        {
            var inspector = new VisualElement();
            inspector.Add(new Label("Inspector"));
            
            inspector.style.flexBasis = 200;
            inspector.style.borderRightWidth = 1;
            inspector.style.borderRightColor = new StyleColor(Color.gray);
            rootVisualElement.Add(inspector);
            
            _graphView.CreateInspectorWindow(inspector);
        }

        private void OnDisable()
        {
            Debug.Log("OnDisable");
            rootVisualElement.Clear();
            // rootVisualElement.Remove(_graphView);
            // rootVisualElement.Remove(inspector);
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
            Debug.Log("Save Changes");
        }

        public override void DiscardChanges()
        {
            base.DiscardChanges();
            Debug.Log("Discard Changes");
        }

        private void Update()
        {
            // _graphView.Update();
        }

        private void OnInspectorUpdate()
        {
            _graphView.Update();


        }
    }
}