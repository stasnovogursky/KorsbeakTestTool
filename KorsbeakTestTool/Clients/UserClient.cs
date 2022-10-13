using System;
using System.IdentityModel.Tokens;
using Kombit.SF1500.User;
using KorsbeakTestTool.Token;
using KorsbeakTestTool.Utils;

namespace KorsbeakTestTool.Clients
{
    public class UserClient
    {
        private readonly SecurityToken _securityToken;

        public UserClient(SecurityToken securityToken)
        {
            _securityToken = securityToken;
        }

        public listResponse GetUser(string userUUID)
        {
            var channel = new CustomChannelFactory<BrugerPortType, BrugerPortTypeClient>().Create(_securityToken);

            try
            {
                var request = GetListRequest(userUUID);

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

        private listRequest GetListRequest(string uuid)
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
                        UUIDIdentifikator = new[] { uuid }
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
