using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;

using Unity.MLAgentsExamples;
using UnityEngine.UI;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[Serializable]
public class TrainerStatus
{
    public int learningTeamId;
}

[Serializable]
public class EnvConfig
{     
    public bool sameModel;
    public bool coplayLearningTeamOnly;
    public string trainerStatusPath;
    public int numberOfCoplayAgents;
    public string modelPath;
    public int keepModels;
    public float selfPlayRatio;
}


public class CoPlayModels : MonoBehaviour
{
    public string EnvConfigPath = @"env_config.yaml";
    public EnvConfig envConfig;
    public TrainerStatus trainerStatus;

    public List<string> modelPathList = new List<string>();
    public List<NNModel> modelList = new List<NNModel>();
    public ModelOverrider myModelOverrider;

    private void ApplyEnvConfig() {
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
        string yaml = File.ReadAllText(EnvConfigPath);
        envConfig = deserializer.Deserialize<EnvConfig>(yaml);
        //print("loading: " + envConfig.trainerStatusPath);
        yaml = File.ReadAllText(envConfig.trainerStatusPath);
        trainerStatus = deserializer.Deserialize<TrainerStatus>(yaml);
        //print("trainerStatus.learningTeamId: " + trainerStatus.learningTeamId);
    }


    void InitializeModelList()
    {
        Debug.Log("Loading models from path");
        Debug.Log(envConfig.modelPath);
        string[] filePaths = Directory.GetFiles(envConfig.modelPath, "*.onnx");
        if(filePaths.Length == 0)
        {
            Debug.Log("path is empty NO models loaded");
        }else{
            foreach (string path in filePaths)
            {
                Debug.Log("Loading model ");
                Debug.Log(path);
                String modelName = path.Replace(".onnx", "");
                modelPathList.Add(modelName);
                byte[] rawModel = File.ReadAllBytes(path);
                NNModel nnModel = myModelOverrider.LoadOnnxModel(rawModel);
                nnModel.name = modelName;
                modelList.Add(nnModel);
            }
        }
    }

    void Start()
    {
        if (myModelOverrider == null) {
            Debug.Log("Overrider null");

        }
        Debug.Log("Coplay start");


        ApplyEnvConfig();
        InitializeModelList();
    }

    public void CheckAndAppendNewModels()
    {
        ApplyEnvConfig();
        string[] filePaths = Directory.GetFiles(envConfig.modelPath, "*.onnx");
        foreach (string path in filePaths)
        {
            String modelName = path.Replace(".onnx", "");
            if(modelPathList.Contains(modelName) == false)
            {                
                if(modelList.Count >= envConfig.keepModels)
                {
                    //todo: it is bugged, rewrite
                    Debug.Log("modelList is full, not adding model");
                    Debug.Log(modelList.Count);
                    //Debug.Log("removing");
                    //Debug.Log(modelPathList[0]);
                    //modelPathList.RemoveAt(0);
                    //modelList.RemoveAt(0);                    
                }else{
                    Debug.Log("Found new model in modelPath, appending to modelList");
                    Debug.Log(path);
                    modelPathList.Add(modelName);                    
                    byte[] rawModel = File.ReadAllBytes(path);
                    NNModel nnModel = myModelOverrider.LoadOnnxModel(rawModel);
                    nnModel.name = modelName;
                    modelList.Add(nnModel);
                    Debug.Log(String.Concat("modelList size = ", modelList.Count));
                }
            }
        }
    }
}
