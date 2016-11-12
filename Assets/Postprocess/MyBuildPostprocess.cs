#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class MyBuildPostprocess
{
	[PostProcessBuild(999)]
	public static void OnPostProcessBuild( BuildTarget buildTarget, string path)
	{
		if(buildTarget == BuildTarget.iOS)
		{
			string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

			PBXProject pbxProject = new PBXProject ();
			pbxProject.ReadFromFile(projectPath);

			string target = pbxProject.TargetGuidByName("Unity-iPhone");            
			pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
			pbxProject.SetBuildProperty(target, "CLANG_ENABLE_MODULES", "YES");

			pbxProject.WriteToFile (projectPath);
		}
	}
}
#endif