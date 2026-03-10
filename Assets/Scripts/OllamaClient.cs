using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:11434";
    [SerializeField] private string model = "qwen2.5:7b";
    [SerializeField] private float temperature = 0.4f;

    [Serializable]
    private class ChatRequest
    {
        public string model;
        public bool stream;
        public float temperature;
        public Message[] messages;
    }

    [Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class ChatResponse
    {
        public Message message;
    }

    public async Task<string> ChatOnceAsync(string systemPrompt, string userPrompt)
    {
        var req = new ChatRequest
        {
            model = model,
            stream = false,
            temperature = temperature,
            messages = new[]
            {
                new Message { role = "system", content = systemPrompt },
                new Message { role = "user", content = userPrompt }
            }
        };

        string json = JsonUtility.ToJson(req);
        var url = $"{baseUrl}/api/chat";

        using var www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        var op = www.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Ollama error: {www.error}\n{www.downloadHandler.text}");
            return "(LLM error)";
        }

        var res = JsonUtility.FromJson<ChatResponse>(www.downloadHandler.text);
        return res?.message?.content ?? "(no response)";
    }
}

