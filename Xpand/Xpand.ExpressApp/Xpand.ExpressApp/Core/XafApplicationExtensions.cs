﻿using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.MiddleTier;
using DevExpress.ExpressApp.Security;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Xpand.ExpressApp.MiddleTier;

namespace Xpand.ExpressApp.Core {
    public static class XafApplicationExtensions {
        public static T FindModule<T>(this XafApplication xafApplication) where T : ModuleBase {
            return (T)xafApplication.Modules.FindModule(typeof(T));
        }

        public static SimpleDataLayer CreateCachedDataLayer(this XafApplication xafApplication, IDataStore argsDataStore) {
            var cacheRoot = new DataCacheRoot(argsDataStore);
            var cacheNode = new DataCacheNode(cacheRoot);
            return new SimpleDataLayer(XpandModuleBase.Dictiorary, cacheNode);
        }

        public static string GetConnectionString(this XafApplication xafApplication) {

            if (xafApplication is ServerApplication && !(xafApplication is IXafApplication))
                throw new NotImplementedException("Use " + typeof(XpandServerApplication) + " insted of " +
                                                  xafApplication.GetType());
            var connectionString = ((IXafApplication)xafApplication).ConnectionString;
            return connectionString;

        }
        public static void CreateCustomObjectSpaceprovider(this XafApplication xafApplication, CreateCustomObjectSpaceProviderEventArgs args) {
            var connectionString = getConnectionStringWithOutThreadSafeDataLayerInitialization(args);
            ((IXafApplication)xafApplication).ConnectionString = connectionString;
            var connectionProvider = XpoDefault.GetConnectionProvider(connectionString, AutoCreateOption.DatabaseAndSchema);
            IDataStore dataStore = ((IXafApplication)xafApplication).GetDataStore(connectionProvider);
            var selectDataSecurityProvider = xafApplication.Security as ISelectDataSecurityProvider;
            args.ObjectSpaceProvider = dataStore != null ? new XpandObjectSpaceProvider(new MultiDataStoreProvider(dataStore), selectDataSecurityProvider) : new XpandObjectSpaceProvider(new MultiDataStoreProvider(connectionString), selectDataSecurityProvider);
        }

        static string getConnectionStringWithOutThreadSafeDataLayerInitialization(CreateCustomObjectSpaceProviderEventArgs args) {
            return args.Connection != null ? args.Connection.ConnectionString : args.ConnectionString;
        }
    }
}