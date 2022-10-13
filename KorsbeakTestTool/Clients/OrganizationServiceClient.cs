using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using Kombit.SF1500.OrganizationSystem;
using KorsbeakTestTool.Token;
using KorsbeakTestTool.Dtos;
using KorsbeakTestTool.Utils;

namespace KorsbeakTestTool.Clients
{
    public class OrganizationServiceClient
    {
        private readonly SecurityToken _securityToken;

        public OrganizationServiceClient(SecurityToken securityToken)
        {
            _securityToken = securityToken;
        }


        public IReadOnlyList<OrgUnitRegWrapper> ReadOUHierarchy(string municipalityUUID)
        {
            var registrations = new List<OrgUnitRegWrapper>();

            var channel = new CustomChannelFactory<OrganisationSystemPortType, OrganisationSystemPortTypeClient>().Create(_securityToken);

            var hasMoreData = true;
            var offset = 0;
            var chunkSize = 500; //500 is the Max allowed size

            while (hasMoreData)
            {
                try
                {
                    var request = GetOuHierarchyRequest(offset, chunkSize);

                    var response = channel.fremsoegobjekthierarki(request);

                    EnsureSuccessResponse(response);

                    var ous = response.FremsoegobjekthierarkiResponse1.FremsoegObjekthierarkiOutput.OrganisationEnheder;//OrganisationEnheder => Organizational Units

                    hasMoreData = ous.Length == chunkSize;

                    if (hasMoreData)
                        offset += chunkSize;

                    var processedRegistrations = ProcessResponse(ous, municipalityUUID);

                    registrations.AddRange(processedRegistrations);
                }
                catch (Exception ex)
                {
                    //think about error handling, while there are a lot of places with questionable behaviour
                    Console.WriteLine($"Exception: {ex}");
                    throw;
                }
            }

            return registrations;
        }

        private void EnsureSuccessResponse(fremsoegobjekthierarkiResponse response)
        {
            int statusCode = Int32.Parse(response.FremsoegobjekthierarkiResponse1.FremsoegObjekthierarkiOutput.StandardRetur.StatusKode);
            if (statusCode != 20)
            {
                //string message = StubUtil.ConstructSoapErrorMessage(statusCode, "FremsoegObjektHierarki", OrganisationSystemStubHelper.SERVICE, result.FremsoegobjekthierarkiResponse1.FremsoegObjekthierarkiOutput.StandardRetur.FejlbeskedTekst);
                //log.Error(message);
                //throw new SoapServiceException(message);
                throw new Exception($"SoapServiceException, with statusCode = {statusCode}, with message: {response.FremsoegobjekthierarkiResponse1.FremsoegObjekthierarkiOutput.StandardRetur.FejlbeskedTekst}");
            }
        }

        private fremsoegobjekthierarkiRequest GetOuHierarchyRequest(int offset, int chunkSize)
        {
            var request = new fremsoegobjekthierarkiRequest()
            {
                RequestHeader = new RequestHeaderType()
                {
                    TransactionUUID = IdUtils.GenerateUuid()
                },
                FremsoegobjekthierarkiRequest1 = new FremsoegobjekthierarkiRequestType()
                {
                    CallContext = GetCallContext(),
                    FremsoegObjekthierarkiInput = new FremsoegObjekthierarkiInputType()
                    {
                        MaksimalAntalKvantitet = chunkSize.ToString(), //maxCount
                        FoersteResultatReference = offset.ToString() //offSet
                    }
                }
            };

            return request;
        }

        private IReadOnlyList<OrgUnitRegWrapper> ProcessResponse(FiltreretOejebliksbilledeType[] ous, string municipalityUUID)
        {
            var registrations = new List<OrgUnitRegWrapper>();

            foreach (var ou in ous)
            {
                string uuid = ou.ObjektType?.UUIDIdentifikator;

                if (uuid == null)
                {
                    Console.WriteLine("OU in hierarchy does not have a uuid");
                }
                else if (ou.Registrering == null)
                {
                    Console.WriteLine("OU in hierarchy does not have a registration: " + uuid);
                }
                else
                {
                    if (ou.Registrering.Length != 1)
                    {
                        Console.WriteLine("OU in hierarchy does has more than one registration: " + uuid);
                    }

                    var reg = ou.Registrering[0];

                    if (municipalityUUID.Equals(reg.RelationListe?.Tilhoerer?.ReferenceID?.Item, StringComparison.InvariantCultureIgnoreCase))
                    {
                        registrations.Add(new OrgUnitRegWrapper()
                        {
                            Uuid = uuid,
                            Registration5 = reg
                        });
                    }
                    else
                    {
                        Console.WriteLine("Skipping OrgUnit with Tilhoerer relation unknown Organisation: " + reg.RelationListe?.Tilhoerer?.ReferenceID?.Item);
                    }
                }
            }

            return registrations;
        }


        private CallContextType GetCallContext()
        {
            return new CallContextType()
            {
                AccountingInfo = ConfigVariables.AccountingInfo,
                OnBehalfOfUser = ConfigVariables.OnBehalfOfUser,
                CallersServiceCallIdentifier = ConfigVariables.CallersServiceCallIdentifier
            };
        }
    }

}
