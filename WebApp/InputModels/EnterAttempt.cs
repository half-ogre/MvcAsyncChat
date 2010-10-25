using System;
using System.ComponentModel.DataAnnotations;

namespace MvcAsyncChat.InputModels
{
    public class EnterAttempt
    {
        [Required, StringLength(16), RegularExpression(@"^[A-Za-z0-9_\ -]+$", ErrorMessage="A name must be alpha-numeric.")]
        public string Name { get; set; }
    }
}