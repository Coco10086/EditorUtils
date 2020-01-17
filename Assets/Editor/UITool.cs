using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEditor.Animations;
using UnityEngine.UI;
public class UITool : MonoBehaviour {
	// Rename Child Name Sequential name (item1,item2,item3...)
	[MenuItem("Custom/RenameChildName")]
	public static void RenameChildNameAsArray()
	{
		if (Selection.objects.Length != 1) {
			Debug.LogError("No Selection GameObject");
			return;
		}
		UnityEngine.Object obj = Selection.activeObject;
		GameObject parent = obj as GameObject;
		if (parent == null )
		{
			Debug.LogError("Selection No GameObject");
			return;
		}
		if (parent.transform.childCount == 0) {
			Debug.LogError("Selection GameObject No childs ");
			return;
		}

		string firstChildName = parent.transform.GetChild(0).name;
		string namePattern = firstChildName;
		if (firstChildName.EndsWith("1")) {
			namePattern = firstChildName.Substring(0, firstChildName.Length - 1);
		}

		for (int i = 0; i < parent.transform.childCount; ++i)
		{
			parent.transform.GetChild(i).name = namePattern + (i + 1).ToString();
		}
	}

	[MenuItem("Custom/CheckImageSprite")]
	public static void SpriteCheck()
	{
		UnityEngine.Object obj = Selection.activeObject;
		GameObject parent = obj as GameObject;
		List<string> list = new List<string> ();
		if (parent == null) {
			string selectPath=	AssetDatabase.GetAssetPath (obj);
			list = CheckFolderSprites (selectPath);
		} else {
			list.Add (AssetDatabase.GetAssetPath (parent));
		}
		OutPutImagesPath(list);
	}

	[MenuItem("Custom/Find Missing Image")]
	public static void LoseSourceImage()
	{
		StringBuilder sbuild = new StringBuilder ();
		UnityEngine.Object selectobj = Selection.activeObject;
		GameObject parent = selectobj as GameObject;
		List<string> list;
		if (parent == null) {
			string selectPath=	AssetDatabase.GetAssetPath (selectobj);
			list = CheckFolderSprites (selectPath);
		} else {
			list = new List<string> ();
			list.Add (AssetDatabase.GetAssetPath (parent));
		}

		for (int i = 0; i < list.Count; ++i) {
			GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject> (list[i]);
			if(obj!= null)
			{
				sbuild.Append ("GameObject Path:"+list[i]+"\n");
				Image [] comps = obj.transform.GetComponentsInChildren<Image>();
				//Debug.Log (" sprite1 "+comps[j].sprite.name+" sprite2 "+comps[j].overrideSprite.name+" sprite3 "+comps[j].mainTexture.name);
				for (int j = 0; j < comps.Length; j++) {
					if (comps [j] != null &&comps [j].sprite ==null) {
						sbuild.Append (" \t chid Object name:"+comps[j].gameObject.name+"\n");
					}
				}
				
				SerializedObject so = new SerializedObject(comps[i]);
				var sp = so.GetIterator();
				while (sp.NextVisible(true))
				{
					if (sp.propertyType == SerializedPropertyType.ObjectReference)
					{
						if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
						{
							sbuild.Append ("GameObject Path:"+comps[i].gameObject.name+"\n");
						}
					}
				}
			}
		}
		Debug.Log (sbuild.ToString());
	}

	[MenuItem("Custom/CheckTexture")]
	public static void CheckTexture()
	{
		TextureImporterFormat formatType;
		TextureImporterFormat specialFormatType;
		int type = 1;//1安卓，2苹果
		string platform = "Standalone";//平台
		if (type == 1) platform = "Android";
		else if (type == 2) platform = "iPhone";
		
		bool withApha = false; //贴图是否带 Apha 
		string[] Extensions = { "*.png","*.jpg","*.tga"}; //要处理的贴图后缀
		string texturePath = Application.dataPath+"/Texture";//贴图路径

		int maxSize = 1024;
		int compressionQuality;
		if (type == 1)
		{
			formatType = withApha ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC_RGB4;
			specialFormatType = TextureImporterFormat.ETC2_RGBA8;
			compressionQuality = 100;
		}
		else
		{
			formatType = withApha ? TextureImporterFormat.PVRTC_RGBA4 : TextureImporterFormat.PVRTC_RGB4;
			specialFormatType = TextureImporterFormat.RGBA16;
			compressionQuality = 100;
		}
	
		TextureImporter texture;
		TextureImporterPlatformSettings tis;
		TextureImporterFormat tif;
		for (int j = 0; j < Extensions.Length; j++)
		{
			string extension = Extensions[j];
			string [] files = Directory.GetFiles(texturePath, extension, SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++) 
			{
				string info = (i + 1) + "/" + files.Length+"total: ("+j+"/"+Extensions.Length+")";
				float progress = (i + 1) * 1.0f / files.Length;
				if (EditorUtility.DisplayCancelableProgressBar("修改贴图中", info + " ", progress))
				{
					break;
				}
				
				FileInfo file = new FileInfo(files[i]);
				string path = file.FullName.Substring(file.FullName.IndexOf("Assets"));
				texture = (TextureImporter)TextureImporter.GetAtPath(path);
				tis = texture.GetPlatformTextureSettings(platform);
				
				if (texture.textureType != TextureImporterType.Default || texture.isReadable || texture.mipmapEnabled ||
				    texture.npotScale != TextureImporterNPOTScale.ToNearest || tis.format != formatType ||
				    tis.compressionQuality != compressionQuality || !tis.overridden || (maxSize != -1 && tis.maxTextureSize != maxSize))
				{
					texture.textureType = TextureImporterType.Default;
					texture.isReadable = false;
					texture.mipmapEnabled = false;
					texture.npotScale = TextureImporterNPOTScale.ToNearest;

					tis.format = formatType;
					tis.compressionQuality = compressionQuality;
					tis.overridden = true;
					if(maxSize != -1) tis.maxTextureSize = maxSize;
					texture.SetPlatformTextureSettings(tis);
					texture.SaveAndReimport();
				}
				
			}
		}
		EditorUtility.ClearProgressBar();
		AssetDatabase.Refresh();
	}


	static List<string> CheckFolderSprites(string selectPath)
	{
		List<string> assetPathList =new List<string>();
		try {
			DirectoryInfo dirinfo =new DirectoryInfo(selectPath);
			FileInfo [] fileList=dirinfo.GetFiles("*.prefab");
			for (int i = 0; i < fileList.Length; ++i)
			{
				string file_path = fileList[i].FullName.Replace("\\","/");
				string asset_path = file_path.Replace(Application.dataPath, "Assets");
				assetPathList.Add(asset_path);
			}
		} catch (Exception ex) {
			Debug.LogError (ex.Message);
		}
		return assetPathList;
	}


	static void OutPutImagesPath(List<string> assetPathList)
	{
		//EditorUtility.DisplayProgressBar("Check Assets ", "Check Assets ...", 0f);
		StringBuilder sbuild = new StringBuilder ();
		for (int i = 0; i < assetPathList.Count; ++i) {
			GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject> (assetPathList[i]);
			if(obj!= null)
			{
				sbuild.Append ("GameObject Path:"+assetPathList[i]+"\n");
				Image [] comps = obj.transform.GetComponentsInChildren<Image>();
				for (int j = 0; j < comps.Length; j++) {
					if (comps [j] != null) {
						Sprite _sprite = comps [j].sprite;
						if (_sprite != null && _sprite.texture != null ) {
							Texture _texture = _sprite.texture;
							string _tmpPath = AssetDatabase.GetAssetPath (_texture);
							sbuild.Append (" \t Texture:"+_tmpPath+"\n");
						}
					}
				}
				//EditorUtility.DisplayProgressBar("Check Assets ", "Check Assets ...", i/assetPathList.Count);
			}
		}
		//save log 
		//EditorUtility.ClearProgressBar ();
		Debug.Log (sbuild.ToString());
	}
	
	
	[MenuItem("Custom/build Android")]
	public static void BuildAndroid()
	{
		string time = DateTime.Now.ToString("MM.dd");
		string savePath = "/Users/Desktop/" + "Android" + time;
		Debug.Log("==savePath:"+savePath);
		DirectoryInfo dirinfo = new DirectoryInfo(savePath);
		if (dirinfo.Exists)
		{
			dispDirs(dirinfo);
		}
		else
		{
			dirinfo.Create();
		}
		BuildPipeline.BuildPlayer(FindEnableEditorrScenes(), savePath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
	}
	
	
	[MenuItem("Custom/BuildController")]
	public static void autioBuildController()
	{
		AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath("Assets/animation.controller");
		AnimatorControllerLayer layer = animatorController.layers[0];
		
		//clip文件名
		List<string> list = new List<string>();
		list.Add("AccountToRoot");
		list.Add("root");

		
		for (int i = 0; i < list.Count; i++)
		{
			//文件路径
			string path = string.Format("Assets/Res/Ani/cam/{0}.anim", list[i]);
			AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
			if (clip != null)
			{
				AnimatorStateMachine stateMachine = layer.stateMachine;
				AnimatorState state = stateMachine.AddState(clip.name);
				state.motion = clip;
				//相当于 AnimatorController 中的连线
				AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(state);
				
				
			}
			else
			{
				Debug.Log("路径找不到 animation clip文件 ");
			}
		}
		AssetDatabase.SaveAssets();
	}

	[MenuItem("Custom/SetTextureConfig")]
	public static void SetTextureConfig()
	{
		string filePath = "";
		Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
		if(texture==null)
			return;
		int textureSize = Mathf.Max(texture.height, texture.width);
		TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;

		importer.textureType = TextureImporterType.Default;
		importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
		{
			overridden = true,
			name = "Android", //iPhone
			maxTextureSize = textureSize,
			format = TextureImporterFormat.ETC_RGB4Crunched, //ASTC ios 平台
			textureCompression = TextureImporterCompression.Compressed,
			resizeAlgorithm = TextureResizeAlgorithm.Mitchell
		});
		importer.SaveAndReimport();
		AssetDatabase.ImportAsset(filePath);
		
		AssetDatabase.Refresh();
	}

	public static string[] FindEnableEditorrScenes()
	{
		List<string> editorScenes = new List<string>();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (!scene.enabled) continue;
			editorScenes.Add(scene.path);
		}
		return editorScenes.ToArray();
	}
	
	public static void dispDirs(DirectoryInfo dir)
	{
		//返回目录中的目录列表对象也就是所有子目录
		DirectoryInfo[] directories = dir.GetDirectories();
		//返回目录下的所有文件
		FileInfo[] f = dir.GetFiles();
		foreach (FileInfo item in f)
		{
			item.Delete();//删除目录下所有文件
		}
		foreach (DirectoryInfo dirX in directories)
		{
			dispDirs(dirX);//递归删除所有文件夹
			dirX.Delete();
		}
	}
}
