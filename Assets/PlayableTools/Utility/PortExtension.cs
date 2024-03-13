using System;
using System.Reflection;
using UnityEditor.Experimental.GraphView;

namespace PlayableTools.Editor.Extension
{
    public static class PortExtension
    {
        public static void AddConnectLinstener(this Port port, Action<Port> action)
        {
            Type portType = port.GetType();
           
            var field = portType.GetField("OnConnect", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(port, Action.Combine((Delegate)field.GetValue(port),action));
        }
        
        public static void RemoveConnectLinstener(this Port port, Action<Port> action)
        {
            Type portType = port.GetType();
           
            var field = portType.GetField("OnConnect", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(port, Action.Remove((Delegate)field.GetValue(port),action));
        }
        
        public static void AddDisConnectLinstener(this Port port, Action<Port> action)
        {
            Type portType = port.GetType();
           
            var field = portType.GetField("OnDisconnect", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(port, Action.Combine((Delegate)field.GetValue(port),action));
        }
        
        public static void RemoveDisConnectLinstener(this Port port, Action<Port> action)
        {
            Type portType = port.GetType();
           
            var field = portType.GetField("OnDisconnect", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(port, Action.Remove((Delegate)field.GetValue(port),action));
        }
    }
}