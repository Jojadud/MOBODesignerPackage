using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;

namespace MOBODesigner
{
    public class MDClient
    {
        public Action<List<float>> OnNewDesignReceived;

        private HttpListener listener;
        private Thread listenerThread;

        private int pId_ = 0;

        public MDClient(int pId)
        {
            pId_ = pId;
        }

        ~MDClient()
        {

        }

        public void InitializeListener(string clientAddress, string port)
        {
            listener = new HttpListener();
            string uri = clientAddress + ":" + port + "/";
            listener.Prefixes.Add(uri);
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Start();

            listenerThread = new Thread(startListener);
            listenerThread.Start();

            Debug.Log("Initialized Listener");
        }

        public void TerminateListener()
        {
            listener.Abort();

            if (listenerThread.IsAlive)
            {
                listenerThread.Abort();
            }

            Debug.Log("Terminated Listener");
        }

        public void SendPerformanceResult(List<float> designParams, List<float> objs, bool formal)
        {
            // Parse and inject into payload
            //string payload = "{ \"design_params\": [0.00,0.00,0.00,0.00,0.25], \"objectives\" : [0.5123,0.7456], \"participant_id\":1, \"formal_eval\":1 }";
            string payload = "";
            payload += "{ \"design_params\": [";
            payload += string.Join(",", designParams);
            payload += "], \"objectives\" : [";
            payload += string.Join(",", objs);
            payload += "], \"participant_id\":" + pId_ + ", ";
            payload += "\"formal_eval\":";
            if (formal)
            {
                payload += "1";
            }
            else
            {
                payload += "0";
            }
            payload += " }";

            Debug.Log(payload);

            // Send post request
            Post("https://www.jjdudley.com/mobo/cgi/web_service.py", payload);
        }

        private void Post(string url, string postData)
        {
            UnityWebRequest www = UnityWebRequest.Post(url, postData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept-Encoding", "gzip, deflate");

            UnityWebRequestAsyncOperation response = www.SendWebRequest();
            response.completed += OnPostResponse;



        }

        private void OnPostResponse(AsyncOperation asyncOperation)
        {
            UnityWebRequestAsyncOperation operation = (UnityWebRequestAsyncOperation)asyncOperation;

            if (operation.webRequest.isNetworkError || operation.webRequest.isHttpError)
            {
                Debug.Log(operation.webRequest.error);
            }
            else
            {
                Debug.Log("Web Request Response: " + operation.webRequest.downloadHandler.text);
            }
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

            if (context.Request.HttpMethod == "POST")
            {
                var data_text = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
                HandleRequest(data_text);
            }

            context.Response.Close();
        }

        private void HandleRequest(string data)
        {
            // Debug.Log("HandleRequest: " + data);

            //"new_design_params": [0.0, 0.0, 0.0, 0.0, 0.25]
            string[] dataSplit = data.Split(':');
            string requestType = dataSplit[0].Trim(new Char[] { ' ', '{' });
            List<char> charsToRemove = new List<char>() { ' ', '[', ']', '{', '}' };

            string valuesStr = Regex.Replace(dataSplit[1], @"[ ""\[\]{}]", ""); //dataSplit[1].Filter(charsToRemove);
                                                                                //string valuesStr = dataSplit[1].Trim(new Char[] { ' ', '[', ']' });
            List<float> values = valuesStr.Split(',').Select(float.Parse).ToList();

            Debug.Log("Request type: " + requestType);
            string valuesParsed = "";
            foreach (float val in values)
            {
                valuesParsed += " " + val;
            }
            Debug.Log("Values: " + valuesParsed);

            OnNewDesignReceived?.Invoke(values);
        }

    }
}