using UnityEngine;
using UnityEngine.UIElements;

namespace PlayableTools.Utility
{
    public class AnimationParameterField:VisualElement
    {
        public Color floatColor = new Color(0.5f,0.9f,0.5f,1);
        public Color intColor = new Color(0.3f,0.6f,0.8f,1);
        public Color boolColor = new Color(1f,0.2f,0.4f,1);
        public Color triggerColor = new Color(0.5f,0.3f,0.5f,1);
        
        private Label _label;
        private Label _typeLabel;
        private VisualElement _field;
        public AnimationParameterField(AnimatorControllerParameter parameter)
        {
            _label = new Label(parameter.name);
            _typeLabel = new Label(parameter.type.ToString());
            _field = new VisualElement();

            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    var floatField = new FloatField();
                    _field.Add(floatField);
                    break;
                case AnimatorControllerParameterType.Int:
                    var intField = new IntegerField();
                    _field.Add(intField);
                    break;
                case AnimatorControllerParameterType.Bool:
                    var boolField = new Toggle();
                    _field.Add(boolField);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    var triggerField = new Button();
                    triggerField.text = "Trigger";
                    _field.Add(triggerField);
                    break;
            }
            this.Add(_label);
            this.Add(_typeLabel);
            // this.Add(_field);
            
            this.style.flexDirection = FlexDirection.Row;
            this.style.flexGrow = 1;
            
            _label.style.minWidth = 60;
            _label.style.width = 60;
            _label.SetMargin(5);
            _typeLabel.style.minWidth = 50;
            _typeLabel.style.width = 50;
            _typeLabel.SetBorderRadius(2);
            _typeLabel.SetTextAlignCenter();
            SetBackGroundColorByType(_typeLabel,parameter.type);
            _typeLabel.SetBorderColor(Color.cyan);
            _typeLabel.SetMargin(5);
            _field.style.flexGrow = 1f;
            
            this.SetMargin(5);
            this.SetBorderColor(Color.white);
            this.SetBorderWidth(1);
            this.SetBorderRadius(2);
        }
        
        private void SetBackGroundColorByType(VisualElement element,AnimatorControllerParameterType type)
        {
            switch (type)
            {
                case AnimatorControllerParameterType.Float:
                    element.SetBackgroundColor(floatColor);
                    break;
                case AnimatorControllerParameterType.Int:
                    element.SetBackgroundColor(intColor);
                    break;
                case AnimatorControllerParameterType.Bool:
                    element.SetBackgroundColor(boolColor);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    element.SetBackgroundColor(triggerColor);
                    break;
            }
        }
        
    }
}