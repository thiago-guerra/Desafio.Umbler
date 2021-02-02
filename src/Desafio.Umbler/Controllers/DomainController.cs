using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Desafio.Umbler.Models;
using Whois.NET;
using Microsoft.EntityFrameworkCore;
using DnsClient;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Desafio.Umbler.Controllers
{
    [Route("api")]
    public class DomainController : Controller
    {
        private readonly DatabaseContext _db;

        public DomainController(DatabaseContext db)
        {
            _db = db;
        }

        [HttpGet, Route("domain/{domainName}")]
        public async Task<IActionResult> Get(string domainName)
        {
            DomainModel domainModel = new DomainModel();
            try
            {

                if (!string.IsNullOrEmpty(domainName) && ValidaDominio(domainName))
                {

                    Domain domain = await _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);

                    if (domain == null)
                    {
                        var response = await WhoisClient.QueryAsync(domainName);

                        var lookup = new LookupClient();
                        var result = await lookup.QueryAsync(domainName, QueryType.ANY);
                        var record = result.Answers.ARecords().FirstOrDefault();
                        var address = record?.Address;
                        var ip = address?.ToString();

                        var hostResponse = await WhoisClient.QueryAsync(ip);

                        domain = new Domain
                        {
                            Name = domainName,
                            Ip = ip,
                            UpdatedAt = DateTime.Now,
                            WhoIs = response.Raw,
                            Ttl = record?.TimeToLive ?? 0,
                            HostedAt = hostResponse.OrganizationName
                        };

                        _db.Domains.Add(domain);
                    }

                    if (DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl)
                    {
                        var response = await WhoisClient.QueryAsync(domainName);
                        var lookup = new LookupClient();
                        var result = await lookup.QueryAsync(domainName, QueryType.ANY);
                        var record = result.Answers.ARecords().FirstOrDefault();
                        var address = record?.Address;
                        var ip = address?.ToString();

                        var hostResponse = await WhoisClient.QueryAsync(ip);

                        domain.Name = domainName;
                        domain.Ip = ip;
                        domain.UpdatedAt = DateTime.Now;
                        domain.WhoIs = response.Raw;
                        domain.Ttl = record?.TimeToLive ?? 0;
                        domain.HostedAt = hostResponse.OrganizationName;
                    }

                    await _db.SaveChangesAsync();

                    domainModel = ConverterDomainEfToModel(domain);
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

        private DomainModel ConverterDomainEfToModel(Domain pDomainEf)
        {
            DomainModel domainModel = new DomainModel()
            {
                Name = pDomainEf.Name,
                HostedAt = pDomainEf.HostedAt,
                Ip = pDomainEf.Ip,
                WhoIs = pDomainEf.WhoIs
            };

            return domainModel;
        }

        private bool ValidaDominio(string pDominio)
        {
            return Regex.IsMatch(pDominio, @"^((?!-))(xn--)?[a-z0-9][a-z0-9-_]{0,61}[a-z0-9]{0,1}\.(xn--)?([a-z0-9\-]{1,61}|[a-z0-9-]{1,30}\.[a-z]{2,})$");
        }
    }
}
