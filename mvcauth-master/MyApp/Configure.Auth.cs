using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.FluentValidation;

[assembly: HostingStartup(typeof(MyApp.ConfigureAuth))]

namespace MyApp
{
    // Add any additional metadata properties you want to store in the Users Typed Session
    public class CustomUserSession : AuthUserSession
    {
    }
    
    // Custom Authorization Provider
    public class customN : BasicAuthProvider
    {

    }
// Custom Validator to add custom validators to built-in /register Service requiring DisplayName and ConfirmPassword
    public class CustomRegistrationValidator : RegistrationValidator
    {
        public CustomRegistrationValidator()
        {   
            
            RuleSet(ApplyTo.Post, () =>
            {
                RuleFor(x => x.UserName).Matches(@"\b[N]\w+");
                RuleFor(x => x.ConfirmPassword).NotEmpty();
            });
        }
    }

    public class ConfigureAuth : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder) => builder
            .ConfigureServices(services => {
                //services.AddSingleton<ICacheClient>(new MemoryCacheClient()); //Store User Sessions in Memory Cache (default)
            })
            .ConfigureAppHost(appHost => {
                var appSettings = appHost.AppSettings;
                appHost.Plugins.Add(new AuthFeature(() => new CustomUserSession(),
                    new IAuthProvider[] {
                        //new BasicAuthProvider(appSettings),
                        new JwtAuthProvider(){
                            AuthKey = AesUtils.CreateKey()

                        },
                        //new BasicAuthProvider(appSettings){},
                        /* Sign In with Username / Password credentials */
                        new CredentialsAuthProvider(appSettings),
 

                    })) ;
                

                appHost.Plugins.Add(new RegistrationFeature()); //Enable /register Service

                //override the default registration validation with your own custom implementation
                appHost.RegisterAs<CustomRegistrationValidator, IValidator<Register>>();
            });
    }
}
