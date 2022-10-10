using UnityEngine;
using UnityEditor;
using PathCreation;
using Newtonsoft;
using Newtonsoft.Json;
using System.IO;

namespace PathCreation.Examples
{
    [CustomEditor(typeof(PathSceneTool), true)]
    public class PathSceneToolEditor : Editor
    {
        protected PathSceneTool pathTool;
        bool isSubscribed;

        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,            
        };

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                DrawDefaultInspector();

                if (check.changed)
                {
                    if (!isSubscribed)
                    {
                        TryFindPathCreator();
                        Subscribe();
                    }

                    if (pathTool.autoUpdate)
                    {
                        TriggerUpdate();

                    }
                }
            }

            if (GUILayout.Button("Manual Update"))
            {
                if (TryFindPathCreator())
                {
                    TriggerUpdate();
                    SceneView.RepaintAll();
                }
            }


            if (GUILayout.Button("Save Path"))
            {
                if (TryFindPathCreator())
                {
                    SavePath();
                    SceneView.RepaintAll();
                }
            }

        }


        void TriggerUpdate() {
            if (pathTool.pathCreator != null) {
                pathTool.TriggerUpdate();
            }
        }

        private void SavePath()
        {
            TextAsset[] roadFiles = Resources.LoadAll<TextAsset>("Roads");

            var fileName = "Road_" + roadFiles.Length.ToString() + ".json";

            var filePath = Application.dataPath + "/Resources/Roads/" + fileName;

            var currentRoadBezierPath = pathTool.pathCreator.bezierPath;

            var jsonObj = JsonConvert.SerializeObject(currentRoadBezierPath, typeof(BezierPath), jsonSerializerSettings);

            File.WriteAllText(filePath, jsonObj);
            AssetDatabase.Refresh();
        }

        protected virtual void OnPathModified()
        {
            if (pathTool.autoUpdate)
            {
                TriggerUpdate();
            }
        }

        protected virtual void OnEnable()
        {
            pathTool = (PathSceneTool)target;
            pathTool.onDestroyed += OnToolDestroyed;

            if (TryFindPathCreator())
            {
                Subscribe();
                TriggerUpdate();
            }
        }

        void OnToolDestroyed() {
            if (pathTool != null) {
                pathTool.pathCreator.pathUpdated -= OnPathModified;
            }
        }

 
        protected virtual void Subscribe()
        {
            if (pathTool.pathCreator != null)
            {
                isSubscribed = true;
                pathTool.pathCreator.pathUpdated -= OnPathModified;
                pathTool.pathCreator.pathUpdated += OnPathModified;
            }
        }

        bool TryFindPathCreator()
        {
            // Try find a path creator in the scene, if one is not already assigned
            if (pathTool.pathCreator == null)
            {
                if (pathTool.GetComponent<PathCreator>() != null)
                {
                    pathTool.pathCreator = pathTool.GetComponent<PathCreator>();
                }
                else if (FindObjectOfType<PathCreator>())
                {
                    pathTool.pathCreator = FindObjectOfType<PathCreator>();
                }
            }
            return pathTool.pathCreator != null;
        }
    }
}