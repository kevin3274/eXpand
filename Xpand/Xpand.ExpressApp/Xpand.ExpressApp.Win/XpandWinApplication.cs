﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Forms;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Win;
using DevExpress.ExpressApp.Win.Core.ModelEditor;
using DevExpress.ExpressApp.Win.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Xpo.DB;
using Xpand.ExpressApp.Model;
using Xpand.ExpressApp.Security;
using Xpand.ExpressApp.Win.ViewStrategies;
using Xpand.ExpressApp.Core;

namespace Xpand.ExpressApp.Win {

    public class XpandWinApplication : WinApplication, IWinApplication {
        static XpandWinApplication _application;
        DataCacheNode _cacheNode;
        ApplicationModulesManager _applicationModulesManager;


        public XpandWinApplication() {
            if (_application == null)
                Application.ThreadException += (sender, args) => HandleException(args.Exception, this);
            else {
                Application.ThreadException += (sender, args) => HandleException(args.Exception, _application);
            }
            DetailViewCreating += OnDetailViewCreating;
            ListViewCreating += OnListViewCreating;
            if (_application == null)
                _application = this;
        }

        protected override void OnSetupComplete() {
            base.OnSetupComplete();
            var xpandObjectSpaceProvider = (ObjectSpaceProvider as XpandObjectSpaceProvider);
            if (xpandObjectSpaceProvider != null)
                xpandObjectSpaceProvider.SetClientSideSecurity(((IModelOptionsClientSideSecurity)Model.Options).ClientSideSecurity);
        }

        ApplicationModulesManager IXafApplication.ApplicationModulesManager {
            get { return _applicationModulesManager; }
        }

        public event EventHandler UserDifferencesLoaded;

        protected virtual void OnUserDifferencesLoaded(EventArgs e) {
            EventHandler handler = UserDifferencesLoaded;
            if (handler != null) handler(this, e);
        }

        protected override void LoadUserDifferences() {
            base.LoadUserDifferences();
            OnUserDifferencesLoaded(EventArgs.Empty);
        }

        protected override Form CreateModelEditorForm() {
            var controller = new ModelEditorViewController(Model, CreateUserModelDifferenceStore());
            ModelDifferenceStore modelDifferencesStore = CreateModelDifferenceStore();
            if (modelDifferencesStore != null) {
                var modulesDiffStoreInfo = new List<ModuleDiffStoreInfo> { new ModuleDiffStoreInfo(null, modelDifferencesStore, "Model") };
                controller.SetModuleDiffStore(modulesDiffStoreInfo);
            }
            return new ModelEditorForm(controller, new SettingsStorageOnModel(((IModelApplicationModelEditor)Model).ModelEditorSettings));
        }

        public new string ConnectionString {
            get { return base.ConnectionString; }
            set {
                base.ConnectionString = value;
                ((IXafApplication)this).ConnectionString = value;
            }
        }

        string IXafApplication.ConnectionString { get; set; }

        public new SettingsStorage CreateLogonParameterStoreCore() {
            return base.CreateLogonParameterStoreCore();
        }

        public new void WriteLastLogonParameters(DetailView view, object logonObject) {
            base.WriteLastLogonParameters(view, logonObject);
        }

        protected override void OnCreateCustomObjectSpaceProvider(CreateCustomObjectSpaceProviderEventArgs args) {
            base.OnCreateCustomObjectSpaceProvider(args);
            if (args.ObjectSpaceProvider == null)
                this.CreateCustomObjectSpaceprovider(args);
        }

        public new void Start() {
            if (SecuritySystem.LogonParameters is IXpandLogonParameters) ReadLastLogonParameters(SecuritySystem.LogonParameters);
            base.Start();
        }

        public event EventHandler<ViewShownEventArgs> AfterViewShown;

        public event EventHandler<CreatingListEditorEventArgs> CustomCreateListEditor;


        protected override ApplicationModulesManager CreateApplicationModulesManager(ControllersManager controllersManager) {
            _applicationModulesManager = base.CreateApplicationModulesManager(controllersManager);
            return _applicationModulesManager;
        }
        public event CancelEventHandler ConfirmationRequired;


        protected void OnConfirmationRequired(CancelEventArgs e) {
            CancelEventHandler handler = ConfirmationRequired;
            if (handler != null) handler(this, e);

        }

        public virtual void OnAfterViewShown(Frame frame, Frame sourceFrame) {
            if (AfterViewShown != null) {
                AfterViewShown(this, new ViewShownEventArgs(frame, sourceFrame));
            }
        }

        string IXafApplication.ModelAssemblyFilePath {
            get { return GetModelAssemblyFilePath(); }
        }

        public void OnCustomCreateListEditor(CreatingListEditorEventArgs e) {
            EventHandler<CreatingListEditorEventArgs> handler = CustomCreateListEditor;
            if (handler != null) handler(this, e);
        }

        protected override void OnCustomProcessShortcut(CustomProcessShortcutEventArgs args) {
            base.OnCustomProcessShortcut(args);
            new ViewShortCutProccesor(this).Proccess(args);

        }

        public override ConfirmationResult AskConfirmation(ConfirmationType confirmationType) {
            var cancelEventArgs = new CancelEventArgs();
            OnConfirmationRequired(cancelEventArgs);
            return cancelEventArgs.Cancel ? ConfirmationResult.No : base.AskConfirmation(confirmationType);
        }

        protected override ShowViewStrategyBase CreateShowViewStrategy() {
            var showViewStrategyBase = base.CreateShowViewStrategy();
            return showViewStrategyBase is ShowInMultipleWindowsStrategy
                       ? new XpandShowInMultipleWindowsStrategy(this)
                       : showViewStrategyBase;
        }

        void OnListViewCreating(object sender, ListViewCreatingEventArgs args) {
            args.View = ViewFactory.CreateListView(this, args.ViewID, args.CollectionSource, args.IsRoot);
        }

        protected override Window CreateWindowCore(TemplateContext context, ICollection<Controller> controllers, bool isMain, bool activateControllersImmediatelly) {
            Tracing.Tracer.LogVerboseValue("WinApplication.CreateWindowCore.activateControllersImmediatelly", activateControllersImmediatelly);
            return new XpandWinWindow(this, context, controllers, isMain, activateControllersImmediatelly);
        }

        protected override Window CreatePopupWindowCore(TemplateContext context, ICollection<Controller> controllers) {
            return new XpandWinWindow(this, context, controllers, false, true);
        }

        void OnDetailViewCreating(object sender, DetailViewCreatingEventArgs args) {
            args.View = ViewFactory.CreateDetailView(this, args.ViewID, args.Obj, args.ObjectSpace, args.IsRoot);
        }


        public override IModelTemplate GetTemplateCustomizationModel(IFrameTemplate template) {
            var applicationBase = ((ModelApplicationBase)Model);
            if (applicationBase.Id == "Application") {
                var list = new List<ModelApplicationBase>();
                while (applicationBase.LastLayer.Id != "UserDiff" && applicationBase.LastLayer.Id != AfterSetupLayerId) {
                    var modelApplicationBase = applicationBase.LastLayer;
                    list.Add(modelApplicationBase);
                    ModelApplicationHelper.RemoveLayer(modelApplicationBase);
                }
                var modelTemplate = base.GetTemplateCustomizationModel(template);
                foreach (var modelApplicationBase in list) {
                    ModelApplicationHelper.AddLayer((ModelApplicationBase)Model, modelApplicationBase);
                }
                return modelTemplate;
            }
            return base.GetTemplateCustomizationModel(template);
        }

        protected override ListEditor CreateListEditorCore(IModelListView modelListView, CollectionSourceBase collectionSource) {
            var creatingListEditorEventArgs = new CreatingListEditorEventArgs(modelListView, collectionSource);
            OnCustomCreateListEditor(creatingListEditorEventArgs);
            return creatingListEditorEventArgs.Handled ? creatingListEditorEventArgs.ListEditor : base.CreateListEditorCore(modelListView, collectionSource);
        }


        public static void HandleException(Exception exception, XpandWinApplication xpandWinApplication) {
            xpandWinApplication.HandleException(exception);
        }

        public XpandWinApplication(IContainer container) {
            container.Add(this);
        }

        IDataStore IXafApplication.GetDataStore(IDataStore dataStore) {
            if ((ConfigurationManager.AppSettings["DataCache"] + "").Contains("Client")) {
                if (_cacheNode == null) {
                    var _cacheRoot = new DataCacheRoot(dataStore);
                    _cacheNode = new DataCacheNode(_cacheRoot);
                }
                return _cacheNode;
            }
            return null;
        }

        string IXafApplication.RaiseEstablishingConnection() {
            return this.GetConnectionString();
        }
    }

}