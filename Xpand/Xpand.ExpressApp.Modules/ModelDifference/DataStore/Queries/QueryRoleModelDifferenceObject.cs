﻿using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base.Security;
using DevExpress.Xpo;
using Xpand.ExpressApp.ModelDifference.DataStore.BaseObjects;

namespace Xpand.ExpressApp.ModelDifference.DataStore.Queries {
    public class QueryRoleModelDifferenceObject : QueryDifferenceObject<RoleModelDifferenceObject> {
        public QueryRoleModelDifferenceObject(Session session)
            : base(session) {
        }
        public override IQueryable<RoleModelDifferenceObject> GetActiveModelDifferences(string applicationName, string name) {
            var userWithRoles = SecuritySystem.CurrentUser as IUserWithRoles;
            if (userWithRoles != null) {
                IEnumerable<object> collection =
                    userWithRoles.Roles.Cast<XPBaseObject>().Select(role => role.ClassInfo.KeyProperty.GetValue(role));
                Type roleType = ((IRoleTypeProvider)SecuritySystem.Instance).RoleType;
                ITypeInfo roleTypeInfo = XafTypesInfo.Instance.PersistentTypes.Single(info => info.Type == roleType);
                var criteria = new ContainsOperator("Roles", new InOperator(roleTypeInfo.KeyMember.Name, collection.ToList()));

                var roleAspectObjects = base.GetActiveModelDifferences(applicationName, name).ToList();
                return roleAspectObjects.Where(aspectObject => aspectObject.Fit(criteria.ToString())).AsQueryable();
            }

            return base.GetActiveModelDifferences(applicationName, name).OfType<RoleModelDifferenceObject>().AsQueryable();
        }
    }
}
