using System;
using System.Collections.Generic;

namespace MvcAsyncChat.ResponseModels
{
    public class GetMessagesResponse
    {
        public string error { get; set; }
        public IEnumerable<string> messages { get; set; }
        public string since { get; set; }
    }
}