using Microsoft.AspNetCore.Http;

namespace QLSV_V1.Middlewares
{
    public class JwtUserMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Nếu user đã đăng nhập và có JWT token
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var accId = context.User.FindFirst("accId")?.Value;
                var role = context.User.FindFirst("role")?.Value;
                var teacherId = context.User.FindFirst("teacherId")?.Value;
                var advisorId = context.User.FindFirst("advisorId")?.Value;

                // Lưu vào HttpContext.Items để API có thể lấy
                context.Items["accId"] = accId;
                context.Items["role"] = role;
                context.Items["teacherId"] = teacherId;
                context.Items["advisorId"] = advisorId;
            }

            await _next(context);
        }
    }
}
