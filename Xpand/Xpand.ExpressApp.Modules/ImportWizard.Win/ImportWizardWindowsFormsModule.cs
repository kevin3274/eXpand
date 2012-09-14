using System;
using System.ComponentModel;
using System.Drawing;
using DevExpress.ExpressApp;

namespace Xpand.ExpressApp.ImportWizard.Win {
    [ToolboxBitmap(typeof(ImportWizardWindowsFormsModule))]
    [ToolboxItem(true)]
    public sealed class ImportWizardWindowsFormsModule : XpandModuleBase {
        public const string XpandImportWizardWin = "eXpand.ImportWizard.Win";

        public ImportWizardWindowsFormsModule() {
            ResourcesExportedToModel.Add(typeof(ImportWizResourceLocalizer));
            ResourcesExportedToModel.Add(typeof(ImportWizFrameTemplateLocalizer));
        }

        protected override ModuleTypeList GetRequiredModuleTypesCore() {
            var requiredModuleTypesCore = base.GetRequiredModuleTypesCore();
            requiredModuleTypesCore.AddRange(new[] { typeof(ImportWizardModule) });
            return requiredModuleTypesCore;
        }
    }
}