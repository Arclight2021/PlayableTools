using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PlayableTools.Components;
using PlayableTools.Editor.Extension;
using PlayableTools.Nodes;
using PlayableTools.Nodes.Editor;
using PlayableTools.Utility;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
//todo:场景内调试功能
namespace PlayableTools.GraphView.Editor
{
    public class PlayableNodeGraphView :UnityEditor.Experimental.GraphView.GraphView
    {
        private GenericMenu _menu;
        private PlayableNodeGraphManager _manager;
        private Label inspectorLabel;
        private VisualElement inspectorNodeEditorContainer;
        private VisualElement inspectorruntimeNodeEditorContainer;
        private VisualElement managerInspectorContainer;
        private bool isFocusing = false;
        private Vector2 mousePosition;

        private Button playButton;
        private bool isPlaying;
        private Label playTargetLabel;
        private GameObject playTarget;

        private PlayableGraphNodeBase _currentSelectionGraphNode;

        public Action onNeedSave;
        public Action onSaveChange;
        
        private Dictionary<PlayableNodeBase, PlayableGraphNodeBase> nodelookup
        {
            get
            {
                    Dictionary<PlayableNodeBase, PlayableGraphNodeBase> _nodelookup = new ();
                    foreach (var node in this.nodes)
                    {
                        var graphNode = (PlayableGraphNodeBase)node;
                        _nodelookup.Add(graphNode.tar,graphNode);
                    }
                return _nodelookup;
            }
        }

        public PlayableNodeGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale,ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            var grid = new GridBackground();
            Insert(0,grid);
            grid.StretchToParentSize();

            this.graphViewChanged += (change) =>
            {
                onNeedSave?.Invoke();
                return change;
            };

            // this.RegisterCallback<MouseDownEvent>(evt =>
            // {
            //     CreateRightClickMenu(evt);
            // });
            RegisterCallback<MouseMoveEvent>(evt =>
            {
                //check mouse position in graphView
                var pos = evt.localMousePosition;
                mousePosition = viewTransform.matrix.inverse.MultiplyPoint(pos);
            });

            _manager = new("Name");
            
            //toolbar
            CreateToolbar();
            
            RegisterCallback<PointerEnterEvent>((evt)=>isFocusing = true);
            RegisterCallback<PointerLeaveEvent>((evt)=>isFocusing = false);
            
            //inspector 
            // CreateInspectorWindow();

            //search window
            PlayableNodeGraphSearchWindowProvider searchWindowProvider = ScriptableObject.CreateInstance<PlayableNodeGraphSearchWindowProvider>();
            searchWindowProvider.onCreateNode =(type,pos)=> CreateGraphNode(type,null);
            nodeCreationRequest = (context) =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindowProvider);
            };
        }

        #region Inspector
        private Action updateInspector;
        public void CreateInspectorWindow(VisualElement container)
        {
            VisualTreeAsset inspectorAsset =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/PlayableTools/UIToolkit/inspector.uxml");
            var inspectorContainer = inspectorAsset.CloneTree();
            inspectorLabel = inspectorContainer.Q<Label>("playablenodeeditor-inspector-label");
            inspectorNodeEditorContainer = inspectorContainer.Q<VisualElement>("playablenodeeditor-inspector-container-node-editor");
            inspectorruntimeNodeEditorContainer = inspectorContainer.Q<VisualElement>("playablenodeeditor-inspector-container-node-runtime-editor");
            container.Add(inspectorContainer);

            managerInspectorContainer = inspectorContainer.Q<VisualElement>("playablenodeeditor-managerInspector-container");

            container = inspectorContainer.Q<VisualElement>("playablenodeeditor-inspector-common-container");
            
            var nodeNameField = new TextField("Name");
            nodeNameField.SetLabelMinWidth(30);
            nodeNameField.RegisterValueChangedCallback(evt =>
            {
                if (_currentSelectionGraphNode is null)
                {
                    return;
                }
                _currentSelectionGraphNode.tar.name = evt.newValue;
            });
            container.Add(nodeNameField);
            
            var nodeTypeField = new TextField("Type");
            nodeTypeField.SetEnabled(false);
            nodeTypeField.SetLabelMinWidth(30);
            container.Add(nodeTypeField);
            
            var nodeGuidField = new TextField("Guid");
            nodeGuidField.SetEnabled(false);
            nodeGuidField.SetLabelMinWidth(30);
            container.Add(nodeGuidField);

            Action<string> MakePortTitle = (string title) =>
            {
                var inputPortsTitle = new Label();
                inputPortsTitle.text = title;
                inputPortsTitle.SetBorderWidth(1);
                inputPortsTitle.SetBorderRadius(5);
                inputPortsTitle.SetBorderColor(Color.grey);
                container.Add(inputPortsTitle);
            };

            MakePortTitle("Input Ports");
            var inputPortsContainer = new VisualElement();
            container.Add(inputPortsContainer);
            MakePortTitle("Output Ports");
            var outputPortsContainer = new VisualElement();
            container.Add(outputPortsContainer);
            
            updateInspector = () =>
            {
                if (_currentSelectionGraphNode is null)
                {
                    return;
                }
                nodeNameField.value = _currentSelectionGraphNode.tar.name;
                nodeTypeField.value = _currentSelectionGraphNode.tar.GetType().Name;
                nodeGuidField.value = _currentSelectionGraphNode.tar.name;
                
                inputPortsContainer.Clear();
                outputPortsContainer.Clear();
                
                foreach (var port in _currentSelectionGraphNode.tar.inputPorts)
                {
                    var portLabel = new TextField($"#{port.index}");
                    portLabel.SetLabelMinWidth(30);
                    var targetNodeName = port.targetNodePort?.parentNode.name;
                    portLabel.value = targetNodeName;
                    portLabel.SetEnabled(false);
                    inputPortsContainer.Add(portLabel);
                }
                
                foreach (var port in _currentSelectionGraphNode.tar.outputPorts)
                {
                    var portLabel = new TextField($"Output#{port.index}");
                    portLabel.SetLabelMinWidth(30);
                    var targetNodeName = port.targetNodePort?.parentNode.name;
                    portLabel.value = targetNodeName;
                    portLabel.SetEnabled(false);
                    outputPortsContainer.Add(portLabel);
                }
                
            };
        }
        private void UpdateInspectorDetailInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var selectable in this.selection)
            {
                stringBuilder.Append(selectable.ToString());
                stringBuilder.AppendLine();
                stringBuilder.Append("----------");
                stringBuilder.AppendLine();

                var nodegraph = selectable as PlayableGraphNodeBase;
                if (nodegraph is null)
                {
                    continue;
                }

                stringBuilder.Append(nodegraph.tar.ToString());

                stringBuilder.AppendLine();
                stringBuilder.Append("----------");
                stringBuilder.AppendLine();
            }

            stringBuilder.AppendLine("------------------------");
            inspectorLabel.text = stringBuilder.ToString();
            updateInspector?.Invoke();
            UpdateManagerInspectorInfo();
        }

        private void UpdateManagerInspectorInfo()
        {
            var container = managerInspectorContainer;
            container.Clear();
            
            _manager.CollectParams();
            var paramsDic = _manager.GetParams();
            var label = new Label("All Parameters");
            label.SetBorder(1,Color.grey,5);
            container.Add(label);
            foreach (var kp in paramsDic)
            {
                // var lab = new Label($"{kp.Key} {kp.Value.GetParamType()} {kp.Value.GetNodeCount()}");
                var amiField = new AnimationParameterField(kp.Value.Parameter);
                container.Add(amiField);
            }
        }
        #endregion


        public void Update()
        {
            var selection = this.selection.FirstOrDefault();
            if (selection is PlayableGraphNodeBase && selection != _currentSelectionGraphNode)
            {
                _currentSelectionGraphNode = selection as PlayableGraphNodeBase;
                // Debug.Log("Select Node Change");
                inspectorNodeEditorContainer.Clear();
                _currentSelectionGraphNode.OnCreateExtensionContainer(inspectorNodeEditorContainer);
                inspectorruntimeNodeEditorContainer.Clear();
                _currentSelectionGraphNode.OnRuntimeCreateContainer(inspectorruntimeNodeEditorContainer);
                
                UpdateInspectorDetailInfo();
            }else if (selection is null)
            {
                _currentSelectionGraphNode = null;
                inspectorNodeEditorContainer.Clear();
                inspectorruntimeNodeEditorContainer.Clear();
            }
            
           inspectorruntimeNodeEditorContainer.SetEnabled(isPlaying);
            
            UpdatePlayTarget();
            if (!isFocusing && !isPlaying)return;

            // UpdateInspectorInfo();

            if (isPlaying)
            {
                _manager.RuntimeUpdate();
                
                _currentSelectionGraphNode?.OnRuntimeUpdate();
            }
        }

        public void CreateToolbar()
        {
            Toolbar toolbar = new();
            this.Add(toolbar);
            
            toolbar.Add(new Button(() =>
            {
                string path =null;
                if (this.userData is not null)
                {
                    path = AssetDatabase.GetAssetPath((Object)this.userData);
                }
                if (string.IsNullOrEmpty(path))
                {
                    path = EditorUtility.SaveFilePanel(
                        "Save File",
                        EditorApplication.applicationPath,
                        "New PlayableNodeGraph",
                        "asset"
                    );
                }
                if (string.IsNullOrEmpty(path))
                    return;
                path =  path.Replace(Application.dataPath, "Assets");
                PlayableNodeGraphEditorUtility.Export(path,_manager,this);
                onSaveChange?.Invoke();
            })
            {
                text = "Save"
            });

            var soSelectObjField = new ObjectField("选择SO");
            soSelectObjField.objectType = typeof(NodeEditorDataSO);
            soSelectObjField.RegisterValueChangedCallback((evt) =>
            {
                this.ClearNodesAndEdges();
                var so = evt.newValue as NodeEditorDataSO;
                PlayableNodeGraphEditorUtility.Import(so,_manager,this);
            });
            toolbar.Add(soSelectObjField);

            playButton = new Button(() =>
            {
                if (isPlaying)
                {
                    Stop();
                    playButton.text = "Play";
                    isPlaying = false;
                }
                else
                {
                    Play();
                    playButton.text = "Stop";
                    isPlaying = true;
                }
            });
            playButton.text = "Play";
            toolbar.Add(playButton);

            playTargetLabel = new Label();
            toolbar.Add(playTargetLabel);

            var clearUnusedNodeBtn = new Button();
            clearUnusedNodeBtn.text = "Clear Unused Node";
            clearUnusedNodeBtn.clicked += () =>
            {
                List<PlayableNodeBase> nodesToRemove = new();
                foreach (var nodeBase in _manager.nodeDic)
                {
                    if (!nodelookup.ContainsKey(nodeBase.Value))
                    {
                        nodesToRemove.Add(nodeBase.Value);
                    }
                }

                foreach (var nodeBase in nodesToRemove)
                {
                    _manager.RemoveNode(nodeBase);
                }
            };
            toolbar.Add(clearUnusedNodeBtn);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var graphNodeBase = evt.target as PlayableGraphNodeBase;
            if (graphNodeBase is not null)
            {
                evt.menu.AppendAction("Set as Output Node", action =>
                {
                    SetAsOutputNode(graphNodeBase);
                });
            }
            base.BuildContextualMenu(evt);
        }

        public void SetAsOutputNode(PlayableGraphNodeBase graphNodeBase)
        {
            var prevOutputNode =
                nodes.FirstOrDefault(x => ((PlayableGraphNodeBase)x).tar.name == _manager.outputNodeName);
            if (prevOutputNode is not null)
            {
                ((PlayableGraphNodeBase)prevOutputNode).SetDefaultColor();
                graphNodeBase.SetBorderWidth(0);
                graphNodeBase.SetBorderColor(Color.black);
            }
            _manager.SetOutputNode(graphNodeBase.tar);
            graphNodeBase.SetBorderColor(Color.yellow);
            graphNodeBase.SetBorderWidth(10);
            graphNodeBase.SetBorderRadius(10);
        }

        public void CreateRightClickMenu(MouseDownEvent evt)
        {
            var mousePos = evt.localMousePosition;
            _menu = new GenericMenu();
            _menu.AddItem(new GUIContent("Create Node"),false, () =>
            {
                
                this.nodeCreationRequest?.Invoke(new NodeCreationContext()
                {
                    screenMousePosition = evt.mousePosition
                });
            });
            _menu.AddItem(new GUIContent("Create/AnimationClip"),true,
                () =>
                {
                    var pos = viewTransform.matrix.inverse.MultiplyPoint(mousePos);
                    AnimationClipGraphNode node = new AnimationClipGraphNode(_manager);
                    node.SetPosition(new Rect(pos,new Vector2(200,150)));
                    node.Refresh();
                    AddElement(node);
                });
            _menu.ShowAsContext();
        }

        public PlayableGraphNodeBase CreateGraphNode(Type type,object pos,PlayableNodeBase nodeBase = null)
        {
            var node = Activator.CreateInstance(type,new object[]{
                _manager,
                nodeBase
            }) as PlayableGraphNodeBase;
            if (pos is Rect)
            {
                node.SetPosition((Rect)pos);
            }else if (pos is Vector2)
            {
                node.SetPosition((Vector2)pos);
            }
            else
            {
                node.SetPosition(mousePosition);
            }
            node.Refresh();
            AddElement(node);
            return node;
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach((port) =>
            {
                if (port != startPort && port.node != startPort.node)
                {
                    compatiblePorts.Add(port);
                }
            });
            
            return compatiblePorts;
        }

        public void Play()
        {
            var go = playTarget;
            if (go.TryGetComponent<Animator>(out var animator))
            {
                _manager.GeneratePlayableGraph(animator);
            }
            
            var comp = go.AddComponent<PlayableNodeEditorTemporaryRecordTransformComponent>();
            comp.Record();
        }

        public void Stop()
        {
            var go = playTarget;
            go.GetComponent<PlayableNodeEditorTemporaryRecordTransformComponent>().Revert();
            _manager.StopPlay();
            
        }

        private void UpdatePlayTarget()
        {
            if (isPlaying)
            {
                return;
            }

            if (playTarget is not null)
            {
                playTargetLabel.text = playTarget.name;
            }
            else
            {
                playTargetLabel.text = "not have play target";
            }

            var go = Selection.activeGameObject;
            {
                if (go is null)
                {
                    return;
                }
                if (go.TryGetComponent<Animator>(out var animator))
                {
                    playTarget = go;
                }
            }
        }
    }
}