﻿using Microsoft.VisualStudio.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangPackage
{
    public sealed class LaborasLangPreferences : LanguagePreferences
    {
        public LaborasLangPreferences() :
            base()
        {
        }

        public LaborasLangPreferences(IServiceProvider site, Guid langSvc, string name) :
            base(site, langSvc, name)
        {
        }

        public override void Init()
        {
            base.Init();

            // Supported stuff so far
            this.CutCopyBlankLines = true;new LaborasLangAuthoringScope();
            this.EnableShowMatchingBrace = true;
            this.EnableLeftClickForURLs = true;
            this.EnableMatchBraces = true;
            this.EnableMatchBracesAtCaret = true;
            this.HideAdvancedMembers = false;
            this.IndentSize = 4;
            this.IndentStyle = IndentingStyle.Smart;
            this.InsertTabs = true;
            this.LanguageName = "LaborasLang";
            this.LineNumbers = true;
            this.MaxErrorMessages = 100;
            this.MaxRegionTime = 2000;
            this.ParameterInformation = false;
            this.ShowNavigationBar = false;
            this.TabSize = 4;
            this.VirtualSpace = false;
            this.WordWrap = false;
            this.WordWrapGlyphs = false;

            // Unsupported stuff
            this.AutoListMembers = false;
            this.AutoOutlining = false;
            this.CodeSenseDelay = 0;
            this.EnableAsyncCompletion = false;
            this.EnableCodeSense = false;
            this.EnableCommenting = false;
            this.EnableFormatSelection = false;
            this.EnableQuickInfo = false;
        }
    }
}