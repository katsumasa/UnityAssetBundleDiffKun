﻿using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UTJ.UnityCommandLineTools;
using System.Linq;

#if UNITY_EDITOR
namespace UTJ.UnityAssetBundleDiffKun
{
    // <summary>
    // Diffの結果を行うViewClass   
    // </summary>
    [System.Serializable]
    public class DiffView
    {
        public delegate int Callback();

        [SerializeField] string mText = "";
        [SerializeField] string mTextFile1Path;
        [SerializeField] string mTextFile2Path;
        Vector2 mScrollPos;

        private Callback mCallback;
        
        public Callback callback
        {
            set { mCallback = value; }
        }
        

        static readonly string[] mOptions =
        {
            "--normal",
            "--side-by-side"
        };
        int mOptionSelectIdx = 0;


        public string textFile1Path
        {
            set { mTextFile1Path = value; }
        }
        
        public string textFile2Path
        {
            set { mTextFile2Path = value; }
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("DIFF"))
            {
                Diff();
            }
            mOptionSelectIdx = EditorGUILayout.Popup(mOptionSelectIdx, mOptions);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Verify"))
            {
                var result = mCallback();
            }
            mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);            
            EditorGUILayout.TextArea(mText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        public int Diff()
        {
            var diff = new DiffExec();
            var result = diff.Exec(mTextFile1Path, mTextFile2Path, mOptions[mOptionSelectIdx]);
            mText = diff.output;
            return result;
        }

        void Verify()
        {

        }
    }


    // <summary>
    // AssetBundleの中身を表示する為のClass
    // programed by Katsumasa.Kimura
    // </summary>
    [System.Serializable]
    public class ABTextView
    {
        [SerializeField] Object mObject;
        public Object obj {
            get { return mObject; }
        }

        [SerializeField] string mText;
        public string text
        {
            get { return mText; }
        }
        
        [SerializeField] string mTextFilePath;
        public string textFilePath
        {
            get { return mTextFilePath; }
        }

        // <value>
        // AssetBundleに含まれているAssetへのパス
        // </value>
        [SerializeField] List<string> mAssetPaths;

        public List<string> assetPaths
        {
            get { return mAssetPaths; }
        }

        // <value>
        // mAssetPathsからファイル名のみ抽出したテーブル
        // </value>
        [SerializeField] string[] mAssetNames;
        public string[] assetNames
        {
            get { return mAssetNames; }
        }

        // <value>
        // mAssetPaths/mAssetNamesへのインデックス
        // </value>
        [SerializeField] int mSelectedIndex = 0;
        public int selectIndex
        {
            get { return mSelectedIndex; }
            set { mSelectedIndex = value; }
        }


        Vector2 mScrollPos;
        
        // <summary>
        // Workフォルダ
        // </summary>
        string mWorkFolderBase;


        // <sumamry>
        // コンストラクタ
        // </summary>
        public ABTextView()
        {
            mAssetNames = new string[0];
            mWorkFolderBase = "ABTextView"+base.GetHashCode().ToString();
        }

        // <summary>
        // 描画処理
        // </summary>
        public void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("WebExtract"))
            {
                if (mObject != null)
                {
                    WebExtract(true);
                }
            }
            mObject = EditorGUILayout.ObjectField(mObject, typeof(Object), true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bin2Text"))
            {
                Bin2Text(true);
            }
            mSelectedIndex = EditorGUILayout.Popup(mSelectedIndex, mAssetNames);
            GUILayout.EndHorizontal();

            mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);
            EditorGUILayout.TextArea(mText,GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        // <summary>
        // AssetBundleを展開する
        // </summary>
        public int WebExtract(bool IsOpenSuccessDialog)
        {            
            var workFolderPath = Path.Combine(Application.temporaryCachePath, mWorkFolderBase);
            if (Directory.Exists(workFolderPath) == false)
            {
                Directory.CreateDirectory(workFolderPath);
            }
            var src = AssetDatabase.GetAssetOrScenePath(mObject);
            var assetBundleFileName = Path.GetFileName(src);
            var dst = Path.Combine(workFolderPath, assetBundleFileName);
            File.Copy(src, dst, true);
            var exec = new WebExtractExec();
            var path = Path.Combine(workFolderPath, assetBundleFileName);
            var result = exec.Exec($@"""{path}""");
            if(result != 0)
            {
                EditorUtility.DisplayDialog("WebExtract", "Fail", "OK");
                return result;
            }
            else if (IsOpenSuccessDialog)
            {
                EditorUtility.DisplayDialog("WebExtract", "Success", "OK");
            }                        
            var execFolderPath = dst + "_data";

            if (mAssetPaths == null)
            {
                mAssetPaths = new List<string>();
            } 
            else
            {
                mAssetPaths.Clear();
            }

            var files = Directory.GetFiles(execFolderPath, "BuildPlayer-*", SearchOption.AllDirectories);
            mAssetPaths.AddRange(files);
            // リソースファイルを含めない(拡張子無しのファイルのみを含める)ように修正
            files = Directory.GetFiles(execFolderPath, "CAB-*.", SearchOption.AllDirectories);
            mAssetPaths.AddRange(files);
            mAssetPaths.Sort();

            mAssetNames = new string[mAssetPaths.Count];
            for(var i = 0; i < mAssetPaths.Count; i++)
            {
                mAssetNames[i] = Path.GetFileName(mAssetPaths[i]);                
            }
            mSelectedIndex = 0;

            return 0;
        }

        // <summary>
        // bin2textを実行する
        // </summary>
        public int Bin2Text(bool IsOpenSuccessDialog)
        {
            if (mSelectedIndex != -1)
            {
                var workFolderPath = Path.Combine(Application.temporaryCachePath, mWorkFolderBase);
                mTextFilePath = Path.Combine(workFolderPath, mAssetNames[mSelectedIndex]) + ".txt";
                var b2t = new Binary2TextExec();
                var result = b2t.Exec(mAssetPaths[mSelectedIndex], mTextFilePath, "");
                        
                if (result != 0)
                {
                    EditorUtility.DisplayDialog("Bin2Text", b2t.output, "OK");
                }
                else 
                {
                    mText = File.ReadAllText(mTextFilePath);
                    if (IsOpenSuccessDialog)
                    {
                        EditorUtility.DisplayDialog("Bin2Text", "Success", "OK");
                    }
                }                    
                return result;
            }
            else
            {
                EditorUtility.DisplayDialog("Bin2Text", "File is not selected.", "OK");
            }
            return -1;
        }
    }


    public class UnityAssetBundleDiffKunEditorWindow : EditorWindow
    {
        static UnityAssetBundleDiffKunEditorWindow editorWindow;
        
        [SerializeField] ABTextView[] mABTextView;
        
        [SerializeField] DiffView mDiffView;

        // <summary>
        // EditorWindowを開く
        // </summary>
        [MenuItem("Window/UTJ/UnityAssetBundleDiffKun")]
        static void Create()
        {
            if (editorWindow == null)
            {
                editorWindow = (UnityAssetBundleDiffKunEditorWindow)EditorWindow.GetWindow(typeof(UnityAssetBundleDiffKunEditorWindow));
            }
            editorWindow.titleContent = new GUIContent("UnityAssetBundleDiffKun");
            editorWindow.wantsMouseMove = true;
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.Show();
        }


        private void OnGUI()
        {
            if (mABTextView == null)
            {
                mABTextView = new ABTextView[2];
                for (var i = 0; i < mABTextView.Length; i++)
                {
                    mABTextView[i] = new ABTextView();
                }
            }

            if (mDiffView == null)
            {
                mDiffView = new DiffView();
                mDiffView.callback = Verify;
            }

            GUILayout.BeginHorizontal();
            mABTextView[0].OnGUI();

            mDiffView.textFile1Path = mABTextView[0].textFilePath;
            mDiffView.textFile2Path = mABTextView[1].textFilePath;
            mDiffView.callback = Verify;
            mDiffView.OnGUI();

            mABTextView[1].OnGUI();
            GUILayout.EndHorizontal();
        }


        int Verify()
        {

            if(mABTextView[0].obj == null || mABTextView[1].obj == null)
            {
                EditorUtility.DisplayDialog("Verify", "The AssetBundle to be compared is not set.", "OK");

                return -1;
            }
            int result;
            result = mABTextView[0].WebExtract(false);
            if(result != 0)
            {
                return -2;
            }
            result = mABTextView[1].WebExtract(false);
            if(result != 0)
            {
                return -3;
            }
            if(mABTextView[0].assetNames.Length != mABTextView[1].assetNames.Length)
            {
                return 1;
            }
            for(var i = 0; i < mABTextView[0].assetNames.Length; i++)
            {
                if(mABTextView[0].assetNames.Contains(mABTextView[1].assetNames[i]) == false)
                {
                    EditorUtility.DisplayDialog("Verify", "Invalid serialize file.", "OK");
                    return 2;
                }
                if(mABTextView[1].assetNames.Contains(mABTextView[0].assetNames[i]) == false)
                {
                    EditorUtility.DisplayDialog("Verify", "Invalid serialize file.", "OK");
                    return 3;
                }
            }

            for (var i = 0; i < mABTextView[0].assetNames.Length; i++)
            {
                mABTextView[0].selectIndex = i;
                mABTextView[1].selectIndex = i;
                result = mABTextView[0].Bin2Text(false);
                if(result != 0)
                {
                    return -4;
                }
                result = mABTextView[1].Bin2Text(false);
                if(result != 0)
                {
                    return -5;
                }
                result = mDiffView.Diff();
                if(result != 0)
                {
                    EditorUtility.DisplayDialog("Verify", "No match.", "OK");
                    return 4;
                }
            }

            EditorUtility.DisplayDialog("Verify", "Match", "OK");

            return 0;
        }
    }
}
#endif