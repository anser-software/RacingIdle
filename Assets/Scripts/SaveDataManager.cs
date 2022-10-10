using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Serialization;
using Newtonsoft;
using Newtonsoft.Json.Converters;

public static class SaveDataManager
{

    private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        TypeNameHandling = TypeNameHandling.All
    };

    private static string GetTypeDataPath(Type type, string key = "")
    {
        if(key != "")
        {
            key = "_" + key;
        }

        return Application.persistentDataPath + "\\" + type.Name + key + ".json";
    }


    public static void SaveAll(string key = "")
    {
        Debug.Log("Saving data...");

        var isaveableT = typeof(ISaveable);

        var savedTypes = AppDomain.CurrentDomain.GetAssemblies().
            SelectMany(a => a.GetTypes().Where(t => isaveableT.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)).ToArray();

        foreach (var savedT in savedTypes)
        {
            var saveData = new SaveData(savedT);

            var tInstance = UnityEngine.Object.FindObjectOfType(savedT, true);

            if (tInstance == null)
                continue;

            ((ISaveable)tInstance).OnBeforeDataSaved();

            foreach (var field in savedT.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var saveThisAttr = field.GetCustomAttribute<SaveThisAttribute>();

                var isSaved = saveThisAttr != null;

                if(isSaved)
                {
                    Debug.Log(string.Format("Field {0} of class {1} is saved", field.Name, savedT.Name));

                    saveData.savedObjectsAndTheirNames[field.Name] = field.GetValue(tInstance);
                }
            }

            var jsonObj = JsonConvert.SerializeObject(saveData, saveData.GetType(), Formatting.Indented, jsonSerializerSettings);

            File.WriteAllText(GetTypeDataPath(savedT, key), jsonObj);
        }
    }

    public static void SaveCustom<T>(T data, string key = "")
    {
        var savedT = typeof(T);

        var saveData = new SaveData(savedT);

        foreach (var field in savedT.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            Debug.Log(string.Format("Field {0} of class {1} is saved", field.Name, savedT.Name));

            saveData.savedObjectsAndTheirNames[field.Name] = field.GetValue(data);
        }

        var jsonObj = JsonConvert.SerializeObject(saveData, saveData.GetType(), Formatting.Indented, jsonSerializerSettings);

        var filePath = GetTypeDataPath(savedT, key);

        File.WriteAllText(filePath, jsonObj);
    }

    public static bool IsSaved<T>(string key = "")
    {
        var savedT = typeof(T);

        var filePath = GetTypeDataPath(savedT, key);

        return File.Exists(filePath);
    }

    public static T LoadCustom<T>(string key = "")
    {
        var savedT = typeof(T);

        var filePath = GetTypeDataPath(savedT, key);

        if (!File.Exists(filePath))
        {
            Debug.LogWarning(string.Format("No {0} with key {1} is saved", savedT.Name, key));
            return default(T);
        }

        T tInstance = Activator.CreateInstance<T>();

        var jsonObj = File.ReadAllText(GetTypeDataPath(savedT, key));

        var saveData = JsonConvert.DeserializeObject<SaveData>(jsonObj, jsonSerializerSettings);

        foreach (var varName in saveData.savedObjectsAndTheirNames.Keys.ToList())
        {
            var field = savedT.GetField(varName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var value = saveData.savedObjectsAndTheirNames[varName];

            value = Convert.ChangeType(value, field.FieldType);

            field.SetValue(tInstance, value);

            Debug.Log(string.Format("Field {0} of class {1} is loaded", varName, savedT.Name));
        }

        return tInstance;
    }

    public static void LoadAll(string key = "")
    {
        Debug.Log("Loading data...");

        var isaveableT = typeof(ISaveable);

        var savedTypes = AppDomain.CurrentDomain.GetAssemblies().
            SelectMany(a => a.GetTypes().Where(t => isaveableT.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)).ToArray();

        foreach (var savedT in savedTypes)
        {
            var tInstance = UnityEngine.Object.FindObjectOfType(savedT, true);

            if (tInstance == null)
                continue;

            var filePath = GetTypeDataPath(savedT, key);

            if (!File.Exists(filePath))
            {
                ((ISaveable)tInstance).OnNoSaveDataFound();
                continue;
            }

            var jsonObj = File.ReadAllText(filePath);

            var saveData = JsonConvert.DeserializeObject<SaveData>(jsonObj, jsonSerializerSettings);

            foreach (var varName in saveData.savedObjectsAndTheirNames.Keys.ToList())
            {
                var field = savedT.GetField(varName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var value = saveData.savedObjectsAndTheirNames[varName];

                value = Convert.ChangeType(value, field.FieldType);

                field.SetValue(tInstance, value);

                Debug.Log(string.Format("Field {0} of class {1} is loaded", varName, savedT.Name));
            }

            ((ISaveable)tInstance).OnAfterDataLoaded();
        }
    }

}

public interface ISaveable
{

    public void OnBeforeDataSaved() { }

    public void OnAfterDataLoaded() { }

    public void OnNoSaveDataFound() { }

}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SaveThisAttribute : Attribute
{
    public SaveThisAttribute() { }
}

[Serializable]
public class SaveData
{

    public Type ClassType;

    public Dictionary<string, object> savedObjectsAndTheirNames;

    public SaveData(Type classType)
    {
        ClassType = classType;

        savedObjectsAndTheirNames = new Dictionary<string, object>();
    }

}