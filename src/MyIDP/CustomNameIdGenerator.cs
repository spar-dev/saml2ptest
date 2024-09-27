using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

using Microsoft.IdentityModel.Tokens.Saml2;
using Rsk.Saml;
using Rsk.Saml.Generators;


namespace IdentityServer
{

    public class CustomNameIdGenerator : SamlNameIdGenerator
    {
        private readonly ILogger<ISamlNameIdGenerator> _logger;
        public CustomNameIdGenerator(ILogger<ISamlNameIdGenerator> logger) : base(logger)
        {
            _logger = logger;

        }

        public override async Task<Saml2Subject> GenerateNameId(string subjectId, IList<Claim> userClaims, string defaultFormat, string requestedFormat)
        {
            const string customFormat = SamlConstants.NameIdentifierFormats.Transient;

            /*  if (requestedFormat == customFormat)
             {
                 var nameIdValue = userClaims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/SURID")?.Value;
                 _logger.LogInformation("nameIdValue: {nameIdValue}",nameIdValue);
                 return new Saml2Subject(new Saml2NameIdentifier(nameIdValue, new Uri(customFormat)));
             }

             return await base.GenerateNameId(subjectId, userClaims, defaultFormat, requestedFormat); */

            var responseFormat = GetResponseFormat(requestedFormat, defaultFormat);
            if (responseFormat == SamlConstants.NameIdentifierFormats.Transient)
            {
                var nameIdValue = userClaims.FirstOrDefault(x => x.Type == "SURID")?.Value;
                _logger.LogInformation("nameIdValue: {nameIdValue}", nameIdValue);
                return new Saml2Subject(new Saml2NameIdentifier(nameIdValue, new Uri(SamlConstants.NameIdentifierFormats.Transient)));
            }

            return await base.GenerateNameId(subjectId, userClaims, defaultFormat, requestedFormat);


        }
    }

}