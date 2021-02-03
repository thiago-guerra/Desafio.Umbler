using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Desafio.Umbler.Models;
using Desafio.Umbler.Repository;
using Whois.NET;
using DnsClient;
using Microsoft.EntityFrameworkCore;

namespace Desafio.Umbler.Controllers
{
    [Route("api")]
    public class DomainController : Controller
    {
        private readonly DatabaseContext _db;
        private readonly ILookupClient _lookupClient;
       
        public DomainController(DatabaseContext databaseContext, ILookupClient lookupClient)
        {
            _db = databaseContext;
            _lookupClient = lookupClient;
        }

        [HttpGet, Route("domain/{domainName}")]
        public async Task<IActionResult> Get(string domainName)
        {
            DomainModel domainModel = new DomainModel();
            try
            {

                if (!string.IsNullOrEmpty(domainName) && IsValidDominio(domainName))
                {

                    Domain domainDb = await _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);

                    if (domainDb == null)
                    {
                        domainDb = await GetDomain(domainName);
                        _db.Domains.Add(domainDb);
                    }

                    if (DateTime.Now.Subtract(domainDb.UpdatedAt).TotalMinutes > domainDb.Ttl)
                    {
                        var doamainNet = await GetDomain(domainName);

                        domainDb.HostedAt = doamainNet.HostedAt;
                        domainDb.Ip = doamainNet.Ip;
                        domainDb.Name = doamainNet.Name;
                        domainDb.Ttl = doamainNet.Ttl;
                        domainDb.UpdatedAt = doamainNet.UpdatedAt;
                        domainDb.WhoIs = doamainNet.WhoIs;
                    }

                    await _db.SaveChangesAsync();
                    domainModel = ConvertDomainEfToModel(domainDb);
                    return Ok(domainModel);
                }
                else
                {
                    return BadRequest(domainModel);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        private DomainModel ConvertDomainEfToModel(Domain pDomainEf)
        {
            DomainModel domainModel = new DomainModel()
            {
                Id = pDomainEf.Id,
                Name = pDomainEf.Name,
                HostedAt = pDomainEf.HostedAt,
                Ip = pDomainEf.Ip,
                WhoIs = pDomainEf.WhoIs
            };

            return domainModel;
        }

        private bool IsValidDominio(string pDominio)
        {
            return Regex.IsMatch(pDominio, @"^((?!-))(xn--)?[a-z0-9][a-z0-9-_]{0,61}[a-z0-9]{0,1}\.(xn--)?([a-z0-9\-]{1,61}|[a-z0-9-]{1,30}\.[a-z]{2,})$");
        }

        private  async Task<Domain> GetDomain(string nameDomain)
        {
            var response = await WhoisClient.QueryAsync(nameDomain);
            var result = await _lookupClient.QueryAsync(nameDomain, QueryType.ANY);
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
