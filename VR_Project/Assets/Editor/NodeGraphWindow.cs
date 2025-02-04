﻿using UnityEngine;
using UnityEditor;

public class NodeGraphWindow : EditorWindow
{
    float m_nodeDistance = 1.6f;
    int m_nodeConnectionAmount = 16;
    float m_ySpaceLimit = 1;
    bool loaded = false;

    
    
    private static GameObject walkableObjects = null;
    private static NodeContainer nodeContainer = null;

    private int m_layerMask = 0 << 1;
    
    [MenuItem("Window/NodeGraph")] 
    public static void ShowWindow()
    {
        GetWindow(typeof(NodeGraphWindow));
    }
    
    private void OnGUI()
    {
        
        if (loaded == false)
        {
            LoadFile();
            loaded = true;
        }
        
        SaveSystem.SaveData(m_nodeDistance, m_nodeConnectionAmount, m_ySpaceLimit, Application.dataPath + "/Editor/Config.json");
        
        GUILayout.Label("Node Settings", EditorStyles.boldLabel);
        m_nodeDistance = EditorGUILayout.FloatField("Node Join Distance", m_nodeDistance);
        m_nodeConnectionAmount = EditorGUILayout.IntField("Max connections", m_nodeConnectionAmount);
        m_ySpaceLimit = EditorGUILayout.FloatField("Minimum Y Distance", m_ySpaceLimit);

        walkableObjects = (GameObject) EditorGUILayout.ObjectField("Environment Container", walkableObjects, typeof(GameObject), true);
        nodeContainer = (NodeContainer)EditorGUILayout.ObjectField("Node Container", nodeContainer, typeof(NodeContainer),true);
        
        
        
        if (GUILayout.Button("Bake Nodes"))
        {
            EditorUtility.SetDirty(nodeContainer);
            //float time = Time.realtimeSinceStartup;
            if (walkableObjects == null || nodeContainer == null)
                Debug.LogWarning("Please fill walkable container and node container fields before baking");
            
            NodeManager.ChangeValues(m_nodeDistance, m_nodeConnectionAmount,m_ySpaceLimit, nodeContainer, walkableObjects);
            NodeManager.CreateNodes(m_layerMask);
            AssetDatabase.SaveAssets();
            //Debug.Log(Time.realtimeSinceStartup - time);
        }
        if (GUILayout.Button("Show Links"))
        {
            if (nodeContainer.NodeGraph == null)
                return;

            foreach (var node in nodeContainer.NodeGraph)
            {
                for (int i = 0; i < node.connections.Length - 1; i++)
                {
                    //if the connection isnt null then draw a line of it this whole function is self explaining
                    if (node.connections[i] != null)
                        if(node.connections[i].to != -1)
                            Debug.DrawLine(node.m_position, nodeContainer.NodeGraph[node.connections[i].to].m_position);
                }
            }
        }
    }
    private void LoadFile()
    {
        //load all the settings and if its null we can keep the defaults
        EditorValues loadedSettings = SaveSystem.LoadData(Application.dataPath + "/Editor/Config.json");
        if (loadedSettings == null)
            return;

        m_nodeDistance = loadedSettings.m_distance;
        m_nodeConnectionAmount = loadedSettings.m_nodeConnectionAmount;
        m_ySpaceLimit = loadedSettings.m_ySpaceLimit;
    }
}
