using System;
using System.IdentityModel.Tokens;
using KorsbeakTestTool.Token;
using KorsbeakTestTool.Utils;
using Kombit.SF1500.Organization;

namespace KorsbeakTestTool.Clients
{
    public class OrganisationClient
    {
        private readonly SecurityToken _securityToken;

        public OrganisationClient(SecurityToken securityToken)
        {
            _securityToken = securityToken;
        }

        public listResponse ListOrganizations(string[] uuids)
        {
            var channel = new CustomChannelFactory<OrganisationPortType, OrganisationPortTypeClient>().Create(_securityToken);

            try
            {
                var request = GetListRequest(uuids);

                var response = channel.list(request);

                EnsureSuccessResponse(response);

                return response;

            }
            catch (Exception ex)
            {
                //think about error handling, while there are a lot of places with questionable behaviour
                Console.WriteLine($"Exception: {ex}");
                throw;
            }
        }

        private listRequest GetListRequest(params string[] uuids)
        {
            var request = new listRequest()
            {
                RequestHeader = new RequestHeaderType()
                {
                    TransactionUUID = IdUtils.GenerateUuid()
                },

                ListRequest1 = new ListRequestType()
                {
                    CallContext = GetCallContext(),
                    ListInput = new ListInputType()
                    {
                        UUIDIdentifikator = uuids
                    }
                }
            };

            return request;
        }

        private void EnsureSuccessResponse(listResponse response)
        {
            int statusCode = Int32.Parse(response.ListResponse1.ListOutput.StandardRetur.StatusKode);
            if (statusCode != 20)
            {
                //string message = StubUtil.ConstructSoapErrorMessage(statusCode, "FremsoegObjektHierarki", OrganisationSystemStubHelper.SERVICE, result.FremsoegobjekthierarkiResponse1.FremsoegObjekthierarkiOutput.StandardRetur.FejlbeskedTekst);
                //log.Error(message);
                //throw new SoapServiceException(message);
                throw new Exception($"SoapServiceException, with statusCode = {statusCode}, with message: {response.ListResponse1.ListOutput.StandardRetur.FejlbeskedTekst}");
            }
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
