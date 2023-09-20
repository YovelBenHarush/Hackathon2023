using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

	public static IHostBuilder CreateHostBuilder(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseStartup<Startup>().UseKestrel();
				//.UseUrls("https://127.0.0.1:45678");

				webBuilder.ConfigureKestrel(serverOptions =>
				{
					//serverOptions.Listen(IPAddress.Parse("10.166.113.69"), 443);
					serverOptions.Listen(IPAddress.Loopback, 443, listenOptions => //IPAddress.Parse("10.166.113.69")
					{
						//listenOptions. UseHttps();
						listenOptions.UseHttps(httpsOptions =>
						{
							httpsOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;// .NoCertificate;
																										//httpsOptions.AllowAnyClientCertificate();
							httpsOptions.CheckCertificateRevocation = false;
							httpsOptions.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
						});
					});
				});

				webBuilder.ConfigureServices(services =>
				{
					services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
					{
						builder.AllowAnyOrigin()
							   .AllowAnyMethod()
							   .AllowAnyHeader();
					}));
				});
			});

		builder.ConfigureServices(services =>
		{
			services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
			{
				builder.AllowAnyOrigin()
					   .AllowAnyMethod()
					   .AllowAnyHeader();
			}));
		});

		return builder;
	}
}
