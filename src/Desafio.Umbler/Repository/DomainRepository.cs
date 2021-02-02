using Desafio.Umbler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Desafio.Umbler.Repository
{
    public class DomainRepository : IDomainRepository
    {
        private readonly DatabaseContext _db;

        public DomainRepository(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<Domain> GetDomainForName(string domainName)
        {
            return await _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);
        }

        public async Task<int> IncludeDomain(Domain domain)
        {
            _db.Entry(domain).State = EntityState.Added;
            _db.Add(domain);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }

        public async Task<int> UpdateDoamin(Domain domainDb, Domain domainNew)
        {
            domainDb.HostedAt = domainNew.HostedAt;
            domainDb.Ip = domainNew.Ip;
            domainDb.Name = domainNew.Name;
            domainDb.Ttl = domainNew.Ttl;
            domainDb.UpdatedAt = domainNew.UpdatedAt;
            domainDb.WhoIs = domainNew.WhoIs;

            _db.Entry(domainDb).State = EntityState.Modified;
            return await _db.SaveChangesAsync();

        }
    }
}
