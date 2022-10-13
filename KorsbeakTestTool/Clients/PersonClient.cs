using System;
using System.IdentityModel.Tokens;
using KorsbeakTestTool.Token;
using Kombit.SF1500.Person;
using KorsbeakTestTool.Utils;

namespace KorsbeakTestTool.Clients
{
    public class PersonClient
    {
        private readonly SecurityToken _securityToken;

        public PersonClient(SecurityToken securityToken)
        {
            _securityToken = securityToken;
        }

        public listResponse GetPerson(string personUUID)
        {
            var channel = new CustomChannelFactory<PersonPortType, PersonPortTypeClient>().Create(_securityToken);

           try
           {
                var request = GetListRequest(personUUID);

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
