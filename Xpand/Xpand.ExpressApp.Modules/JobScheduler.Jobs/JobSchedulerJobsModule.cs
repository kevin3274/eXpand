﻿using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using Xpand.ExpressApp.JobScheduler.Jobs.SendEmail;

namespace Xpand.ExpressApp.JobScheduler.Jobs {
    public class JobSchedulerJobsModule : XpandModuleBase {
        public JobSchedulerJobsModule() {
            RequiredModuleTypes.Add(typeof(JobSchedulerModule));
        }

        public override void Setup(XafApplication application) {
            base.Setup(application);
            if (application != null && !DesignMode) {
                application.SetupComplete += ApplicationOnSetupComplete;
            }
        }

        void ApplicationOnSetupComplete(object sender, EventArgs eventArgs) {
            var dynamicSecuritySystemObjects = new DynamicSecuritySystemObjects(Application);
            dynamicSecuritySystemObjects.BuildUser(typeof(SendEmailJobDetailDataMap), "UserUsers_UserSendEmailDataMapObjectUserSendEmailDataMaps", "UserSendEmailDataMapObjects", "Users");
            dynamicSecuritySystemObjects.BuildRole(typeof(SendEmailJobDetailDataMap), "RoleRoles_RoleSendEmailDataMaps", "RoleSendEmailDataMapObjects", "Roles");
        }

        public override void CustomizeTypesInfo(ITypesInfo typesInfo) {
            base.CustomizeTypesInfo(typesInfo);
            if (!RuntimeMode) {
                CreateDesignTimeCollection(typesInfo, typeof(SendEmailJobDetailDataMap), "Users");
                CreateDesignTimeCollection(typesInfo, typeof(SendEmailJobDetailDataMap), "Roles");
            }
        }

    }
}
