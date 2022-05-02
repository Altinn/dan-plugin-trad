using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Altinn.Dan.Plugin.Trad.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Caching.Distributed;
using Nadobe;
using Nadobe.Common.Interfaces;
using Nadobe.Common.Models;
using Nadobe.Common.Util;
using Newtonsoft.Json;

namespace Altinn.Dan.Plugin.Trad
{
    public class Main
    {
        private readonly IEvidenceSourceMetadata _metadata;
        private readonly IDistributedCache _cache;

        public Main(IDistributedCache cache, IEvidenceSourceMetadata metadata)
        {
            _metadata = metadata;
            _cache = cache;
        }

        [Function("VerifiserAdvokat")]
        public async Task<HttpResponseData> RunAsyncVerifiserAdvokat([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req, FunctionContext context)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            var response = req.CreateResponse(HttpStatusCode.OK);
            var actionResult = await EvidenceSourceResponse.CreateResponse(null, () => GetEvidenceValuesVerifiserAdvokat(evidenceHarvesterRequest)) as ObjectResult;

            await response.WriteAsJsonAsync(actionResult?.Value);

            return response;
        }

        [Function("HentAdvokatRegisterPerson")]
        public async Task<HttpResponseData> RunAsyncHentPerson([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req, FunctionContext context)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            var response = req.CreateResponse(HttpStatusCode.OK);
            var actionResult = await EvidenceSourceResponse.CreateResponse(null, () => GetEvidenceValuesHentAdvokatRegisterPerson(evidenceHarvesterRequest)) as ObjectResult;

            await response.WriteAsJsonAsync(actionResult?.Value);

            return response;
        }


        [Function(Constants.EvidenceSourceMetadataFunctionName)]
        public async Task<HttpResponseData> RunAsyncMetadata(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(_metadata.GetEvidenceCodes());
            return response;
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesVerifiserAdvokat(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            var res = await _cache.GetAsync(Helpers.GetCacheKeyForSsn(evidenceHarvesterRequest.SubjectParty.NorwegianSocialSecurityNumber));

            var ecb = new EvidenceBuilder(_metadata, "VerifiserAdvokat");
            ecb.AddEvidenceValue("Fodselsnummer", evidenceHarvesterRequest.SubjectParty.NorwegianSocialSecurityNumber, EvidenceSourceMetadata.Source);
            if (res != null)
            {
                var person = JsonConvert.DeserializeObject<Person>(Encoding.UTF8.GetString(res));

                ecb.AddEvidenceValue("ErRegistrert", true, EvidenceSourceMetadata.Source);
                ecb.AddEvidenceValue("Tittel", person.Title, EvidenceSourceMetadata.Source);
            }
            else
            {
                ecb.AddEvidenceValue("ErRegistrert", false, EvidenceSourceMetadata.Source);
            }

            return ecb.GetEvidenceValues();
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesHentAdvokatRegisterPerson(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            var res = await _cache.GetAsync(Helpers.GetCacheKeyForSsn(evidenceHarvesterRequest.SubjectParty.NorwegianSocialSecurityNumber));

            var ecb = new EvidenceBuilder(_metadata, "HentAdvokatRegisterPerson");

            ecb.AddEvidenceValue("default", res != null ? Encoding.UTF8.GetString(res) : "{}",
                EvidenceSourceMetadata.Source);

            return ecb.GetEvidenceValues();
        }
    }
}
