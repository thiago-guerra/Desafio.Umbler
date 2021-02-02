using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Desafio.Umbler.Repository
{
    public interface IDomainRepository
    {
        public Task<Domain> GetDomainForName(string domainName);
        public Task<int> IncludeDomain(Domain domain);
        public Task<int> UpdateDoamin(Domain domainDb, Domain domainNew);
    }
}
