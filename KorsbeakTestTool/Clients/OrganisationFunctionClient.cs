using System;
using System.IdentityModel.Tokens;
using KorsbeakTestTool.Token;
using Kombit.SF1500.OrganisationFunktion;
using KorsbeakTestTool.Utils;

namespace KorsbeakTestTool.Clients
{
    public class OrganisationFunctionClient
    {
        private readonly SecurityToken _securityToken;

        public OrganisationFunctionClient(SecurityToken securityToken)
        {
            _securityToken = securityToken;
        }

        public listResponse GetOrganizationFunctions(string orgFuncUUID)
        {
            var channel = new CustomChannelFactory<OrganisationFunktionPortType, OrganisationFunktionPortTypeClient>().Create(_securityToken);

            try
            {
                var request = GetListRequest(orgFuncUUID);

                var response = channel.list(request);

                //EnsureSuccessResponse(response);

                return response;

            }
            catch (Exception ex)
            {
                //think about error handling, while there are a lot of places with questionable behaviour
                Console.WriteLine($"Exception: {ex}");
                throw;
            }
        }

        public listResponse ListOrganizationFunctions(soegResponse searchResponse)
        {
            var channel = new CustomChannelFactory<OrganisationFunktionPortType, OrganisationFunktionPortTypeClient>().Create(_securityToken);

            try
            {
                var request = GetListRequest(searchResponse);

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

        public soegResponse SearchOrganizationFunctions(string userUUID)
        {
            var channel = new CustomChannelFactory<OrganisationFunktionPortType, OrganisationFunktionPortTypeClient>().Create(_securityToken);

            try
            {
                var request = GetSearchRequest(userUUID);

                var response = channel.soeg(request);

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

        private void EnsureSuccessResponse(soegResponse response)
        {
            int statusCode = Int32.Parse(response.SoegResponse1.SoegOutput.StandardRetur.StatusKode);
            if (statusCode != 20)
            {
                //string message = StubUtil.ConstructSoapErrorMessage(statusCode, "FremsoegObjektHierarki", OrganisationSystemStubHelper.SERVICE, result.FremsoegobjekthierarkiResponse1.FremsoegObjekthierarkiOutput.StandardRetur.FejlbeskedTekst);
                //log.Error(message);
                //throw new SoapServiceException(message);
                throw new Exception($"SoapServiceException, with statusCode = {statusCode}, with message: {response.SoegResponse1.SoegOutput.StandardRetur.FejlbeskedTekst}");
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

        private listRequest GetListRequest(soegResponse searchResponse)
        {
            var uuids = searchResponse.SoegResponse1.SoegOutput.IdListe;
            return GetListRequest(uuids);
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

        private soegRequest GetSearchRequest(string uuid)
        {
            var request = new soegRequest()
            {
                RequestHeader = new RequestHeaderType()
                {
                    TransactionUUID = IdUtils.GenerateUuid()
                },

                SoegRequest1 = new SoegRequestType()
                {
                    CallContext = GetCallContext(),
                    SoegInput = new SoegInputType1()
                    {
                        MaksimalAntalKvantitet = "500",
                        FoersteResultatReference = "0",

                        RelationListe = new RelationListeType()
                        {
                            TilknyttedeBrugere = new BrugerFlerRelationType[1]
                            {
                                new BrugerFlerRelationType()
                                {
                                    ReferenceID = new UnikIdType()
                                    {
                                        Item = uuid,
                                        ItemElementName = ItemChoiceType.UUIDIdentifikator
                                    },
                                    Virkning = new VirkningType()
                                    {
                                        FraTidspunkt = new TidspunktType()
                                        {
                                            Item = DateTime.Now
                                        },
                                        TilTidspunkt = new TidspunktType()
                                        {
                                            Item = true
                                        }
                                    }
                                }
                            },
                            Funktionstype = new KlasseRelationType()
                            {
                                ReferenceID = new UnikIdType()
                                {
                                    Item = ConfigVariables.EmployeeFunctionUUID,
                                    ItemElementName = ItemChoiceType.UUIDIdentifikator
                                },
                                Virkning = new VirkningType()
                                {
                                    FraTidspunkt = new TidspunktType()
                                    {
                                        Item = DateTime.Now
                                    },
                                    TilTidspunkt = new TidspunktType()
                                    {
                                        Item = true
                                    }
                                }
                            }
                        },
                        AttributListe = new AttributListeType(),
                        TilstandListe = new TilstandListeType()
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
