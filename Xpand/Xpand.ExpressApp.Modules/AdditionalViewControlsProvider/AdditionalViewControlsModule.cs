using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using Xpand.ExpressApp.AdditionalViewControlsProvider.Logic;
using Xpand.ExpressApp.AdditionalViewControlsProvider.Model;
using Xpand.ExpressApp.AdditionalViewControlsProvider.NodeUpdaters;
using Xpand.ExpressApp.Logic;
using Xpand.ExpressApp.Logic.Model;

namespace Xpand.ExpressApp.AdditionalViewControlsProvider {
    [ToolboxBitmap(typeof(AdditionalViewControlsModule))]
    [ToolboxItem(false)]
    public sealed class AdditionalViewControlsModule : LogicModuleBase<IAdditionalViewControlsRule, AdditionalViewControlsRule>, IModelExtender {
        #region IModelExtender Members
        public override void ExtendModelInterfaces(ModelInterfaceExtenders extenders) {
            extenders.Add<IModelApplication, IModelApplicationAdditionalViewControls>();
        }

        public override void CustomizeTypesInfo(DevExpress.ExpressApp.DC.ITypesInfo typesInfo) {
            base.CustomizeTypesInfo(typesInfo);
            var typeInfos = typesInfo.PersistentTypes.Where(info => info.FindAttribute<PessimisticLockingMessageAttribute>() != null);
            foreach (var typeInfo in typeInfos) {
                var memberInfo = typeInfo.FindMember("LockedUserMessage");
                if (memberInfo == null) {
                    var xpClassInfo = Dictiorary.GetClassInfo(typeInfo.Type);
                    var lockedUserMessageXpMemberInfo = new LockedUserMessageXpMemberInfo(xpClassInfo);
                    lockedUserMessageXpMemberInfo.AddAttribute(new BrowsableAttribute(false));
                    XafTypesInfo.Instance.RefreshInfo(typeInfo);
                }
            }
        }

        public override void AddGeneratorUpdaters(ModelNodesGeneratorUpdaters updaters) {
            base.AddGeneratorUpdaters(updaters);
            updaters.Add(new AdditionalViewControlsDefaultGroupContextNodeUpdater());
            updaters.Add(new AdditionalViewControlsRulesNodeUpdater());
            updaters.Add(new AdditionalViewControlsDefaultContextNodeUpdater());
        }
        #endregion
        protected override IModelLogic GetModelLogic(IModelApplication applicationModel) {
            return ((IModelApplicationAdditionalViewControls)applicationModel).AdditionalViewControls;
        }
    }
}
