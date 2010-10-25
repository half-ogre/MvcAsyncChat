using System;
using System.ComponentModel.DataAnnotations;

namespace MvcAsyncChat.RequestModels
{
    public class EnterRequest
    {
        [Required, StringLength(16), RegularExpression(@"^[A-Za-z0-9_\ -]+$", ErrorMessage="A name must be alpha-numeric.")]
        public string Name { get; set; }
    }
}