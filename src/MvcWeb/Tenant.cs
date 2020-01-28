using System;

namespace MvcWeb
{
    public class Tenant
    {
        public Tenant(Guid tenantGuid)
        {

            TenantGuid = tenantGuid;
            // Id = Guid.NewGuid();           
        }
       // public int Id { get; set; }
        public Guid TenantGuid { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }
}
