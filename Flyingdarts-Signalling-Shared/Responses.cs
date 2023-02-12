using System.Net;
using Amazon.Lambda.APIGatewayEvents;
namespace Flyingdarts.Signalling.Shared;
public static class Responses
{
    private static APIGatewayProxyResponse Generate(HttpStatusCode code, string body) => new()
    {
        StatusCode = (int)code,
        Body = body
    };

    public static APIGatewayProxyResponse Created(string body) => Generate(HttpStatusCode.Created, body);
    public static APIGatewayProxyResponse InternalServerError(string body) => Generate(HttpStatusCode.InternalServerError, body);
}