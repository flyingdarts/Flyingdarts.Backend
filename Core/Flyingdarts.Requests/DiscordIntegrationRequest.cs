using System.Text;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Flyingdarts.Requests;

public class DiscordIntegrationRequest
{
    [JsonPropertyName("account_id")]
    public string AccountId { get; set; }
    [JsonPropertyName("rawBody")]
    public byte[] RawBody { get; set; }
    [JsonPropertyName("api-id")]
    public string ApiId { get; set; }
    [JsonPropertyName("api-key")] 
    public string ApiKey { get; set; }
    [JsonPropertyName("authorizer-principal-id")] 
    public string AuthorizerPrincipalId { get; set; }
    [JsonPropertyName("caller")] 
    public string Caller { get; set; }
    [JsonPropertyName("cognito-authentication-provider")]
    public string CognitoAuthenticationProvider { get; set; }
    [JsonPropertyName("cognito-authentication-type")]
    public string CognitoAuthenticationType { get; set; }
    [JsonPropertyName("cognito-identity-id")]
    public string CognitoIdentityId { get; set; }
    [JsonPropertyName("cognito-identity-pool-id")]
    public string CognitoIdentityPoolId { get; set; }
    [JsonPropertyName("http-method")]
    public string HttpMethod { get; set; }
    [JsonPropertyName("stage")]
    public string Stage { get; set; }
    [JsonPropertyName("source-ip")] 
    public string SourceIp { get; set; }
    [JsonPropertyName("user")] 
    public string User { get; set; }
    [JsonPropertyName("user-agent")] 
    public string UserAgent { get; set; }
    [JsonPropertyName("user-arn")] 
    public string UserArn { get; set; }
    [JsonPropertyName("request-id")] 
    public string RequestId { get; set; }
    [JsonPropertyName("resource-id")]
    public string ResourceId { get; set; }
    [JsonPropertyName("resource-path")]
    public string ResourcePath { get; set; }

    public static DiscordIntegrationRequest FromApiGatewayProxyRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        return new DiscordIntegrationRequest
        {
            RawBody = Encoding.UTF8.GetBytes(request.Body),
            AccountId = request.RequestContext.AccountId,
            AuthorizerPrincipalId = null, // Lambda Authorizer?
            ApiId = request.RequestContext.ApiId,
            ApiKey = request.RequestContext.Identity.ApiKey,
            Caller = request.RequestContext.Identity.Caller,
            CognitoAuthenticationProvider = request.RequestContext.Identity.CognitoAuthenticationProvider,
            CognitoAuthenticationType = request.RequestContext.Identity.CognitoAuthenticationType,
            CognitoIdentityId = request.RequestContext.Identity.CognitoIdentityId,
            CognitoIdentityPoolId = request.RequestContext.Identity.CognitoIdentityPoolId,
            HttpMethod = request.RequestContext.HttpMethod,
            Stage = request.RequestContext.Stage,
            SourceIp = request.RequestContext.Identity.SourceIp,
            User = request.RequestContext.Identity.User,
            UserAgent = request.RequestContext.Identity.UserAgent,
            UserArn = request.RequestContext.Identity.UserArn,
            RequestId = request.RequestContext.RequestId,
            ResourceId = request.RequestContext.ResourceId,
            ResourcePath = request.RequestContext.ResourcePath
        };
    }
}

