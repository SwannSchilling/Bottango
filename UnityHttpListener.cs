using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class UnityHttpListener : MonoBehaviour
{
    private HttpListener listener;
    private Thread listenerThread;

    public List<string> myServo = new List<string>();

    private string echo;

    private string ListToText(List<string> list)
    {
        string result = "";
        foreach (var listMember in list)
        {
            result += listMember.ToString() + "\n";
        }
        return result;
    }

    void Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:4444/");
        listener.Prefixes.Add("http://127.0.0.1:4444/");
        listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
        listener.Start();

        listenerThread = new Thread(startListener);
        listenerThread.Start();
        Debug.Log("Server Started");
    }

    void Update()
    {
    }

    private void startListener()
    {
        while (true)
        {
            var result = listener.BeginGetContext(ListenerCallback, listener);
            result.AsyncWaitHandle.WaitOne();
        }
    }

    private void ListenerCallback(IAsyncResult result)
    {
        var context = listener.EndGetContext(result);

        Debug.Log("Method: " + context.Request.HttpMethod);
        Debug.Log("LocalUrl: " + context.Request.Url.LocalPath);

        if (context.Request.QueryString.AllKeys.Length > 0)
            foreach (var key in context.Request.QueryString.AllKeys)
            {
                Debug.Log("Key: " + key + ", Value: " + context.Request.QueryString.GetValues(key)[0]);
                myServo.Add(key);
                myServo.Add(context.Request.QueryString.GetValues(key)[0]);
            }

        echo = ListToText(myServo);
        myServo.Clear();
  
        if (context.Request.HttpMethod == "POST")
        {
            Thread.Sleep(1000);
            var data_text = new StreamReader(context.Request.InputStream,
                                context.Request.ContentEncoding).ReadToEnd();
            Debug.Log(data_text);
            echo = data_text;
        }

        // Obtain a response object.
        HttpListenerResponse response = context.Response;
        // Construct a response.
        string responseString = echo;
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        // You must close the output stream.
        output.Close();
    
        context.Response.Close();
    }

}
