using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using OnaxTools.Dto.Http;

namespace Common.Services
{
    public interface IAppSessionContextRepository
    {
        Dictionary<string, string> GetCurrentContextBearerAuth();
    }
    public class AppSessionContextRepositoryCommon : IAppSessionContextRepository
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public AppSessionContextRepositoryCommon(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public virtual Dictionary<string, string> GetCurrentContextBearerAuth()
        {
            Dictionary<string, string> objResp = new();
            StringValues authValues = _contextAccessor.HttpContext.Request.Headers["Authorization"];
            if (StringValues.IsNullOrEmpty(authValues) || !authValues.Any(m => m.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase)))
            {
                return objResp;
            }
            var authHeader = authValues.FirstOrDefault(m => m != null && m.StartsWith("Bearer"));
            if (authHeader != null)
            {
                string[] authToken = !string.IsNullOrWhiteSpace(authHeader) ? authHeader.Split(" ") : new string[] {"",""};
                objResp.Add(Convert.ToString(authToken[0]), Convert.ToString(authToken[1]));
            }
            return objResp;
        }
    }
}
