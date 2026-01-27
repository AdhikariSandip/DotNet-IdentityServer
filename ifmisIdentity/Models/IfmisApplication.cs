

namespace ifmisIdentity.Models
{
    public class MyApplication
    : OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreApplication<int, MyAuthorization, MyToken>
    {
    }
    public class MyAuthorization
        : OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreAuthorization<int, MyApplication, MyToken>
    {
    }
    public class MyToken
        : OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreToken<int, MyApplication, MyAuthorization>
    {
    }
    public class MyScope
        : OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreScope<int>
    {
    }

}
