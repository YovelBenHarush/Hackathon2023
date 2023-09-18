using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseKestrel()
					.UseStartup<Startup>()
					.UseUrls("https://0.0.0.0:45678");
				//.UseUrls("https://localhost:45678"); // Set your desired server URL here

				webBuilder.ConfigureKestrel(serverOptions =>
				{
					serverOptions.ListenAnyIP(45678, listenOptions =>
					{
						listenOptions.UseHttps(httpsOptions =>
						{
							httpsOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;
							httpsOptions.AllowAnyClientCertificate();
						});
					});
				});
			});

						

							//webBuilder.ConfigureServices<KestrelServerOptions>(options =>
							//{
							//	options.ConfigureHttpsDefaults(options =>
							//		options.ClientCertificateMode = ClientCertificateMode.RequireCertificate);
							//});
				//webBuilder.ConfigureKestrel(serverOptions =>

				//{

				//	serverOptions.ListenAnyIP(45678, listenOptions =>

				//	{

				//		listenOptions.UseHttps(httpsOptions =>

				//		{

				//			var localhostCert = CertificateLoader.LoadFromStoreCert(

				//				"localhost", "My", StoreLocation.CurrentUser,

				//				allowInvalid: true);

				//			var exampleCert = CertificateLoader.LoadFromStoreCert(

				//			"10.166.113.69", "My", StoreLocation.CurrentUser,

				//				allowInvalid: true);



				//			listenOptions.UseHttps((stream, clientHelloInfo, state, cancellationToken) =>

				//			{

				//				if (string.Equals(clientHelloInfo.ServerName, "localhost",

				//					StringComparison.OrdinalIgnoreCase))

				//				{

				//					return new ValueTask<SslServerAuthenticationOptions>(

				//						new SslServerAuthenticationOptions

				//						{

				//							ServerCertificate = localhostCert,

				//							// Different TLS requirements for this host

				//							ClientCertificateRequired = true

				//						});

				//				}



				//				return new ValueTask<SslServerAuthenticationOptions>(

				//					new SslServerAuthenticationOptions

				//					{

				//						ServerCertificate = exampleCert

				//					});

				//			}, state: null!);

				//		});

				//	});

				//});
			//});
}
