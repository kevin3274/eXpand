﻿using System;
using System.Collections.Generic;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using eXpand.ExpressApp.AdditionalViewControlsProvider.Logic;
using eXpand.ExpressApp.Attributes;

namespace FeatureCenter.Module.RuntimeFieldsFromModel
{
    public class AttributeRegistrator:Module.AttributeRegistrator
    {
        public override IEnumerable<Attribute> GetAttributes(ITypeInfo typesInfo) {
            if (typesInfo.Type!=typeof(Customer))yield break;
            yield return new AdditionalViewControlsRuleAttribute(Captions.ViewMessage + " " + Captions.HeaderRuntimeMemberFromModel, "1=1", "1=1",Captions.ViewMessageRuntimeMemberFromModel, Position.Bottom){ViewType = ViewType.ListView,View = "RuntimeFieldsFromModel_ListView"};
            yield return new AdditionalViewControlsRuleAttribute(Captions.Header + " " + Captions.HeaderRuntimeMemberFromModel, "1=1", "1=1",
                Captions.HeaderRuntimeMemberFromModel, Position.Top){View = "RuntimeFieldsFromModel_ListView"};
            yield return new CloneViewAttribute(CloneViewType.DetailView, "RuntimeFieldsFromModel_DetailView");
            yield return new CloneViewAttribute(CloneViewType.ListView, "RuntimeFieldsFromModel_ListView") { DetailView = "RuntimeFieldsFromModel_DetailView" };
            yield return new NavigationItemAttribute("Add Runtime Fields From Model","RuntimeFieldsFromModel_ListView");
            yield return new DisplayFeatureModelAttribute("RuntimeFieldsFromModel_ListView", "RuntimeFieldsFromModel");
            yield return new DisplayFeatureModelAttribute("RuntimeFieldsFromModel_DetailView", "RuntimeFieldsFromModel");
        }
    }
}