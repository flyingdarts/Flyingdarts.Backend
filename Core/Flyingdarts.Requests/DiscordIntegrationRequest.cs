using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Flyingdarts.Requests;

public class DiscordIntegrationRequest
{
    public string AccountId { get; set; }
    public byte[] RawBody { get; set; }
    public string ApiId { get; set; }
    public string ApiKey { get; set; }
    public string AuthorizerPrincipalId { get; set; }
    public string Caller { get; set; }
    public string CognitoAuthenticationProvider { get; set; }
    public string CognitoAuthenticationType { get; set; }
    public string CognitoIdentityId { get; set; }
    public string CognitoIdentityPoolId { get; set; }
    public string HttpMethod { get; set; }
    public string Stage { get; set; }
    public string SourceIp { get; set; }
    public string User { get; set; }
    public string UserAgent { get; set; }
    public string UserArn { get; set; }
    public string RequestId { get; set; }
    public string ResourceId { get; set; }
    public string ResourcePath { get; set; }

    public static DiscordIntegrationRequest FromApiGatewayProxyRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        return new DiscordIntegrationRequest
        {
            RawBody = Encoding.UTF8.GetBytes(request.Body),
            AccountId = request.RequestContext.AccountId,
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
        }
    }
}

