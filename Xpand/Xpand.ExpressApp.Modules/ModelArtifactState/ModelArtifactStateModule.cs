﻿using System;
using System.ComponentModel;
using System.Drawing;
using DevExpress.ExpressApp.Model;
using Xpand.ExpressApp.ArtifactState.Model;
using Xpand.ExpressApp.ConditionalActionState;
using Xpand.ExpressApp.ConditionalControllerState;

namespace Xpand.ExpressApp.ModelArtifactState {
    [ToolboxBitmap(typeof(ModelArtifactStateModule))]
    [ToolboxItem(true)]
    public class ModelArtifactStateModule : XpandModuleBase {
        public ModelArtifactStateModule() {
            RequiredModuleTypes.Add(typeof(ConditionalControllerStateModule));
            RequiredModuleTypes.Add(typeof(ConditionalActionStateModule));
        }
        public override void ExtendModelInterfaces(ModelInterfaceExtenders extenders) {
            base.ExtendModelInterfaces(extenders);
            extenders.Add<IModelApplication, IModelApplicationModelArtifactState>();
        }
    }
}