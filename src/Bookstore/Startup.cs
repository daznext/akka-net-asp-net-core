using Akka.Actor;
using Bookstore.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bookstore {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.AddRazorPages ();

            // Register ActorSystem
            services.AddSingleton (_ => ActorSystem.Create ("bookstore", ConfigurationLoader.Load ()));

            services.AddSingleton<BooksManagerActorProvider> (provider => {
                var actorSystem = provider.GetService<ActorSystem> ();
                var booksManagerActor = actorSystem.ActorOf (Props.Create (() => new BooksManagerActor ()));
                return () => booksManagerActor;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env,IHostApplicationLifetime lifetime) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                app.UseHsts ();
            }

            app.UseHttpsRedirection ();
            app.UseRouting ();
            app.UseEndpoints (endpints => { endpints.MapRazorPages (); });

            lifetime.ApplicationStarted.Register (() => {
                app.ApplicationServices.GetService<ActorSystem> (); // start Akka.NET
            });
            lifetime.ApplicationStopping.Register (() => {
                app.ApplicationServices.GetService<ActorSystem> ().Terminate ().Wait ();
            });
        }
    }
}