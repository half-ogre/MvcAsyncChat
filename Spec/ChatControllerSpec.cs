using System;
using System.Web.Mvc;
using Xunit;
using MvcAsyncChat.Controllers;

namespace MvcAsyncChat
{
    public class ChatControllerSpec
    {
        public class The_ShowErrorForm_method
        {
            [Fact]
            void will_show_the_enter_form()
            {
                var controller = CreateController();

                var viewResult = controller.ShowEnterForm() as ViewResult;

                Assert.Equal(string.Empty, viewResult.ViewName);
            }
        }

        static ChatController CreateController()
        {
            return new ChatController();
        }
    }
}
