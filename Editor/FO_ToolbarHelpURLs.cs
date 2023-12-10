
using UnityEngine;
using UnityEditor;

public static class FO_ToolbarHelpURLs
{
    [MenuItem("KitKat/JetSim/Floating Origin/📝 README", priority = -10000)]
    public static void OpenReadme()
    {
        Application.OpenURL("https://github.com/KitKat4191/JetSim-FloatingOrigin/blob/main/README.md");
    }
}
