using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace tests.Api;

[TestClass]
public class MinimalApiTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public MinimalApiTests()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [TestMethod]
    public async Task CreateAccount_returns_created_account()
    {
        var client = _factory.CreateClient();
        var request = new
        {
            name = "Cash",
            direction = "debit"
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/accounts");
        httpRequest.Content = JsonContent.Create(request);
        httpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await client.SendAsync(httpRequest);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.IsNotNull(body);
        Assert.AreEqual("Cash", body!.Name);
        Assert.AreEqual("debit", body.Direction);
        Assert.AreEqual(0m, body.Balance);
    }

    [TestMethod]
    public async Task CreateTransaction_with_invalid_direction_returns_bad_request()
    {
        var client = _factory.CreateClient();
        var accountRequest = new { name = "Cash", direction = "debit" };
        var accountHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/accounts");
        accountHttpRequest.Content = JsonContent.Create(accountRequest);
        accountHttpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var accountResponse = await client.SendAsync(accountHttpRequest);
        var account = await accountResponse.Content.ReadFromJsonAsync<AccountResponse>();

        var transactionRequest = new
        {
            name = "Bad transaction",
            entries = new[]
            {
                new { direction = "invalid", account_id = account!.Id, amount = 100m }
            }
        };

        var transactionHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/transactions");
        transactionHttpRequest.Content = JsonContent.Create(transactionRequest);
        transactionHttpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var response = await client.SendAsync(transactionHttpRequest);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task CreateTransaction_with_unbalanced_entries_returns_bad_request()
    {
        var client = _factory.CreateClient();
        var account1HttpRequest = new HttpRequestMessage(HttpMethod.Post, "/accounts");
        account1HttpRequest.Content = JsonContent.Create(new { name = "A", direction = "debit" });
        account1HttpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var account1 = await client.SendAsync(account1HttpRequest);

        var account2HttpRequest = new HttpRequestMessage(HttpMethod.Post, "/accounts");
        account2HttpRequest.Content = JsonContent.Create(new { name = "B", direction = "credit" });
        account2HttpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var account2 = await client.SendAsync(account2HttpRequest);

        var account1Body = await account1.Content.ReadFromJsonAsync<AccountResponse>();
        var account2Body = await account2.Content.ReadFromJsonAsync<AccountResponse>();

        var transactionRequest = new
        {
            name = "Unbalanced",
            entries = new[]
            {
                new { direction = "debit", account_id = account1Body!.Id, amount = 100m },
                new { direction = "credit", account_id = account2Body!.Id, amount = 90m }
            }
        };

        var transactionHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/transactions");
        transactionHttpRequest.Content = JsonContent.Create(transactionRequest);
        transactionHttpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var response = await client.SendAsync(transactionHttpRequest);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private sealed record AccountResponse(Guid Id, string Name, string Direction, decimal Balance);
}
