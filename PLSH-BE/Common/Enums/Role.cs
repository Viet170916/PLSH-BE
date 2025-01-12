using System.ComponentModel;

namespace Common.Enums
{
    public enum RoleEnum
    {
        [Description("Default")]
        Default = 0,
        [Description("Manager (EM)")]
        EM = 1,
        [Description("CO-Manager (COEM)")]
        COEM = 2,
        [Description("Partner (EP)")]
        EP = 3,
        [Description("BUTEAM")]
        BUTEAM = 4,
        [Description("BUPIC")]
        BUPIC = 5,
        [Description("DS")]
        DS = 6,
        [Description("APIC")]
        APIC = 8,
        [Description("HOA")]
        HOA = 13
    }
}

