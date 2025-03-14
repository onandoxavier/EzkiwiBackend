using Microsoft.EntityFrameworkCore;
using VirtualQueueApi.Data;
using VirtualQueueApi.Utils.Middlewares;

namespace VirtualQueueApi.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddSignalR();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:5174")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromHours(24));
                });
            });

            services.AddAutoMapper(typeof(Program));

            #region ExceptionHandler
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            #endregion

            return services;
        }

        public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app)
        {
            app.UseCors("CorsPolicy");

            #region dbContext
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Valida a existência do banco de dados e o cria se não existir
                // Cria todas as tabelas necessarias mas nao aplica migration
                // dbContext.Database.EnsureCreated();

                // Aplica as migrations pendentes
                //dbContext.Database.Migrate();

            }
            catch (Exception ex)
            {
                // Lida com erros de conexão ou outros problemas
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Ocorreu um erro ao aplicar as migrations.");
            }
            #endregion

            #region ExceptionHandler
            app.UseExceptionHandler();
            #endregion

            return app;
        }
    }
}
