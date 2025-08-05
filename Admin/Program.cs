namespace Admin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controller ve View desteði
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // HTTPS yönlendirme
            app.UseHttpsRedirection();

            // Statik dosyalar (wwwroot için)
            app.UseStaticFiles();

            // Routing ve Authorization middleware
            app.UseRouting();
            app.UseAuthorization();

            // Endpoint tanýmlarý
            app.UseEndpoints(endpoints =>
            {
                // Razor Pages kullanýyorsan bunu:
                // endpoints.MapRazorPages();

                // MVC Controller + View kullanýyorsan bunu:
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");

                // Ana sayfaya gelenleri login sayfasýna yönlendir
                endpoints.MapGet("/", context =>
                {
                    context.Response.Redirect("/Account/Login");
                    return Task.CompletedTask;
                });
            });

            app.Run();
        }
    }
}
