using Funca.Abstractions.Data;

namespace Funca.Abstractions.Shell;

public static class RequestContextModule
{
    extension(RequestContext @this)
    {
        public void SetTenant(TenantId tenantId)
            => @this.Attach("tenantId", tenantId.Id);

        public Option<TenantId> GetTenant()
        {
            return @this
                .Detach<string>("tenantId")
                .Match(
                    s => Option<TenantId>.Some(new TenantId(s)),
                    Option.None<TenantId>);
        }
    }
}