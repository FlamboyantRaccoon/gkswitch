
#if UNITY_EDITOR || UNITY_SWITCH
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: AlwaysLinkAssembly]

namespace UnityEngine.InputSystem.Switch
{
    /// <summary>
    /// Adds support for Switch NPad controllers.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif  
#if UNITY_DISABLE_DEFAULT_INPUT_PLUGIN_INITIALIZATION
    public
#else
    internal
#endif
    static class SwitchConsoleSupport
    {
        static SwitchConsoleSupport()
        {
#if UNITY_EDITOR || UNITY_SWITCH
            InputSystem.RegisterLayout<NPad>(
                matches: new InputDeviceMatcher()
                    .WithInterface("Switch")
                    .WithManufacturer("Nintendo")
                    .WithProduct("Wireless Controller"));
#endif
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeInPlayer() { }
    }
}
#endif
