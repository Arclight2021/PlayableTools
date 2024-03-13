using UnityEngine;
using UnityEngine.UIElements;

namespace PlayableTools.Utility
{
    public static class VisualElementExtension
    {
        public static void SetMinWidth(this VisualElement element, float width)
        {
            element.style.minWidth = width;
        }
        
        public static void SetLabelMinWidth<T>(this BaseField<T> field, float width)
        {
            field.labelElement.style.minWidth = width;
        }
        
        public static void SetBorderWidth(this VisualElement element, float width)
        {
            element.style.borderBottomWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
        }
        
        public static void SetBorderColor(this VisualElement element, UnityEngine.Color color)
        {
            element.style.borderBottomColor = color;
            element.style.borderTopColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
        }
        
        public static void SetBorderRadius(this VisualElement element, float radius)
        {
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
        }
        
        public static void SetBorder(this VisualElement element, float width, UnityEngine.Color color, float radius)
        {
            element.SetBorderWidth(width);
            element.SetBorderColor(color);
            element.SetBorderRadius(radius);
        }
        
        public static void SetMargin(this VisualElement element, float margin)
        {
            element.style.marginBottom = margin;
            element.style.marginTop = margin;
            element.style.marginLeft = margin;
            element.style.marginRight = margin;
        }
        
        public static void SetBackgroundColor(this VisualElement element, UnityEngine.Color color)
        {
            element.style.backgroundColor = new StyleColor(color);
        }
        
        public static void SetTextAlignCenter(this VisualElement element)
        {
            element.style.unityTextAlign = TextAnchor.MiddleCenter;
        }
    }
}