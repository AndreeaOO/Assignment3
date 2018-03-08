using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DomainModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace XUnitTest
{
    public class UnitTest1
    {

        internal const int Port = 5000;

 

        //////////////////////////////////////////////////////////
        /// 
        /// Testing Constrains
        /// 
        ////////////////////////////////////////////////////////// 

        [Fact]
        public void Constraint_ConnectionWithoutRequest_ShouldConnect()
        {
            var client = Helper.Connect();
            Assert.True(client.Connected);
        }

        /*    Method Tests     */

        [Fact]
        public void Constraint_RequestWithoutMethod_MissingMethodError()
        {
            
            var client = Helper.Connect();

            client.SendRequest("{}");

            var response = client.ReadResponse();

            Assert.True(response.Status.ToLower().Contains("missing method"));
        }

        [Fact]
        public void Constraint_RequestWithUnknownMethod_IllegalMethodError()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "xxxx",
                Path = "testing",
                Date = Helper.UnixTimestamp(),
                Body = "{}"
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            Assert.Contains("illegal method", response.Status.ToLower());
        }

        [Theory]
        [InlineData("create")]
        [InlineData("read")]
        [InlineData("update")]
        [InlineData("delete")]
        public void Constraint_RequestForCreateReadUpdateDeleteWithoutResource_MissingResourceError(string method)
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = method,
                Date = DateTimeOffset.Now.ToUnixTimeSeconds().ToString()
            };

            client.SendRequest(request.ToJson());

            var response = client.ReadResponse();

            Assert.Contains("missing resource", response.Status.ToLower());
        }

        /* Date Tests    */

        [Fact]
        public void Constraint_RequestWithoutDate_MissingDateError()
        {
            var client = Helper.Connect();

            client.SendRequest("{}");

            var response = client.ReadResponse();

            Assert.True(response.Status.ToLower().Contains("missing date"));
        }

        [Fact]
        public void Constraint_RequestWhereDateIsNotUnixTime_IllegalDateError()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "update",
                Path = "testing",
                Date = DateTimeOffset.Now.ToString(),
                Body = (new { cid = 1, Name = "Beverages" }).ToJson()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            Assert.Contains("illegal date", response.Status.ToLower());
        }

        /* Body Tests    */

        [Theory]
        [InlineData("create")]
        [InlineData("update")]
        [InlineData("echo")]
        public void Constraint_RequestForCreateUpdateEchoWithoutBody_MissingBodyError(string method)
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = method,
                Path = "testing",
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            Assert.Contains("missing body", response.Status.ToLower());
        }


        [Fact]
        public void Constraint_RequestUpdateWithoutJsonBody_IllegalBodyError()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "update",
                Path = "/api/categories/1",
                Date = Helper.UnixTimestamp(),
                Body = "Hello World"
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();


            Assert.Contains("illegal body", response.Status.ToLower());

        }

        /* Echo Test */
        [Fact]
        public void Echo_RequestWithBody_ReturnsBody()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "echo",
                Date = Helper.UnixTimestamp(),
                Body = "Hello World"
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            Assert.Equal("Hello World", response.Body);

        }

        //////////////////////////////////////////////////////////
        /// 
        /// Testing API 
        /// 
        ////////////////////////////////////////////////////////// 

        /* Path tests  */

        [Fact]
        public void Constraint_RequestWithInvalidpath_StatusBadRequest()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "read",
                Path = "/api/xxx",
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            var expectedResponse = new Response { Status = "4 Bad Request" };

            Assert.Equal(expectedResponse.ToJson().ToLower(), response.ToJson().ToLower());
        }

        [Fact]
        public void Constraint_RequestWithInvalidpathId_StatusBadRequest()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "read",
                Path = "/api/categories/xxx",
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            var expectedResponse = new Response { Status = "4 Bad Request" };

            Assert.Equal(expectedResponse.ToJson().ToLower(), response.ToJson().ToLower());
        }

        [Fact]
        public void Constraint_CreateWithPathId_StatusBadRequest()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "create",
                Path = "/api/categories/1",
                Date = Helper.UnixTimestamp(),
                Body = (new { Name = "" }).ToJson()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            var expectedResponse = new Response { Status = "4 Bad Request" };

            Assert.Equal(expectedResponse.ToJson().ToLower(), response.ToJson().ToLower());
        }

        [Fact]
        public void Constraint_UpdateWithOutPathId_StatusBadRequest()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "update",
                Path = "/api/categories",
                Date = Helper.UnixTimestamp(),
                Body = (new { Name = "" }).ToJson()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            var expectedResponse = new Response { Status = "4 Bad Request" };

            Assert.Equal(expectedResponse.ToJson().ToLower(), response.ToJson().ToLower());
        }

        [Fact]
        public void Constraint_DeleteWithOutPathId_StatusBadRequest()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "delete",
                Path = "/api/categories",
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            var expectedResponse = new Response { Status = "4 Bad Request" };

            Assert.Equal(expectedResponse.ToJson().ToLower(), response.ToJson().ToLower());
        }



        /* Read tests */

        [Fact]
        public void Request_ReadCategories_StatusOkAndListOfCategoriesInBody()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "read",
                Path = "/api/categories",
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            var categories = new List<object>
            {
                new {cid = 1, name = "Beverages"},
                new {cid = 2, name = "Condiments"},
                new {cid = 3, name = "Confections"}
            };

            var expectedResponse = new
            {
                Status = "1 Ok",
                Body = categories.ToJson()
            };

            Assert.Equal(expectedResponse.ToJson(), response.ToJson());
        }

        [Fact]
        public void Request_ReadCategoryWithValidId_StatusOkAndCategoryInBody()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "read",
                Path = "/api/categories/1",
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            var expectedResponse = new Response
            {
                Status = "1 Ok",
                Body = (new { cid = 1, name = "Beverages" }.ToJson())
            };

            Assert.Equal(expectedResponse.ToJson(), response.ToJson());
        }

        [Fact]
        public void Request_ReadCategoryWithInvalidId_StatusNotFound()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "read",
                Path = "/api/categories/123",
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            Assert.Contains("5 not found", response.Status.ToLower());
        }


        /* Update tests  */

        [Fact]
        public void Request_UpdateCategoryWithValidIdAndBody_StatusUpdated()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "update",
                Path = "/api/categories/1",
                Date = Helper.UnixTimestamp(),
                Body = (new { cid = 1, name = "BeveragesTesting" }).ToJson()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();


            Assert.Contains("3 updated", response.Status.ToLower());

            // reset data

            client = Helper.Connect();

            var resetRequest = new
            {
                Method = "update",
                Path = "/api/categories/1",
                Date = Helper.UnixTimestamp(),
                Body = (new { cid = 1, name = "Beverages" }).ToJson()
            };

            client.SendRequest(resetRequest.ToJson());
            client.ReadResponse();
        }

        [Fact]
        public void Request_UpdateCategotyValidIdAndBody_ChangedCategoryName()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "update",
                Path = "/api/categories/1",
                Date = Helper.UnixTimestamp(),
                Body = (new { cid = 1, name = "BeveragesTesting" }).ToJson()
            };

            client.SendRequest(request.ToJson());
            client.ReadResponse();

            client = Helper.Connect();
            var readRequest = new
            {
                Method = "read",
                Path = "/api/categories/1",
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(readRequest.ToJson());
            var response = client.ReadResponse();

            Assert.Equal("BeveragesTesting", response.Body.FromJson<Category>().Name);

            // reset data

            client = Helper.Connect();

            var resetRequest = new
            {
                Method = "update",
                Path = "/api/categories/1",
                Date = Helper.UnixTimestamp(),
                Body = (new { cid = 1, name = "Beverages" }).ToJson()
            };

            client.SendRequest(resetRequest.ToJson());
            client.ReadResponse();
        }

        [Fact]
        public void Request_UpdateCategotyInvalidId_NotFound()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "update",
                Path = "/api/categories/123",
                Date = Helper.UnixTimestamp(),
                Body = (new { cid = 1, name = "BeveragesTesting" }).ToJson()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            Assert.Contains("5 not found", response.Status.ToLower());
        }


        /* Create Tests  */

        [Fact]
        public void Request_CreateCategoryWithValidBodyArgument_CreateNewCategory()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "create",
                Path = "/api/categories",
                Date = Helper.UnixTimestamp(),
                Body = (new { name = "Testing" }).ToJson()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            var category = response.Body.FromJson<Category>();

            Assert.Contains("Testing", category.Name);
            Assert.True(category.Id > 0);

            // reset

            client = Helper.Connect();
            var resetRequest = new
            {
                Method = "delete",
                Path = "/api/categories/" + category.Id,
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(resetRequest.ToJson());
            client.ReadResponse();
        }


        /* Delete Tests  */

        [Fact]
        public void Request_DeleteCategoryWithValidId_RemoveCategory()
        {
            var client = Helper.Connect();

            var request = new
            {
                Method = "create",
                Path = "/api/categories",
                Date = Helper.UnixTimestamp(),
                Body = (new { name = "TestingDeleteCategory" }).ToJson()
            };

            client.SendRequest(request.ToJson());
            var response = client.ReadResponse();

            client = Helper.Connect();
            var verifyRequest = new
            {
                Method = "delete",
                Path = "/api/categories/" + response.Body.FromJson<Category>().Id,
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(verifyRequest.ToJson());
            response = client.ReadResponse();

            Assert.Contains("1 ok", response.Status.ToLower());
        }

        [Fact]
        public void Request_DeleteCategoryWithInvalidId_StatusNotFound()
        {
            var client = Helper.Connect();
            var verifyRequest = new
            {
                Method = "delete",
                Path = "/api/categories/1234",
                Date = Helper.UnixTimestamp()
            };

            client.SendRequest(verifyRequest.ToJson());
            var response = client.ReadResponse();

            Assert.Contains("5 not found", response.Status.ToLower());
        }






    }
}
