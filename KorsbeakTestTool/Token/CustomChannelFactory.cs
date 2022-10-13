using KorsbeakTestTool.Utils;
using System.IdentityModel.Tokens;
using System.Net.Security;
using System.ServiceModel;

namespace KorsbeakTestTool.Token
{
    public class CustomChannelFactory<TPortType,TPortTypeClient>
        where TPortType: class
        where TPortTypeClient: ClientBase<TPortType>, new()
    {
        public TPortType Create(SecurityToken token)
        {
            var portType = new TPortTypeClient();

            // Disable revocation checking (do not use in production).
            // Should be uncommented if you intent to call DemoService locally.
            // demoPortType.ClientCredentials.ServiceCertificate.Authentication.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;
            EndpointIdentity identity = EndpointIdentity.CreateDnsIdentity(ConfigVariables.ServiceCertificateAlias);
            EndpointAddress endpointAddress = new EndpointAddress(portType.Endpoint.Address.Uri, identity);
            portType.Endpoint.Address = endpointAddress;
            var certificate = CertificateLoader.LoadCertificate(
                ConfigVariables.ClientCertificateStoreName,
                ConfigVariables.ClientCertificateStoreLocation,
                ConfigVariables.ClientCertificateThumbprint);
            portType.ClientCredentials.ClientCertificate.Certificate = certificate;

            //"The 'RequestHeader', 'http://kombit.dk/xml/schemas/RequestHeader/1/' required message part  was not signed."
            //orgPortType.Endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;
            portType.Endpoint.Contract.ProtectionLevel = ProtectionLevel.None;

            //Because of this: "The X.509 certificate SERIALNUMBER=CVR:19435075-FID:97480420 + CN=kombit-sp-signing2 (funktionscertifikat), O=KOMBIT A/S // CVR:19435075, C=DK
            //chain building failed. The certificate that was used has a trust chain that cannot be verified. Replace the certificate or change the certificateValidationMode. The certificate is not valid for the requested usage.\r\n"
            portType.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.PeerOrChainTrust;

            return portType.ChannelFactory.CreateChannelWithIssuedToken(token);
        }
    }
}
