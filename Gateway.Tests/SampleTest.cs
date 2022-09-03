using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Gateway.Tests;

public class UnitTest1 
{
    [Fact]
    public async Task ObserveHttpMock()
    {
        // arrange
        // run real app, the gateway
        var gatewayHost = Program.GetApp(new string[0]);
        await RunApplicationHost(gatewayHost);

        // run and setup mock server
        var expectedResultString = "Yo mama, i am the request from the mock http server";
        var expectedValueFromQueryString = "fromQueryString";
        string receivedValueFromQueryString = null;
        var testHost = BuildHttpServerWithMiddleware(async (ctx, next) =>
        {
            receivedValueFromQueryString = ctx.Request.Query[RepeatController.OptionalKey];
            await ctx.Response.WriteAsync(expectedResultString);
        });
        await RunMockHost(testHost);

        // act
        var client = new HttpClient {BaseAddress = new Uri("http://localhost:5289")};
        var response = await client.GetAsync($"/Repeat?{RepeatController.OptionalKey}={expectedValueFromQueryString}");
        
        // assert
        response.EnsureSuccessStatusCode();
        var responseStringBody = await response.Content.ReadAsStringAsync();
        // ensure that gateway answered exactly like the mock middleware
        Assert.Equal(expectedResultString, responseStringBody);
        // ensure that mock server received the expected query string
        Assert.Equal(expectedValueFromQueryString, receivedValueFromQueryString);

        // teardown
        await testHost.StopAsync();
        await gatewayHost.StopAsync();
    }

    private static async Task RunMockHost(IHost? mockHost)
    {
        var mockHostIsReady = false;
        var mockHostApplicationLifeTime =
            mockHost.Services.GetService(typeof(IHostApplicationLifetime)) as IHostApplicationLifetime;
        mockHostApplicationLifeTime.ApplicationStarted.Register(() => mockHostIsReady = true);
        mockHost.RunAsync();
        await WaitForReady(mockHostIsReady);
    }

    private static async Task RunApplicationHost(WebApplication gateway)
    {
        var gatewayIsReady = false;
        gateway.Lifetime.ApplicationStarted.Register(() => gatewayIsReady = true);
        gateway.RunAsync();
        await WaitForReady(gatewayIsReady);
    }

    private static async Task WaitForReady(bool gatewayIsReady)
    {
        while (!gatewayIsReady)
        {
            await Task.Delay(1000);
        }
    }

    private IHost BuildHttpServerWithMiddleware(Func<HttpContext, RequestDelegate, Task> middleware)
    {
        var builder = Host.CreateDefaultBuilder(new string[0]);
        builder.ConfigureWebHostDefaults(bui =>
        {
            bui.UseUrls("http://localhost:3000");
            bui.Configure((ctx, app) =>
            {
                app.Use(middleware);
            });
        });
            
        var app = builder.Build();
        return app;
    }
}
