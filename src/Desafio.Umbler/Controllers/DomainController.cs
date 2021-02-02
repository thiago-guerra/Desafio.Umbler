using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Whois.NET;
using Microsoft.EntityFrameworkCore;
using DnsClient;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using Desafio.Umbler.Models;
using Desafio.Umbler.Repository;
using Desafio.Umbler.Services;

namespace Desafio.Umbler.Controllers
{
    [Route("api")]
    public class DomainController : Controller
    {
        private readonly IDomainRepository _domainRepository;
        public DomainController(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        [HttpGet, Route("domain/{domainName}")]
        public async Task<IActionResult> Get(string domainName)
        {
            Models.DomainModel domainModel = new Models.DomainModel();
            try
            {

                if (!string.IsNullOrEmpty(domainName) && IsValidDominio(domainName))
                {

                    Domain domainDb = await _domainRepository.GetDomainForName(domainName);

                    if (domainDb == null)
                    {
                        domainDb = await WhoisNetService.GetDomain(domainName);
                        await _domainRepository.IncludeDomain(domainDb);
                    }

                    if (DateTime.Now.Subtract(domainDb.UpdatedAt).TotalMinutes > domainDb.Ttl)
                    {
                        var doamainNet = await WhoisNetService.GetDomain(domainName);
                        await _domainRepository.UpdateDoamin(domainDb, doamainNet);
                    }

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
    }
}
