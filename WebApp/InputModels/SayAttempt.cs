using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcAsyncChat.InputModels
{
    public class SayAttempt
    {
        [Required, StringLength(1024), DataType(DataType.MultilineText)]
        public string Text { get; set; }
    }
}