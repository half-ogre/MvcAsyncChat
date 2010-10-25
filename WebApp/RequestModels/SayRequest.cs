using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcAsyncChat.RequestModels
{
    public class SayRequest
    {
        [Required, StringLength(1024), DataType(DataType.MultilineText)]
        public string Text { get; set; }
    }
}