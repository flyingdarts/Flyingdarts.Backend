using System.Net;
using Amazon.Lambda.APIGatewayEvents;
namespace Flyingdarts.Signalling.Shared;
public static class Responses
{
    private static APIGatewayProxyResponse Generate(string body, HttpStatusCode code = HttpStatusCode.Created) => new()
    {
        StatusCode = (int)code,
        Body = body
    };

    public static APIGatewayProxyResponse Created(string body) => Generate(body);
    public static APIGatewayProxyResponse InternalServerError(string body) => Generate(body, HttpStatusCode.InternalServerError);
}