using System.Security.Cryptography.X509Certificates;

namespace KorsbeakTestTool
{
    // TO_BE_MODIFIED
    // All these constants can be modified to configure the client.
    // See the document "Programmers Guide - Serviceplatformen" for details.
    static class ConfigVariables
    {
        // The alias used for Serviceplatformen endpoint identity check.
        public const string ServiceCertificateAlias = "kombit-sp-signing-test (funktionscertifikat)";//TEST
        //public const string ServiceCertificateAlias = "kombit-sp-signing2 (funktionscertifikat)";//PROD

        // SHA-1 thumbprint of the client certificate to call STS and Serviceplatformen.
        //public const string ClientCertificateThumbprint = "9B 6B 8E 98 30 9E 53 89 24 35 BA AB 1D 0B 86 C4 F3 38 A1 DF";
        //public const string ClientCertificateThumbprint = "9210e11fc98bbb9ae2484e02a3fd078ed04efa4c"; //CN = APPLIKATOR ApS - Applikator Aps Nemid
        public const string ClientCertificateThumbprint = "9dddcde42046838a97b05c25c47ece56fca7533c"; //CN = APPLIKATOR ApS - NemID Test

        public const StoreLocation ClientCertificateStoreLocation = StoreLocation.CurrentUser;

        public const StoreName ClientCertificateStoreName = StoreName.My;

        // Entity ID for the Serviceplatform service to fetch token for and call.
        // This ID can be found in the service contract package from the Serviceplatform as 'service.entityID' inside /sp/service.properties.
        //public const string ServiceEntityId = "http://demo.prod-serviceplatformen.dk/service/DemoService/1";
        //public const string ServiceEntityId = "http://organisation.serviceplatformen.dk/service/organisation/5";
        public const string OrganizationServiceEntityId = "http://organisation.serviceplatformen.dk/service/organisation/5";

        // The STS issuer for token requests.
        //public const string StsIssuer = "https://adgangsstyring.eksterntest-stoettesystemerne.dk/";
        public const string StsIssuer = "https://adgangsstyring.stoettesystemerne.dk/";

        // The endpoint of the STS (Secure Token Service).
        public const string StsEndpoint = "https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed"; //TEST
        //public const string StsEndpoint = "https://adgangsstyring.stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed"; //PROD

        // The alias used for STS endpoint identity check.
	    public const string StsCertificateAlias = "test-ekstern-adgangsstyring (funktionscertifikat)"; //TEST
        //public const string StsCertificateAlias = "produktion-adgangsstyring (funktionscertifikat)"; //PROD

        // SHA-1 thumbprint of the certificate used for signing by STS.
        public const string StsCertificateThumbprint = "7002cf221d1d3979eca623599e43e0b6b4c8920c"; //TEST
        //public const string StsCertificateThumbprint = "439fed0f512b599793ab2b4d36bec5af65e98a1c"; //PROD

        public const StoreLocation StsCertificateStoreLocation = StoreLocation.CurrentUser;

        public const StoreName StsCertificateStoreName = StoreName.My;

        // The CVR of the municipality involved in the service agreement.
        // Used in the token request to STS.
        //public const string MunicipalityCvr = "29189846"; //Viborg
        public const string MunicipalityCvr = "11111111"; //Korsbeak

        //it appears that we need to use UUID instead of CVR in hierarchy request
        //public const string MinicipalityUUID = "f79ea045-08ae-4d3a-86b6-10d29a001ba2"; //Viborg
        public const string MinicipalityUUID = "1e03cedd-6f04-493a-89b0-62e3e7fd008d"; //Korsbeak

        //Test Person UUID
        public const string UserUUID = "ef8f9972-d6c7-4ea0-baf7-40ff1ddacf21"; //C=DK,O=11111111,CN=Matt Damon,Serial=ef8f9972-d6c7-4ea0-baf7-40ff1ddacf21

        // These are the UUIDs used to indicate the type of an OrgUnit
        public const string ORGUNIT_TYPE_DEPARTMENT = "16bf18c3-ed6f-44b0-b7a1-35f94984e519"; //
        public const string ORGUNIT_TYPE_TEAM = "2d9710bf-e9cc-465f-8ec7-46d5f2a64412"; //

        //UUID to search Organization functions for employees
        public const string EmployeeFunctionUUID = "02e61900-33e0-407f-a2a7-22f70221f003";



        // Below are some of the optional values that can be used in the CallContext.
        public const string AccountingInfo = "Applikator test run";
        public const string OnBehalfOfUser = "Rasmus Juhl Kallesøe";
        public const string CallersServiceCallIdentifier = "000000001-Applikator";
    }
}
