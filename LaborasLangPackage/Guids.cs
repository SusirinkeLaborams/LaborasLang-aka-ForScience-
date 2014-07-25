// Guids.cs
// MUST match guids.h
using System;

namespace LaborasLangPackage
{
    static class GuidList
    {
        public const string guidLaborasLangPackagePkgString = "643e7116-ca4e-459e-8323-4f89c3958f82";
        public const string guidLaborasLangPackageCmdSetString = "6d4b9a7d-cebc-409d-afaa-0e9f86f30981";
        public const string guidLaborasLangPackageEditorFactoryString = "e37d924b-d43c-4cfd-9701-0dc4892b70c6";

        public static readonly Guid guidLaborasLangPackageCmdSet = new Guid(guidLaborasLangPackageCmdSetString);
        public static readonly Guid guidLaborasLangPackageEditorFactory = new Guid(guidLaborasLangPackageEditorFactoryString);
    };
}