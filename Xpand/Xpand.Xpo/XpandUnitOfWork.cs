﻿using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Xpo.Metadata.Helpers;

namespace Xpand.Xpo {
    public class XpandUnitOfWork : UnitOfWork {
        public XpandUnitOfWork() {
        }

        public XpandUnitOfWork(XPDictionary dictionary)
            : base(dictionary) {
        }

        public XpandUnitOfWork(IDataLayer layer, params IDisposable[] disposeOnDisconnect)
            : base(new XpandObjectLayer(layer), disposeOnDisconnect) {
        }

        public XpandUnitOfWork(IObjectLayer layer, params IDisposable[] disposeOnDisconnect)
            : base(layer, disposeOnDisconnect) {
        }

        protected override MemberInfoCollection GetPropertiesListForUpdateInsert(object theObject, bool isUpdate) {
            var supportChangedMembers = theObject as ISupportChangedMembers;
            if (supportChangedMembers != null && !IsNewObject(supportChangedMembers)) {
                XPClassInfo ci = GetClassInfo(supportChangedMembers);
                var changedMembers = new MemberInfoCollection(ci);
                var memberInfos = base.GetPropertiesListForUpdateInsert(supportChangedMembers, isUpdate).Where(m => MemberHasChanged(supportChangedMembers, m));
                changedMembers.AddRange(memberInfos);
                return changedMembers;
            }

            return base.GetPropertiesListForUpdateInsert(theObject, isUpdate);
        }

        bool MemberHasChanged(ISupportChangedMembers supportChangedMembers, XPMemberInfo m) {
            return m.HasAttribute(typeof(PersistentAttribute)) || m.IsKey || m is ServiceField ||
                   supportChangedMembers.ChangedProperties.Contains(m.Name);
        }
    }
}