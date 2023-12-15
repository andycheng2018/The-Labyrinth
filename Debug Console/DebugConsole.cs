using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Reflection;

public class DebugConsole : MonoBehaviour
{     
    private List<DebugCommand> debugCommands = new List<DebugCommand>();
    private string logText = "";
    private Vector2 scrollPosition = Vector2.zero;
    private bool showConsole;
    private string input;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    public void OnToggleDebug(InputValue value) {
        showConsole = !showConsole;
    }

    public void OnReturn(InputValue value) {
        if (showConsole) {
            HandleInput();
        }
    }

    private void Awake() {
        ScanAndInvokeCommands();
    }

    private void ScanAndInvokeCommands()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                var methods = type.GetMethods();

                foreach (var method in methods)
                {
                    var commandAttribute = (CommandAttribute)Attribute.GetCustomAttribute(method, typeof(CommandAttribute));

                    if (commandAttribute != null)
                    {
                        var instance = gameObject.GetComponent(type) ?? gameObject.AddComponent(type);
                        var parameters = method.GetParameters();
                        DebugCommand obj;
                        
                        if (parameters.Length == 0)
                        {
                            obj = new DebugCommand(commandAttribute.commandId, commandAttribute.commandDescription, () => {
                                method.Invoke(instance, null);
                            });
                            debugCommands.Add(obj);
                        }
                        else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                        {
                            obj = new DebugCommand(commandAttribute.commandId, commandAttribute.commandDescription, (x) => {
                                method.Invoke(instance, new object[] { x });
                            });
                            debugCommands.Add(obj);
                        }

                        Debug.Log(commandAttribute.commandId);
                    }
                }
            }
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        logText += "<b><color=yellow>" + "HI" + "</color></b>" + ": " + logString + "\n";

        switch (type)
        {
            case LogType.Warning:
                break;
            case LogType.Error:
            case LogType.Exception:
                break;
        }
    }

    private void OnGUI() {
        if (!showConsole) { return; }

        GUI.Box(new Rect(0, 0, Screen.width, 30), "");
        GUI.backgroundColor = new Color(1f, 1f, 1f, 1f);
        GUI.SetNextControlName("console");
        input = GUI.TextField(new Rect(10f, 5f, Screen.width - 30f, 20f), input);   
        GUI.FocusControl("console");

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;
        GUIStyle style2 = new GUIStyle(GUI.skin.box);
        style2.normal.textColor = Color.blue;
        style2.alignment = TextAnchor.UpperLeft;
        float textLength = GUI.skin.box.CalcHeight(new GUIContent(logText), Screen.height - 20);
        float textWidth = style.CalcSize(new GUIContent(logText)).x + 390;
        scrollPosition = GUI.BeginScrollView(new Rect(10f, 35f, Screen.width - 20, 100f), scrollPosition, new Rect(0, 0, textWidth, textLength));
        GUI.Box(new Rect(0, 0, textWidth, textLength), logText, style);
        GUI.EndScrollView();
    }

    private void HandleInput() {
        string[] properties = input.Split(' ');

        for (int i = 0; i < debugCommands.Count; i++) {
            if (input != null && debugCommands[i].commandId.Contains(properties[0])) {

                Debug.Log("<b><color=red>" + input + "</color></b>");
                
                if (properties.Length > 1) {
                    string value = properties[1];
                    debugCommands[i].Invoke(value);
                   
                }
                else {
                    debugCommands[i].Invoke();
                }
            }
        }

        input = "";
    }

    [Command("Help", "Shows a list of all commands")]
    public void Help() {
        for(int i = 0; i < debugCommands.Count; i++) {
            DebugCommandBase command = debugCommands[i];
            string label = $"<b><color=red>{command.commandId}</color></b> - {command.commandDescription}";
            Debug.Log(label);
        }
    }

    [Command("Clear", "Clears all commands")]
    public void Clear() {
        logText = "";
    }
}
