using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using KorsbeakTestTool.Token;
using KorsbeakTestTool.Dtos;
using KorsbeakTestTool.Clients;
using Kombit.SF1500.User;

namespace KorsbeakTestTool
{
    public class Program
    {
        public const string GuidRegex = "[a-z0-9]{8}-([a-z0-9]{4}-){3}[a-z0-9]{12}";

        public static void Main(string[] args)
        {
            try
            {
                string userUUID;

                if (args.Length != 1 || !Regex.IsMatch(args[0], GuidRegex))
                {
                    ShowUsage();
                    return;
                }

                userUUID = args[0];


                var token = new TokenProvider()
                    .GetSamlToken(
                        ConfigVariables.MunicipalityCvr,
                        ConfigVariables.OrganizationServiceEntityId);

                //GetOrganizationHierarchyAndSave(token);

                DrawConsoleLine("Getting info about UUID:");
                GetUser(token, userUUID);
                GetOrganizationFunctions(token, userUUID);

            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        private static void ShowUsage()
        {
            DrawConsoleLine("Wrong usage!\n", ConsoleColor.Yellow);
            DrawConsoleLine("\tUsage:");
            DrawConsoleLine("\t\tKorsbeakTestTool.exe ef8f9972-d6c7-4ea0-baf7-40ff1ddacf21", ConsoleColor.Green);
        }

        private static void DrawConsoleLine(string text, ConsoleColor? color = null)
        {
            if (color is null)
            {
                Console.WriteLine(text);
                return;
            }

            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color.Value;
            Console.WriteLine(text);
            Console.ForegroundColor = previousColor;
        }

        private static void GetUser(SecurityToken token, string userUUID)
        {
            var serviceClient = new UserClient(token);
            var response = serviceClient.GetUser(userUUID);
            ShowUserName(userUUID, response);

            var personUUID = GetPersonUUID(response);

            if (!string.IsNullOrEmpty(personUUID))
                GetPerson(token, personUUID);
        }

        private static void ShowUserName(string userUUID, listResponse userListResponse)
        {
            var userName = userListResponse
                   .ListResponse1
                   .ListOutput?
                   .FiltreretOejebliksbillede?
                   .FirstOrDefault()?
                   .Registrering?
                   .FirstOrDefault()?
                   .AttributListe?
                   .Egenskab
                   .FirstOrDefault()
                   .BrugerNavn;

            DrawConsoleLine($"\tUserUUID: {userUUID}; Name: {userName}", ConsoleColor.Green);
        }

        private static string GetPersonUUID(Kombit.SF1500.User.listResponse userListResponse)
        {
            return userListResponse
                   .ListResponse1
                   .ListOutput?
                   .FiltreretOejebliksbillede?
                   .FirstOrDefault()?
                   .Registrering?
                   .FirstOrDefault()?
                   .RelationListe?
                   .TilknyttedePersoner
                   .FirstOrDefault()
                   .ReferenceID?
                   .Item;
        }

        private static void GetPerson(SecurityToken token, string personUUID)
        {
            var serviceClient = new PersonClient(token);
            var listResponse = serviceClient.GetPerson(personUUID);
            ShowPersonName(listResponse);
        }

        private static void ShowPersonName(Kombit.SF1500.Person.listResponse personListResponse)
        {
            var registering = personListResponse
                   .ListResponse1
                   .ListOutput?
                   .FiltreretOejebliksbillede?
                   .FirstOrDefault()?
                   .Registrering?
                   .FirstOrDefault();

            var attributes = registering?
                   .AttributListe?
                   .FirstOrDefault();

            var cprText = attributes?.CPRNummerTekst;
            var nameText = attributes?.NavnTekst;
            var personUUID = attributes?.BrugervendtNoegleTekst;


            DrawConsoleLine($"\tPersonUUID: {personUUID}; Name: {nameText}; Cpr: {cprText}", ConsoleColor.Green);
        }


        private static void GetOrganizationFunctions(SecurityToken token, string userUUID)
        {
            var serviceClient = new OrganisationFunctionClient(token);
            var searchResponse = serviceClient.SearchOrganizationFunctions(userUUID);
            var orgFunctionsUUID = GetOrganizationFunctions(searchResponse);
            var listResponse = serviceClient.ListOrganizationFunctions(searchResponse);
            ShowOrgFunctions(listResponse);

            var ouUUIDs = GetRelatedOUsUUID(listResponse);
            var ouMap = GetOrganizationsMap(token);
            ShowOrganisations(ouUUIDs, uuid => ouMap[uuid].Name);
        }

        private static void ShowOrganisations(string[] uuids, Func<string,string> getOuName)
        {
            DrawConsoleLine($"\tOrganizations:", ConsoleColor.Green);

            foreach (var uuid in uuids)
            {
                DrawConsoleLine($"\t\tOrgUUID: {uuid}; OrgName: {getOuName(uuid)}", ConsoleColor.Green);
            }
        }

        //Seems like for Organization this flow doesn't work, cause provided UUIDs
        //wasn't found and 44 code returned, BUT!
        //UUIDs were found in Hierarchy call results!
        //So, for now I will check orgUUIDs from orgFuncs in hierarchy
        private static void GetOrganizations(SecurityToken token, string[] uuids)
        {
            var organizationClient = new OrganisationClient(token);
            var listResponse = organizationClient.ListOrganizations(uuids);
            ShowOrganizations(listResponse);
        }

        private static string[] GetRelatedOUsUUID(Kombit.SF1500.OrganisationFunktion.listResponse listResponse)
        {
            var ouUUIDs = new List<string>();

            var filteredSnapshots = listResponse
                   .ListResponse1
                   .ListOutput?
                   .FiltreretOejebliksbillede;

            foreach (var snapshot in filteredSnapshots)
            {
                var ouUUID = snapshot
                        .Registrering
                        .FirstOrDefault()?
                        .RelationListe?
                        .TilknyttedeEnheder?
                        .FirstOrDefault()?
                        .ReferenceID.
                        Item;

                if (!string.IsNullOrEmpty(ouUUID))
                    ouUUIDs.Add(ouUUID);
            }

            return ouUUIDs.ToArray();
        }

        private static void ShowOrganizations(Kombit.SF1500.Organization.listResponse listResponse)
        {
            var filteredSnapshots = listResponse
                   .ListResponse1
                   .ListOutput?
                   .FiltreretOejebliksbillede;

            foreach (var snapshot in filteredSnapshots)
            {
                var attributes = snapshot.Registrering?.FirstOrDefault().AttributListe;
                var characteristic = attributes.Egenskab?.FirstOrDefault();
                var orgUUID = characteristic.BrugervendtNoegleTekst;
                var orgName = characteristic.OrganisationNavn;

                DrawConsoleLine($"\tOrgUUID: {orgUUID}; OrgName: {orgName}", ConsoleColor.Green);
            }
        }

        private static void ShowOrgFunctions(Kombit.SF1500.OrganisationFunktion.listResponse listResponse)
        {
            var filteredSnapshots = listResponse
                   .ListResponse1
                   .ListOutput?
                   .FiltreretOejebliksbillede;

            DrawConsoleLine($"\tOrganization Functions:", ConsoleColor.Green);

            foreach (var snapshot in filteredSnapshots)
            {
                var attributes = snapshot.Registrering?.FirstOrDefault().AttributListe;
                var characteristic = attributes.Egenskab?.FirstOrDefault();
                var orgFuncUUID = characteristic.BrugervendtNoegleTekst;
                var orgFuncName = characteristic.FunktionNavn;

                DrawConsoleLine($"\t\tOrgFuncUUID: {orgFuncUUID}; OrgFuncName: {orgFuncName}", ConsoleColor.Green);
            }
        }

        private static string[] GetOrganizationFunctions(Kombit.SF1500.OrganisationFunktion.soegResponse searchResponse)
        {
            return searchResponse.SoegResponse1.SoegOutput.IdListe;
        }

        private static IDictionary<string,OU> GetOrganizationsMap(SecurityToken token)
        {
            var organizationServiceClient = new OrganizationServiceClient(token);
            var orgUnitRegWrappers = organizationServiceClient.ReadOUHierarchy(ConfigVariables.MinicipalityUUID);
            var map = orgUnitRegWrappers.Select(OU.Create).ToDictionary(k => k.Uuid);
            return map;
        }

        private static void GetOrganizationHierarchyAndSave(SecurityToken token)
        {
            var organizationServiceClient = new OrganizationServiceClient(token);
            var orgUnitRegWrappers = organizationServiceClient.ReadOUHierarchy(ConfigVariables.MinicipalityUUID);

            SaveItems(orgUnitRegWrappers, "Korsbeak_OrgUnitRegWrappers");

            var ouTree = GetOuTree(orgUnitRegWrappers);
            SaveItems(ouTree, "Korsbeak_OuTree");
        }

        private static IList<OU> GetOuTree(IReadOnlyList<OrgUnitRegWrapper> orgUnitRegWrappers)
        {
            var map = orgUnitRegWrappers.Select(OU.Create).ToDictionary(k => k.Uuid);
            var tree = new List<OU>();
            foreach (var kvPair in map)
            {
                if (string.IsNullOrEmpty(kvPair.Value.ParentUuid))
                {
                    tree.Add(kvPair.Value);
                    continue;
                }

                var parentUuid = kvPair.Value.ParentUuid;
                if (map.TryGetValue(parentUuid, out var parentOu))
                {
                    parentOu.Childs.Add(kvPair.Value);
                }
                else
                {
                    //think about situation when parent wasn't found
                    Console.WriteLine($"OU with Uuid: '{parentUuid}' is not found");
                }
            }

            return tree;
        }

        private static void SaveItems<T>(T items, string title)
        {
            var dateFormated = DateTime.Now.ToString("ddMMyy_hhmmss");
            var filePath = $".\\{title}_{dateFormated}.xml";

            SaveToFile(items, filePath);
            Console.WriteLine($"File was saved: '{filePath}'");
        }

        private static void SaveToFile<T>(T ous, string filePath)
        {
            using (var memoryStream = Serialize(ous))
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    memoryStream.WriteTo(fs);
                }
            }
        }

        private static MemoryStream Serialize<T>(T serializableObject)
        {
            var stream = new MemoryStream();
            var encodedWriter = new StreamWriter(stream, Encoding.UTF8);

            var serializer = new XmlSerializer(serializableObject.GetType());
            serializer.Serialize(new XmlTextWriter(encodedWriter), serializableObject);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}

