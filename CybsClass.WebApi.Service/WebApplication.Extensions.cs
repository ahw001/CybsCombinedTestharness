//using AspNetCoreRateLimit; // To use IClientPolicyStore and so on.
using Microsoft.AspNetCore.Http.HttpResults; // To use Results.
using Microsoft.AspNetCore.Mvc; // To use [FromServices] and so on.
using Microsoft.EntityFrameworkCore;
using CybsClass.Cybersource.Models;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Json;
using CybsClass.Cybersource.Transactions;
using CybsClass.EntityModels; // To use CybsDbContext, Product.
using CybsClass.WebApi.Service.Services.CcTransatcionProcessing;
using CybsClass.WebApi.Service.Services.DBOperations;
using CybsClass.WebApi.Service.Services.TokenProcessing;
using System.Security.Claims; // To use ClaimsPrincipal.
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CybsClass.WebApi.Service;

public static class WebApplicationExtensions
{
    // Minimal inline diagnostic page — no wwwroot/static file serving is configured for this
    // Minimal API host, so the page is served directly as a string. Lets you exercise a real
    // sandbox CyberSource checkout from the server itself, isolating server<->CyberSource
    // connectivity from any client<->server (CORS/SSL/serialization) concerns.
    private const string HomePageHtml = """
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8" />
            <title>CyberSourceServer</title>
            <style>
                body { font-family: system-ui, sans-serif; margin: 2rem; max-width: 900px; }
                button { font-size: 1rem; padding: 0.5rem 1rem; cursor: pointer; }
                pre { background: #f4f4f4; border: 1px solid #ccc; border-radius: 4px; padding: 1rem; white-space: pre-wrap; word-break: break-word; }
                #status { margin: 0.75rem 0; font-weight: 600; }
            </style>
        </head>
        <body>
            <h1>CyberSourceServer</h1>
            <p>Hello World!</p>
            <p>Click below to run a real sandbox CyberSource authorization using a hardcoded test card, exercising the same server-side path as <code>api/authtransaction</code>.</p>
            <button id="testCheckoutBtn" onclick="runTestCheckout()">Test CyberSource Checkout</button>
            <div id="status"></div>
            <pre id="output">(no request sent yet)</pre>

            <hr />

            <h2>Raw JSON Processor</h2>
            <p>Paste a raw request body and submit it directly to CyberSource — the same server path as the client's <code>/simplejson</code> page (<code>POST /api/json/processor</code>), with no DTO construction in between.</p>
            <label for="jsonResource">Resource (e.g. /pts/v2/payments)</label><br />
            <input type="text" id="jsonResource" value="/pts/v2/payments" style="width: 100%; box-sizing: border-box; margin-bottom: 0.5rem;" /><br />
            <label for="jsonMethod">HTTP Method</label><br />
            <select id="jsonMethod" style="margin-bottom: 0.5rem;">
                <option value="POST" selected>POST</option>
                <option value="GET">GET</option>
                <option value="PATCH">PATCH</option>
                <option value="DELETE">DELETE</option>
            </select><br />
            <label for="jsonValue">JSON Body</label><br />
            <textarea id="jsonValue" style="width: 100%; height: 300px; box-sizing: border-box; font-family: Consolas, monospace;" placeholder="{ &quot;clientReferenceInformation&quot;: { &quot;code&quot;: &quot;test&quot; }, ... }"></textarea><br />
            <button id="jsonProcessBtn" onclick="runJsonProcessor()" style="margin-top: 0.5rem;">Submit JSON for Processing</button>
            <div id="jsonStatus"></div>
            <pre id="jsonOutput">(no request sent yet)</pre>

            <script>
                async function runTestCheckout() {
                    const btn = document.getElementById('testCheckoutBtn');
                    const status = document.getElementById('status');
                    const output = document.getElementById('output');
                    btn.disabled = true;
                    status.textContent = 'Calling /api/test/checkout ...';
                    output.textContent = '';
                    try {
                        const res = await fetch('/api/test/checkout', { method: 'POST' });
                        const text = await res.text();
                        status.textContent = 'HTTP ' + res.status + ' ' + res.statusText;
                        try {
                            output.textContent = JSON.stringify(JSON.parse(text), null, 2);
                        } catch {
                            output.textContent = text;
                        }
                    } catch (e) {
                        status.textContent = 'Request failed';
                        output.textContent = String(e);
                    } finally {
                        btn.disabled = false;
                    }
                }

                async function runJsonProcessor() {
                    const btn = document.getElementById('jsonProcessBtn');
                    const status = document.getElementById('jsonStatus');
                    const output = document.getElementById('jsonOutput');
                    const resource = document.getElementById('jsonResource').value;
                    const httpMethod = document.getElementById('jsonMethod').value;
                    const value = document.getElementById('jsonValue').value;
                    btn.disabled = true;
                    status.textContent = 'Calling /api/json/processor ...';
                    output.textContent = '';
                    try {
                        const res = await fetch('/api/json/processor', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({
                                value: value,
                                resource: resource,
                                httpMethod: httpMethod,
                                isBoarding: false,
                                requestEncryption: 'none',
                                responseEncryption: 'none'
                            })
                        });
                        const text = await res.text();
                        status.textContent = 'HTTP ' + res.status + ' ' + res.statusText;
                        try {
                            output.textContent = JSON.stringify(JSON.parse(text), null, 2);
                        } catch {
                            output.textContent = text;
                        }
                    } catch (e) {
                        status.textContent = 'Request failed';
                        output.textContent = String(e);
                    } finally {
                        btn.disabled = false;
                    }
                }
            </script>
        </body>
        </html>
        """;

    public static void MapGets(this WebApplication app,
      int pageSize = 10)
    {
        // Combined host: Blazor's Home.razor owns "/" - the server diagnostic home page
        // moves to /server-home to avoid an ambiguous route match.
        app.MapGet("/server-home", () => Results.Content(HomePageHtml, "text/html"))
          .ExcludeFromDescription();

        app.MapGet("/api/images/{id:int}", async (int id, CybsDbContext db) =>
        {
            var entity = await db.Categories.FindAsync(id);

            if (entity == null || entity.Picture == null)
            {
                return Results.NotFound();
            }

            return Results.File(entity.Picture, "image/jpeg"); // Adjust MIME Type as needed
        });


        app.MapGet("/secret", (ClaimsPrincipal user) =>
          string.Format("Welcome, {0}. The secret ingredient is love.",
            user.Identity?.Name ?? "secure user"))
          .RequireAuthorization();

        app.MapGet("api/b2ccustomers/{id:int}", (
        [FromServices] CybsDbContext db,
        [FromRoute] int id) =>
        {
            B2cCustomer? b2Customer = db.B2cCustomers.Find(id);
            if (b2Customer == null)
            {
                return Results.NotFound();
            }
            else
            {
                return Results.Json(b2Customer);
            }
        })
        .WithName("GetCustomerById")
        .Produces<B2cCustomer>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        app.MapGet("api/randomcustomer/", (
            [FromServices] CybsDbContext db) =>
            {
                Console.WriteLine("Calling Random Customer!!!!!!!!!");
                int count = db.SampleCustomerData.Count();
                if (count == 0) return Results.NotFound();

                // SampleCustomerId is an IDENTITY column and not guaranteed to be a
                // contiguous 0-based range, so a random offset (not a random ID value)
                // is required to reliably land on a row.
                Random r = new Random();
                SampleCustomerDatum? c = db.SampleCustomerData
                    .OrderBy(x => x.SampleCustomerId)
                    .Skip(r.Next(count))
                    .FirstOrDefault();
                if (c == null) return Results.NotFound();


                return Results.Json(c);
            })
            .WithName("GetRandomCustomers")
            .Produces<SampleCustomerDatum>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("api/randomproducts/", (
            [FromServices] CybsDbContext db) =>
                {
                    int count = db.Products.Count();

                    if (count <= 3) { count = count + 3; }

                    Random random = new Random();
                    int min = 1;
                    int max = count;
                    int randomNumber = random.Next(min, max + 1); // Generates a random integer from 1 to 10, inclusive

                    IQueryable<Product> products = db.Products.OrderBy(p => p.ProductId)
                        .Skip(randomNumber).Take(3);

                    return Results.Json(products);
            })
        .WithName("GetRandomProducts")  
        .Produces<Product>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        app.MapGet("api/b2ccustomers", (
          [FromServices] CybsDbContext db,
          [FromQuery] int? page) =>
          db.B2cCustomers
            .OrderBy(cust => cust.B2cCustomerId)
            .Skip(((page ?? 1) - 1) * pageSize)
            .Take(pageSize)
          )
          .WithName("GetB2cCustomers")
          .Produces<B2cCustomer[]>(StatusCodes.Status200OK);


        app.MapGet("api/products", (
          [FromServices] CybsDbContext db,
          [FromQuery] int? page) =>
          db.Products
            .Where(p => (p.UnitsInStock > 0) && (!p.Discontinued))
            .OrderBy(product => product.ProductId)
            .Skip(((page ?? 1) - 1) * pageSize)
            .Take(pageSize)
          )
          .WithName("GetProducts")
          .Produces<Product[]>(StatusCodes.Status200OK);

        app.MapGet("api/products/outofstock",
          ([FromServices] CybsDbContext db) => db.Products
            .Where(p => (p.UnitsInStock == 0) && (!p.Discontinued))
          )
          .WithName("GetProductsOutOfStock")
          .Produces<Product[]>(StatusCodes.Status200OK);

        app.MapGet("api/products/discontinued",
          ([FromServices] CybsDbContext db) =>
            db.Products.Where(product => product.Discontinued)
          )
          .WithName("GetProductsDiscontinued")
          .Produces<Product[]>(StatusCodes.Status200OK);

        app.MapGet("api/products/{id:int}",
          async Task<Results<Ok<Product>, NotFound>> (
          [FromServices] CybsDbContext db,
          [FromRoute] int id) =>
            await db.Products.FindAsync(id) is Product product ?
              TypedResults.Ok(product) : TypedResults.NotFound())
          .WithName("GetProductById")
          .Produces<Product>(StatusCodes.Status200OK)
          .Produces(StatusCodes.Status404NotFound);


        app.MapGet("api/products/{name}", (
          [FromServices] CybsDbContext db,
          [FromRoute] string name) =>
            db.Products.Where(p => p.ProductName.Contains(name)))
          .WithName("GetProductsByName")
          .Produces<Product[]>(StatusCodes.Status200OK)
          .RequireCors(policyName: "CybsClass.Mvc.Policy");

        app.MapGet("/api/getorders/{id:int}", async ([FromRoute] int id, CybsDbContext db) =>
        {
            var result = await DbErrorHandler.GuardAsync("QueryingOrders", async () =>
                await db.Orders.Where(o => o.B2cCustomerId == id).ToListAsync());

            return result.ToOkOrError();
        })
        .WithName("QueryingOrders");


        app.MapGet("api/samplecards", async (
            [FromServices] CybsDbContext db) =>
        {
            var result = await DbErrorHandler.GuardAsync("GetSampleCards", async () =>
                await db.PaymentCardSampleData.Where(c => c.NtScenario != "SYSTEM_ERROR").ToListAsync());

            return result.ToOkOrError();
        })
        .WithName("GetSampleCards")
        .Produces<PaymentCardSampleDatum[]>(StatusCodes.Status200OK);


        app.MapGet("api/getcustomerjson", async (
            [FromServices] CybsDbContext db) =>
        {
            var result = await DbErrorHandler.GuardAsync("GetCustomerJson", async () =>
                await db.B2cCustomers.ToListAsync());

            return result.ToOkOrError();
        })
        .WithName("GetCustomerJson")
        .Produces<List<B2cCustomer>>(StatusCodes.Status200OK);

        app.MapGet("api/customercount", async () =>
        {
            return (await DBCustomerServices.GetCustomerCountAsync()).ToOkOrError();
        });

        app.MapGet("api/getcustomers", async () =>
        {
            return (await DBCustomerServices.GetB2CCustomers()).ToOkOrError();
        });

        app.MapGet("api/paymentcard/{id:int}", async ([FromRoute] int id) =>
        {
            return (await DBPaymentCardServices.GetPaymentCardInfoEntityByIdAsync(id))
                .ToOkOrNotFound($"No PaymentCardInfo found with id {id}.");
        })
        .WithName("GetPaymentCardInfoById");


    }


    public static void MapPosts(this WebApplication app)
    {
        app.MapPost("api/ntdecrypt", async ([FromBody] NtDecodeInstDto ntDeCodeInst,
            [FromServices] CallNtDecrypt callNtDecrypt) =>
        {
            string? InstId = string.Empty;

            Dictionary<string, object> dbResults = new Dictionary<string, object>();

            InstId = ntDeCodeInst.InstrumentId;

            string? decryptedNt = await callNtDecrypt.CallForNtDecrypt(InstId!);

            JsonNode jsonNtNode = JsonNode.Parse(decryptedNt)!;

            // NEED TO ENHANCE ERROR HANDLING HERE

            if (decryptedNt is null)
            {
                Console.WriteLine("NO NETWORK TOKEN NUMBER FOUND");
                await Console.Out.WriteLineAsync("-------------- DB FUNCTIONS SKIPPED!");
            }
            else if ((decryptedNt is not null) && (decryptedNt.Contains("errors", StringComparison.OrdinalIgnoreCase)))
            {

                Console.WriteLine("NO NETWORK TOKEN NUMBER FOUND");
                await Console.Out.WriteLineAsync("-------------- DB FUNCTIONS SKIPPED!");

            }
            else
            {
                //await Console.Out.WriteLineAsync("--------------- Sending to DB functions ... ");

                await Console.Out.WriteLineAsync($"*************** IN NT DECRYPT MIN API: {jsonNtNode.ToString()}");

                jsonNtNode["PaymentCardId"] = ntDeCodeInst.PaymentCardId;
                dbResults = await PersistNtData.InsertNt(jsonNtNode);
                /*
                foreach (var result in dbResults)
                {
                    await Console.Out.WriteLineAsync($"DB Results Key: " + result.Key + " " + "DB Results Value: " + result.Value.ToString());
                }
                */

            }

            return Results.Json(jsonNtNode);

        })
          .Produces<JsonNode>(StatusCodes.Status201Created);

        app.MapPost("api/followontrans", async ([FromBody] FollowOnTransDto followOnTransDto) =>
        {
            string? originalTransId = string.Empty;

            int transActionType = (int)followOnTransDto.FollowOnTransaction.GetValueOrDefault();

            FollowOnTransactions folloOnTransValue = (FollowOnTransactions)transActionType; // Cast the int to the Transaction Type enum

            Dictionary<string, object> dbResults = new Dictionary<string, object>();

            originalTransId = followOnTransDto.OriginalTransactionId;

            if (originalTransId is null && followOnTransDto is not null
                && followOnTransDto.TransactionId is not null)
            {
                originalTransId = followOnTransDto.TransactionId;
            }

            string? amount = followOnTransDto!.TransactionAmount ?? "0";

            FollowOnTransJson? followOnTransJson = new FollowOnTransJson();

            JsonNode followOnTransJsonResponse = await CallForCybsFollowOn.RunAsyncFollowOnJsonObject(originalTransId!, amount!, folloOnTransValue.ToString());

            followOnTransJson = JsonSerializer.Deserialize<FollowOnTransJson>(followOnTransJsonResponse.ToString()!);
            string status = (string)followOnTransJsonResponse!["status"]!;

            if (followOnTransJson is not null)
            {
                dbResults = await PersistFollowOnTransaction.InsertFollowOnTransaction(followOnTransJsonResponse, followOnTransJson, followOnTransDto);

            }
            return Results.Json(followOnTransJsonResponse);

        })
        .Produces<JsonNode>(StatusCodes.Status201Created);

        app.MapPost("api/authtransaction", async ([FromBody] B2cCustomerDto b2cCustomerDto,
            [FromServices] CcAuthService ccAuthService) =>
        {
            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            Console.WriteLine($"api/authtransaction INBOUND JSON: {JsonSerializer.Serialize(b2cCustomerDto, options)}");

            JsonObject jsonObject = await ccAuthService.CallForAuth(b2cCustomerDto);

            var jsonString = JsonSerializer.Serialize(jsonObject, options);
            Console.WriteLine($"api/authtransaction OUTBOUND JSON: {jsonString}");

            JsonNode jsonNode = jsonObject;

            Dictionary<string, object> dbResults = new Dictionary<string, object>();

            string? statusNode = jsonNode!["status"]?.GetValue<string>();

            if (statusNode is null)
            {
                // CyberSource returned no "status" node — either a raw error object
                // ({ "error": ..., "cybersourceJson": ... } from CallCyberSourceApiJson's
                // catch block) or an unparseable/non-JSON body. Surface it as a structured
                // ErrorObject (HTTP 2XX per the app-wide error convention) instead of
                // falling through to an opaque response the client can't interpret.
                var err = new CybsClass.Cybersource.Models.BaseData.ErrorObject
                {
                    Error = jsonNode!["error"]?.GetValue<string>() ?? "No status returned from CyberSource",
                    Message = "CyberSource did not return a parseable transaction response.",
                    CybersourceJson = jsonNode!["cybersourceJson"]?.GetValue<string>()
                };

                Console.WriteLine($"api/authtransaction OUTBOUND (error) JSON: {JsonSerializer.Serialize(err, options)}");
                return Results.Json(err);
            }

            // statusNode is guaranteed non-null here — the null case returned above.
            await Console.Out.WriteLineAsync($"**** STATUS NODE = {statusNode}");

            // NEED MORE ERROR HANDLING HERE FOR TIMEOUTS
            if (statusNode.Contains("INVALID", StringComparison.OrdinalIgnoreCase)
                || statusNode.Contains("DECLINED", StringComparison.OrdinalIgnoreCase)
                || statusNode.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
            {

                Console.WriteLine("INVALID_REQUEST");
                await Console.Out.WriteLineAsync("-------------- DB FUNCTIONS SKIPPED!");
            }
            else
            {
                await Console.Out.WriteLineAsync("--------------- Sending to DB functions ... ");
                dbResults = await PersistCustomerData.InsertCustomers(b2cCustomerDto, jsonNode);
                /*
                foreach (var result in dbResults)
                {
                    await Console.Out.WriteLineAsync($"DB Results Key: " + result.Key + " " + "DB Results Value: " + result.Value.ToString());
                }
                */
                // The payment itself already succeeded at this point, so a persistence failure
                // must not turn into a 500 — report it alongside the CyberSource response.
                // Deliberately NOT named "error": the client's JsonErrorExtractor treats an
                // "error" property as a failed transaction, which this is not.
                if (dbResults.TryGetValue(DbErrorHandler.ErrorKey, out object? persistError))
                {
                    jsonNode["dbPersistError"] = persistError?.ToString();
                    await Console.Out.WriteLineAsync($"-------------- DB PERSIST FAILED: {persistError}");
                }
                else
                {
                    if (dbResults.TryGetValue("B2cCustomerId", out object? b2cCustomerId))
                    {
                        jsonNode["B2cCustomerId"] = Convert.ToInt32(b2cCustomerId);
                    }

                    if (dbResults.TryGetValue("PaymentCardId", out object? payCardId))
                    {
                        jsonNode["PaymentCardId"] = payCardId?.ToString();
                    }

                    if (dbResults.TryGetValue("OrderId", out object? orderId))
                    {
                        jsonNode["OrderId"] = orderId?.ToString();
                    }
                }
            }

            return Results.Json(jsonNode);

        })
        .Produces<JsonNode>(StatusCodes.Status201Created);


        app.MapPost("api/processtms", async ([FromBody] B2cCustomerDto b2cCustomerDto) =>
        {
            //************ CallCyberSource for Token Create

            JsonObject jsonObject = await CallForCybsAuthTokenCreate.RunAsyncJsonObject(b2cCustomerDto);

            //************ CallCybersource for Token Create

            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            var jsonString = JsonSerializer.Serialize(jsonObject, options);

            Dictionary<string, string> dbResults = new Dictionary<string, string>();

            JsonNode jsonNode = jsonObject;

            if (jsonObject != null)
            {
                try
                {
                    int customerId = b2cCustomerDto.B2cCustomerId;
                    string statusNode = (string)jsonNode!["status"]!;
                    string id = (string)jsonNode!["id"]!;
                    await Console.Out.WriteLineAsync($"**** STATUS NODE = {id}");

                    await Console.Out.WriteLineAsync("--------------- Sending to DB functions ... ");
                    dbResults = await PersistCybsTokenData.TokenDBOps(customerId, jsonObject);
                    /*
                    foreach (var result in dbResults)
                    {
                        await Console.Out.WriteLineAsync($"DB Results Key: " + result.Key + " " + "DB Results Value: " + result.Value.ToString());
                    }
                    */

                    // The token already exists at CyberSource — report a persistence failure
                    // alongside the response rather than throwing out of the handler.
                    if (dbResults.TryGetValue(DbErrorHandler.ErrorKey, out string? persistError))
                    {
                        jsonNode["dbPersistError"] = persistError;
                    }
                    else if (dbResults.TryGetValue("PaymentCardId", out string? payCardId))
                    {
                        jsonNode["PaymentCardId"] = payCardId;
                    }

                    return Results.Json(jsonNode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION: " + ex.Message);
                    jsonNode["Exception"] = ex.Message;
                    return Results.Json(jsonNode);
                }

            }
            else if (jsonString.Contains("errors", StringComparison.OrdinalIgnoreCase))
            {

                Console.WriteLine("INVALID_REQUEST");
                await Console.Out.WriteLineAsync("-------------- DB FUNCTIONS SKIPPED!");
                jsonNode["Exception"] = jsonString;
                return Results.Json(jsonNode);
            }
            else
            {
                Console.WriteLine("ERROR IN PROCESSING");
                await Console.Out.WriteLineAsync("-------------- DB FUNCTIONS SKIPPED!");
                jsonNode["Exception"] = "UNKNOWN ERROR IN PROCESSING";
                return Results.Json(jsonNode);
            }
        })
        .Produces<JsonNode>(StatusCodes.Status201Created);

        app.MapPost("api/standalonecredit", async ([FromBody] B2cCustomerDto b2cCustomerDto) =>
        {
            JsonObject jsonObject = await CallForCybsStandAloneCredit.RunAsyncJsonObject(b2cCustomerDto);
            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var jsonString = JsonSerializer.Serialize(jsonObject, options);

            JsonNode jsonNode = jsonObject;

            Dictionary<string, object> dbResults = new Dictionary<string, object>();

            string statusNode = (string)jsonNode!["status"]!;

            if (statusNode is not null)
            {
                await Console.Out.WriteLineAsync($"**** STATUS NODE = {statusNode}");

                if (statusNode.Contains("INVALID") || statusNode.Contains("DECLINED"))
                {

                    Console.WriteLine("INVALID_REQUEST");
                    await Console.Out.WriteLineAsync("-------------- DB FUNCTIONS SKIPPED!");
                }
                else
                {
                    await Console.Out.WriteLineAsync("--------------- Sending to DB functions ... ");
                    dbResults = await PersistStandAloneCredit.InsertStandAloneCredit(b2cCustomerDto, jsonNode);
                    /*
                    foreach (var result in dbResults)
                    {
                        await Console.Out.WriteLineAsync($"DB Results Key: " + result.Key + " " + "DB Results Value: " + result.Value.ToString());
                    }
                    */
                    if (dbResults.TryGetValue(DbErrorHandler.ErrorKey, out object? persistError))
                    {
                        jsonNode["dbPersistError"] = persistError?.ToString();
                    }
                    else if (dbResults.TryGetValue("StandAloneCreditId", out object? standAloneCreditID))
                    {
                        jsonNode["StandAloneCreditId"] = Convert.ToString(standAloneCreditID);
                    }

                }
            }

            return Results.Json(jsonNode);

        })
        .Produces<JsonNode>(StatusCodes.Status201Created);

        app.MapPost("api/products", async ([FromBody] Product product,
          [FromServices] CybsDbContext db) =>
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"api/products/{product.ProductId}", product);
        })
          .Produces<Product>(StatusCodes.Status201Created);

    }

    public static void MapPuts(this WebApplication app)
    {
        app.MapPut("api/products/{id:int}", async (
          [FromRoute] int id,
          [FromBody] Product product,
          [FromServices] CybsDbContext db) =>
        {
            Product? foundProduct = await db.Products.FindAsync(id);

            if (foundProduct is null) return Results.NotFound();

            foundProduct.ProductName = product.ProductName;
            foundProduct.CategoryId = product.CategoryId;
            foundProduct.SupplierId = product.SupplierId;
            foundProduct.QuantityPerUnit = product.QuantityPerUnit;
            foundProduct.UnitsInStock = product.UnitsInStock;
            foundProduct.UnitsOnOrder = product.UnitsOnOrder;
            foundProduct.ReorderLevel = product.ReorderLevel;
            foundProduct.UnitPrice = product.UnitPrice;
            foundProduct.Discontinued = product.Discontinued;

            await db.SaveChangesAsync();

            return Results.NoContent();
        })
          .Produces(StatusCodes.Status404NotFound)
          .Produces(StatusCodes.Status204NoContent);
    }

    public static void MapDeletes(this WebApplication app)
    {
        app.MapDelete("api/products/{id:int}", async (
          [FromRoute] int id,
          [FromServices] CybsDbContext db) =>
        {
            if (await db.Products.FindAsync(id) is Product product)
            {
                db.Products.Remove(product);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }
            return Results.NotFound();
        })
          .Produces(StatusCodes.Status404NotFound)
          .Produces(StatusCodes.Status204NoContent);
    }

}
