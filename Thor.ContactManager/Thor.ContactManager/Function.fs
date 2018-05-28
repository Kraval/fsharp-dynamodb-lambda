namespace Thor.ContactManager

open System.Net
open System.Collections.Generic
open Amazon.Lambda.Core
open Amazon.Lambda.APIGatewayEvents
open Amazon.DynamoDBv2.DataModel
open Amazon.DynamoDBv2
open Newtonsoft.Json
open Amazon.Runtime
open Amazon
open Models
open System
open Microsoft.FSharp.Collections

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.Json.JsonSerializer>)>]
()


type Functions() =
    
    let credentials = BasicAWSCredentials("ACCESS_KEY","SECRET_KEY")
    let awsClient = new AmazonDynamoDBClient(credentials,RegionEndpoint.USEast1)   
    let config = new DynamoDBContextConfig()
    let dbContext = new DynamoDBContext(awsClient, config)
    
        
    /// <summary>
    /// A Lambda function to respond to HTTP GET methods from API Gateway
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns>The list of contacts</returns>
    member _this.Get (request: APIGatewayProxyRequest) (context: ILambdaContext) =
        context.Logger.LogLine(sprintf "GET: %s" request.Path);
        try
                let search = dbContext.ScanAsync<THContact>(null);
                let page = search.GetNextSetAsync() 
                            |> Async.AwaitTask
                            |> Async.RunSynchronously
                let res = APIGatewayProxyResponse(
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = JsonConvert.SerializeObject(page),
                            Headers = Dictionary<string, string>())
                res.Headers.Add("Content-Type", "application/json")
                res
        with
                | :? System.Exception  -> 
                        let res = APIGatewayProxyResponse(
                                    StatusCode = (int)HttpStatusCode.InternalServerError,
                                    Body = JsonConvert.SerializeObject(new Error("Error fetching Contacts")),
                                    Headers = Dictionary<string, string>())
                        res.Headers.Add("Content-Type", "application/json")
                        res
    
    /// <summary>
    /// A Lambda function to respond to HTTP POST methods from API Gateway
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns>Creates a new Contact into DynamoDB</returns>
    member _this.Post (request: APIGatewayProxyRequest) (context: ILambdaContext) =        
        context.Logger.LogLine(sprintf "Create: %s" request.Path);
        try
                let contact = JsonConvert.DeserializeObject<THContact>(request.Body)
                contact.ContactID <- Guid.NewGuid().ToString()
                let r= dbContext.SaveAsync<THContact>(contact) 
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                let res = APIGatewayProxyResponse(
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = JsonConvert.SerializeObject(contact),
                            Headers = Dictionary<string, string>())
                res.Headers.Add("Content-Type", "application/json")
                res
        with
        | :? System.Exception  -> 
                let res = APIGatewayProxyResponse(
                            StatusCode = (int)HttpStatusCode.InternalServerError,
                            Body = JsonConvert.SerializeObject(new Error("Error creating Contact")),
                            Headers = Dictionary<string, string>())
                res.Headers.Add("Content-Type", "application/json")
                res
    
    /// <summary>
    /// A Lambda function to respond to HTTP DELETE methods from API Gateway
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns>Delets requesed Contact by ID </returns>
    member _this.Delete (request: APIGatewayProxyRequest) (context: ILambdaContext) =           
            context.Logger.LogLine(sprintf "Delete: %s" request.Path);
            try
                    let cid  = 
                        match request.QueryStringParameters.TryGetValue("cid") with
                        | (true, value) -> value
                        | (false, _) -> ""
                    context.Logger.LogLine(sprintf "Cid: %s" cid)
                    let re = dbContext.DeleteAsync<THContact>(cid) 
                                |> Async.AwaitTask
                                |> Async.RunSynchronously
                      
                    context.Logger.LogLine("Delete Completd")
                    let res = APIGatewayProxyResponse(
                                StatusCode = (int)HttpStatusCode.OK,
                                Body = JsonConvert.SerializeObject(new Result("Contact successfully deleted")),
                                Headers = Dictionary<string, string>())
                    res.Headers.Add("Content-Type", "application/json")
                    res
            with
            | :? System.Exception  -> 
                let res = APIGatewayProxyResponse(
                            StatusCode = (int)HttpStatusCode.InternalServerError,
                            Body = JsonConvert.SerializeObject(new Error("Error deleting Contact")),
                            Headers = Dictionary<string, string>())
                res.Headers.Add("Content-Type", "application/json")
                res

    /// <summary>
    /// A Lambda function to respond to HTTP PUT methods from API Gateway
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns>Updates requesed Contact </returns>
    member _this.Update (request: APIGatewayProxyRequest) (context: ILambdaContext) =
            context.Logger.LogLine(sprintf "Update: %s" request.Path);
            try
                    let contact = JsonConvert.DeserializeObject<THContact>(request.Body)
                    let dbOpConfig = new DynamoDBOperationConfig()
                    dbOpConfig.IgnoreNullValues <- Nullable(true)
                    let r= dbContext.SaveAsync<THContact>(contact,dbOpConfig) 
                            |> Async.AwaitTask
                            |> Async.RunSynchronously
                      
                    context.Logger.LogLine("Update Completd")
                    let res = APIGatewayProxyResponse(
                                StatusCode = (int)HttpStatusCode.OK,
                                Body = JsonConvert.SerializeObject(new Result("Contact successfully updated")),
                                Headers = Dictionary<string, string>())
                    res.Headers.Add("Content-Type", "application/json")
                    res    
            with 
            :? System.Exception  -> 
                let res = APIGatewayProxyResponse(
                            StatusCode = (int)HttpStatusCode.InternalServerError,
                            Body = JsonConvert.SerializeObject(new Error("Error updating Contact")),
                            Headers = Dictionary<string, string>())
                res.Headers.Add("Content-Type", "application/json")
                res
   
        
            
            
