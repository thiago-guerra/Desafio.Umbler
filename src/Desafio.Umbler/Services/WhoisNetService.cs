using Desafio.Umbler.Repository;
using DnsClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Services
{
    public static class WhoisNetService
    {
        public static async Task<Domain> GetDomain(string nameDomain)
        {
            var response = await WhoisClient.QueryAsync(nameDomain);
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(nameDomain, QueryType.ANY);
            var record = result.Answers.ARecords().FirstOrDefault();
            var address = record?.Address;
            var ip = address?.ToString();

            var hostResponse = await WhoisClient.QueryAsync(ip);

            Domain domain = new Domain
            {
                Name = nameDomain,
                Ip = ip,
                UpdatedAt = DateTime.Now,
                WhoIs = response.Raw,
                Ttl = record?.TimeToLive ?? 0,
                HostedAt = hostResponse.OrganizationName
            };

            return domain;
        }
    }
}
