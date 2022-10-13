using Kombit.SF1500.OrganizationSystem;
using System.Collections.Generic;
using System.Linq;

namespace KorsbeakTestTool.Dtos
{
    public class OrgUnitRegWrapper
    {
        public string Uuid { get; set; }
        public RegistreringType5 Registration5 { get; set; }
    }

    public class OU
    {
        public OU()
        {
            Childs = new List<OU>();
        }

        public static OU Create(OrgUnitRegWrapper orgUnitRegWrapper)
        {
            var ou = new OU
            {
                Uuid = orgUnitRegWrapper.Uuid,
                Name = GetName(orgUnitRegWrapper.Registration5),
                Type = GetOuType(orgUnitRegWrapper.Registration5),
                Status = GetOuStatus(orgUnitRegWrapper.Registration5),
                ParentUuid = GetParentUuid(orgUnitRegWrapper.Registration5)
            };

            return ou;
        }

        public string Uuid { get; set; }
        public string Name { get; set; }
        public OuType Type { get; set; }
        public OuStatus Status { get; set; }
        public string ParentUuid { get; set; }

        public List<OU> Childs { get; set; }

        private static string GetName(RegistreringType5 registreringType5)
        {
            return registreringType5.AttributListe.Egenskab.FirstOrDefault()?.EnhedNavn;
        }

        private static OuType GetOuType(RegistreringType5 registreringType5)
        {
            var typeItem = registreringType5.RelationListe.Enhedstype?.ReferenceID?.Item;
            switch (typeItem)
            {
                case ConfigVariables.ORGUNIT_TYPE_TEAM:
                    return OuType.Team;
                default:
                    return OuType.Department;
            }
        }

        private static OuStatus GetOuStatus(RegistreringType5 registreringType5)
        {
            var statusCode = registreringType5.TilstandListe.Gyldighed?.FirstOrDefault()?.GyldighedStatusKode;
            switch (statusCode)
            {
                case GyldighedStatusKodeType.Aktiv:
                    return OuStatus.Active;
                case GyldighedStatusKodeType.Inaktiv:
                    return OuStatus.Inactive;
                default:
                    return OuStatus.Unknown;
            }
        }

        private static string GetParentUuid(RegistreringType5 registreringType5)
        {
            return registreringType5.RelationListe.Overordnet?.ReferenceID?.Item;
        }
    }

    public enum OuType
    {
        Department,
        Team
    }

    public enum OuStatus
    {
        Unknown,
        Active,
        Inactive
    }
}
