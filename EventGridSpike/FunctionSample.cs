using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Configuration;
using System.Collections.Generic;
using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net;
using System.Text;

namespace EventGridSpike
{

    public static class FunctionSample
    {

        private static async Task sendEventGridMessageWithEventGridClientAsync(string topic, string subject, object data)
        {
             var credentials = new Microsoft.Azure.EventGrid.Models.TopicCredentials(topicKey);
           
            var client = new Microsoft.Azure.EventGrid.EventGridClient(credentials);
            
            var eventGridEvent = new Microsoft.Azure.EventGrid.Models.EventGridEvent
            {
                Subject = subject,
                EventType = "func-event",
                EventTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Data = data,
                DataVersion = "1.0.0",                
            };
            var events = new List<Microsoft.Azure.EventGrid.Models.EventGridEvent>();
            events.Add(eventGridEvent);
            await client.PublishEventsWithHttpMessagesAsync(topic, events);
        }

        public class Poco
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Function1([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage message , TraceWriter log)
        {
            var body = await message.Content.ReadAsStringAsync();
            var poco = new Poco { Id = "100", Name = "Tsuyoshi" };
            var pocoString = JsonConvert.SerializeObject(poco);
            log.Info($"The Function1 called: {pocoString}");
            await sendEventGridMessageWithEventGridClientAsync(topicHostName, "some/func1", poco);
            return message.CreateResponse(HttpStatusCode.OK, "Thanks");
        }

        [FunctionName("SomeFunc")]
        public static async Task SomeFunc([EventGridTrigger] EventGridEvent eventGridEvent, TraceWriter log)
        {
            var data = JsonConvert.SerializeObject(eventGridEvent);
            log.Info($"The SomeFunc : {data}");
        }

        [FunctionName("FilteredFuncOK")]
        public static async Task FilteredFuncOK([EventGridTrigger] EventGridEvent eventGridEvent, TraceWriter log)
        {
            var data = JsonConvert.SerializeObject(eventGridEvent);
            log.Info($"The FilteredFuncOK : {data}");
        }

        [FunctionName("FilteredFuncNO")]
        public static async Task FilteredFuncNO([EventGridTrigger] EventGridEvent eventGridEvent, TraceWriter log)
        {
            var data = JsonConvert.SerializeObject(eventGridEvent);
            log.Info($"The FilteredFuncNO : {data}");
        }

        [FunctionName("HttpHook")]
        public static async Task<HttpResponseMessage> HttpHook([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var body = await req.Content.ReadAsStringAsync();
            log.Info($"HttpHook hooked: {body}");
            return req.CreateResponse(HttpStatusCode.OK, new { greeting = "hello" });
        }
        private static string topicHostName = ConfigurationManager.AppSettings.Get("EventGrid:TopicHostName");
        private static string topicKey =      ConfigurationManager.AppSettings.Get("EventGrid:TopicKey");
    }

}
