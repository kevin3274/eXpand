using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using Xpand.ExpressApp.FilterDataStore.Core;
using Xpand.ExpressApp.FilterDataStore.Model;
using Xpand.Xpo.DB;
using Xpand.Xpo.Filtering;
using Xpand.ExpressApp.Core;
using DevExpress.Persistent.Base;

namespace Xpand.ExpressApp.FilterDataStore {
    public abstract class FilterDataStoreModuleBase : XpandModuleBase {
        static FilterDataStoreModuleBase() {
            _tablesDictionary = new Dictionary<string, Type>();
        }

        protected static Dictionary<string, Type> _tablesDictionary;
        private Dictionary<ITypeInfo, ITypeInfo> _BaseTypesDictionary = new Dictionary<ITypeInfo,ITypeInfo>();
        public override void Setup(XafApplication application) {
            base.Setup(application);
            application.CreateCustomObjectSpaceProvider += ApplicationOnCreateCustomObjectSpaceProvider;
        }

        private void ApplicationOnCreateCustomObjectSpaceProvider(object sender, CreateCustomObjectSpaceProviderEventArgs createCustomObjectSpaceProviderEventArgs) {
            if (!(createCustomObjectSpaceProviderEventArgs.ObjectSpaceProvider is IXpandObjectSpaceProvider))
                Application.CreateCustomObjectSpaceprovider(createCustomObjectSpaceProviderEventArgs);
        }

        public override void Setup(ApplicationModulesManager moduleManager) {
            base.Setup(moduleManager);
            if (FilterProviderManager.IsRegistered && ProxyEventsSubscribed.HasValue && ProxyEventsSubscribed.Value) {
                SubscribeToDataStoreProxyEvents();
            }
        }
        void SubscribeToDataStoreProxyEvents() {
            if (Application != null && Application.ObjectSpaceProvider != null) {
                var objectSpaceProvider = (Application.ObjectSpaceProvider);
                if (!(objectSpaceProvider is IXpandObjectSpaceProvider)) {
                    throw new NotImplementedException("ObjectSpaceProvider does not implement " + typeof(IXpandObjectSpaceProvider).FullName);
                }
                DataStoreProxy proxy = ((IXpandObjectSpaceProvider)objectSpaceProvider).DataStoreProvider.Proxy;
                proxy.DataStoreModifyData += (o, args) => ModifyData(args.ModificationStatements);
                proxy.DataStoreSelectData += Proxy_DataStoreSelectData;
                ProxyEventsSubscribed = true;
            }
        }

        protected abstract bool? ProxyEventsSubscribed { get; set; }

        public override void CustomizeTypesInfo(ITypesInfo typesInfo) {
            base.CustomizeTypesInfo(typesInfo);
            if (FilterProviderManager.IsRegistered && FilterProviderManager.Providers != null) {
                SubscribeToDataStoreProxyEvents();
                
                foreach (var persistentType in typesInfo.PersistentTypes.Where(info => info.IsPersistent)) {
                    GetBaseTypeInfo(persistentType);
                    var xpClassInfo = XafTypesInfo.XpoTypeInfoSource.GetEntityClassInfo(persistentType.Type);
                    if (xpClassInfo.TableName != null && xpClassInfo.ClassType != null) {
                        if (!IsMappedToParent(xpClassInfo))
                            _tablesDictionary.Add(xpClassInfo.TableName, xpClassInfo.ClassType);
                    }
                }
                CreateMembers(typesInfo);
            }
        }

        bool IsMappedToParent(XPClassInfo xpClassInfo) {
            var attributeInfo = xpClassInfo.FindAttributeInfo(typeof(MapInheritanceAttribute));
            return attributeInfo != null &&
                   ((MapInheritanceAttribute)attributeInfo).MapType == MapInheritanceType.ParentTable;
        }

        private ITypeInfo GetBaseTypeInfo(ITypeInfo typeInfo)
        {
            ITypeInfo persistentBaseInfo = typeInfo;
            if (_BaseTypesDictionary.TryGetValue(typeInfo, out persistentBaseInfo))
                return persistentBaseInfo;
            for (var baseInfo = typeInfo; baseInfo != null; baseInfo = baseInfo.Base)
            {
                if (baseInfo.IsPersistent)
                    persistentBaseInfo = baseInfo;
            }
            _BaseTypesDictionary.Add(typeInfo, persistentBaseInfo);
            return persistentBaseInfo;
        }
        void CreateMembers(ITypesInfo typesInfo) {
            foreach (FilterProviderBase provider in FilterProviderManager.Providers) {
                foreach (ITypeInfo typeInfo in typesInfo.PersistentTypes.Where(
                    typeInfo => (provider.ObjectTypes == null 
                        || provider.ObjectTypes.Length == 0 
                        || provider.ObjectTypes.Contains(typeInfo.Type)) 
                       && typeInfo.IsPersistent)) {
                           var baseInfo = GetBaseTypeInfo(typeInfo);
                           
                    if (baseInfo.FindMember(provider.FilterMemberName) == null )
                        CreateMember(baseInfo, provider);
                }
            }
        }

        public static void CreateMember(ITypeInfo typeInfo, FilterProviderBase provider) {
            var attributes = new List<Attribute>
                                 {
                                     new BrowsableAttribute(false),
                                     new MemberDesignTimeVisibilityAttribute(false)
                                 };

            IMemberInfo member = typeInfo.CreateMember(provider.FilterMemberName, provider.FilterMemberType);
            if (provider.FilterMemberIndexed)
                attributes.Add(new IndexedAttribute());
            if (provider.FilterMemberSize != SizeAttribute.DefaultStringMappingFieldSize)
                attributes.Add(new SizeAttribute(provider.FilterMemberSize));
            foreach (Attribute attribute in attributes)
                member.AddAttribute(attribute);
        }

        private void Proxy_DataStoreSelectData(object sender, DataStoreSelectDataEventArgs e) {
            if (_tablesDictionary.Count > 0)
                FilterData(e.SelectStatements);
        }

        public void ModifyData(ModificationStatement[] statements) {
            if (_tablesDictionary.Count > 0) {
                InsertData(statements.OfType<InsertStatement>().ToList());
                UpdateData(statements.OfType<UpdateStatement>());
            }
        }

        public void UpdateData(IEnumerable<UpdateStatement> statements) {
            foreach (UpdateStatement statement in statements) {
                if (!IsSystemTable(statement.TableName)) {
                    var objectType = GetObjectType(statement.TableName);
                    if (objectType == null) continue;
                    if (!_BaseTypesDictionary.ContainsValue(XafTypesInfo.CastTypeToTypeInfo(objectType))) continue;

                    List<QueryOperand> operands = statement.Operands.OfType<QueryOperand>().ToList();
                    for (int i = 0; i < operands.Count(); i++) {
                        int index = i;
                        FilterProviderBase providerBase = FilterProviderManager.GetFilterProvider(objectType, operands[index].ColumnName, StatementContext.Update);
                        if (providerBase != null && !FilterIsShared(statement.TableName, providerBase.Name))
                            statement.Parameters[i].Value = GetModifyFilterValue(providerBase);
                    }
                }

            }
        }

        object GetModifyFilterValue(FilterProviderBase providerBase) {
            return providerBase.FilterValue is IList
                       ? ((IList)providerBase.FilterValue).OfType<object>().FirstOrDefault()
                       : providerBase.FilterValue;
        }


        public void InsertData(IList<InsertStatement> statements) {
            foreach (InsertStatement statement in statements) {
                if (!IsSystemTable(statement.TableName)) {
                    var objectType = GetObjectType(statement.TableName);
                    if (objectType == null) continue;
                    if (!_BaseTypesDictionary.ContainsValue(XafTypesInfo.CastTypeToTypeInfo(objectType))) continue;

                    List<QueryOperand> operands = statement.Operands.OfType<QueryOperand>().ToList();
                    for (int i = 0; i < operands.Count(); i++) {
                        FilterProviderBase providerBase =
                            FilterProviderManager.GetFilterProvider(objectType, operands[i].ColumnName, StatementContext.Insert);
                        if (providerBase != null && !FilterIsShared(statements, providerBase))
                            statement.Parameters[i].Value = GetModifyFilterValue(providerBase);
                    }
                }
            }

        }

        bool FilterIsShared(IEnumerable<InsertStatement> statements, FilterProviderBase providerBase) {
            return statements.Aggregate(false, (current, insertStatement) => current & FilterIsShared(insertStatement.TableName, providerBase.Name));
        }


        public BaseStatement[] FilterData(SelectStatement[] statements) {
            return statements.Where(statement => !IsSystemTable(statement.TableName)).Select(ApplyCondition).ToArray();
        }

        public SelectStatement ApplyCondition(SelectStatement statement) {
            var extractor = new CriteriaOperatorExtractor();
            extractor.Extract(statement.Condition);

            foreach (FilterProviderBase provider in FilterProviderManager.Providers) {
                Type objectType = null;
                if (!_tablesDictionary.TryGetValue(statement.TableName, out objectType))
                    continue;
                FilterProviderBase providerBase = FilterProviderManager.GetFilterProvider(objectType, provider.FilterMemberName, StatementContext.Select);
                if (providerBase != null) {
                    IEnumerable<BinaryOperator> binaryOperators = GetBinaryOperators(extractor, providerBase);
                    if (!FilterIsShared(statement.TableName, providerBase.Name) && binaryOperators.Count() == 0) {
                        string nodeAlias = GetNodeAlias(statement, providerBase);
                        if (!string.IsNullOrEmpty(nodeAlias))
                            ApplyCondition(statement, providerBase, nodeAlias);
                    }
                }
            }
            return statement;
        }

        public class FilterKeyProcesor : CriteriaProcessorBase, IQueryCriteriaVisitor
        {
            private bool _ContainsKey;
            private string _KeyColumnName;
            private string _NodeAlias;
            public FilterKeyProcesor(string keyColumnName,string nodeAlias)
            {
                _KeyColumnName = keyColumnName;
                _NodeAlias = nodeAlias;
                _ContainsKey = false;
            }


            public bool ContainsKey
            {
                get { return _ContainsKey; }
            }
            

            protected override void Process(BinaryOperator theOperator)
            {
                base.Process(theOperator);
                var left = theOperator.LeftOperand as QueryOperand;
                if (!object.ReferenceEquals(left, null) && left.ColumnName == _KeyColumnName && left.NodeAlias == _NodeAlias)
                    _ContainsKey = true;
                else
                {
                    var right = theOperator.RightOperand as QueryOperand;
                    if (!object.ReferenceEquals(null, right) && right.ColumnName == _KeyColumnName && right.NodeAlias == _NodeAlias)
                        _ContainsKey = true;
                }
            }

            protected override void Process(InOperator theOperator)
            {
                base.Process(theOperator);
                var leftOperand = theOperator.LeftOperand as QueryOperand;
                if (!object.ReferenceEquals(leftOperand, null))
                {
                    if (leftOperand.ColumnName == _KeyColumnName && leftOperand.NodeAlias == _NodeAlias)
                        _ContainsKey = true;
                }


            }

            public object Visit(QuerySubQueryContainer theOperand)
            {
                return null;
            }

            public object Visit(QueryOperand theOperand)
            {
                return null;
            }
        }

        private bool FilteredByKey(SelectStatement statement, Type objectType)
        {
            var keyColumnName = XafTypesInfo.XpoTypeInfoSource.GetEntityClassInfo(objectType).KeyProperty.Name;
            FilterKeyProcesor procesor = new FilterKeyProcesor(keyColumnName,statement.Alias);
            procesor.Process(statement.Condition);
            return procesor.ContainsKey;
        }
        void ApplyCondition(SelectStatement statement, FilterProviderBase providerBase, string nodeAlias) {
            var objectType = GetObjectType(statement.TableName);
            if (FilteredByKey(statement, objectType)) return;
            CriteriaOperator condition = null;
            if (providerBase.FilterValue is IList) {
                CriteriaOperator criteriaOperator = ((IEnumerable)providerBase.FilterValue).Cast<object>().Aggregate<object, CriteriaOperator>(null, (current, value)
                    => current | (
                        value == null 
                            ? (CriteriaOperator)new QueryOperand(providerBase.FilterMemberName, nodeAlias).IsNull()
                            : new QueryOperand(providerBase.FilterMemberName, nodeAlias) == new OperandValue( value)));
                criteriaOperator = new GroupOperator(criteriaOperator);
                condition = criteriaOperator;
            } else
                condition = new QueryOperand(providerBase.FilterMemberName, nodeAlias) == (providerBase.FilterValue == null ? null : providerBase.FilterValue.ToString());

            
            if (objectType != null)
            {
                var typeInfo = XafTypesInfo.CastTypeToTypeInfo(objectType);
                if (typeInfo.OwnMembers.FirstOrDefault(x=>x.Name == "ObjectType") != null)
                {
                    
                     var excludes = new List<Type>();
                    foreach (var item in _BaseTypesDictionary.Where(x => x.Value == typeInfo).Select(x => x.Key))
                    {
                        if (FilterProviderManager.GetFilterProvider(item.Type,providerBase.FilterMemberName,StatementContext.Select) == null)
                            excludes.Add(item.Type);
                    }

                    if (excludes.Count > 0)
                    {
                        var table = XafTypesInfo.XpoTypeInfoSource.GetEntityClassInfo(typeof(XPObjectType)).Table;
                        statement.SubNodes.Add(new JoinNode(table,"OT",JoinType.Inner){Condition = new QueryOperand("ObjectType",statement.Alias) == new QueryOperand("OId","OT")});
                        condition |= new InOperator(new QueryOperand("TypeName","OT"),excludes.Select(x=>new OperandValue( x.FullName)).ToArray());
                    }
                }
            }

            statement.Condition &= new GroupOperator(condition);
        }

        IEnumerable<BinaryOperator> GetBinaryOperators(CriteriaOperatorExtractor extractor, FilterProviderBase providerBase) {
            return extractor.BinaryOperators.Where(
                                                      @operator =>
                                                      @operator.RightOperand is OperandValue &&
                                                      ReferenceEquals(((OperandValue)@operator.RightOperand).Value, providerBase.FilterMemberName));
        }

        string GetNodeAlias(SelectStatement statement, FilterProviderBase providerBase) {
            return statement.Operands.OfType<QueryOperand>().Where(operand
                => operand.ColumnName == providerBase.FilterMemberName).Select(operand
                    => operand.NodeAlias).FirstOrDefault() ?? GetNodeAlias(statement, providerBase.FilterMemberName);
        }

        string GetNodeAlias(SelectStatement statement, string filterMemberName) {
            if (!_tablesDictionary.ContainsKey(statement.TableName)) {
                var classInfo = Application.Model.BOModel.Select(mclass => XafTypesInfo.XpoTypeInfoSource.XPDictionary.QueryClassInfo(mclass.TypeInfo.Type)).Where(info => info != null && info.TableName == statement.TableName).FirstOrDefault();
                if (classInfo != null)
                    _tablesDictionary.Add(classInfo.TableName, classInfo.ClassType);
                else
                    throw new ArgumentException(statement.TableName);
            }

            var typeInfo = _BaseTypesDictionary[XafTypesInfo.CastTypeToTypeInfo(GetObjectType(statement.TableName))];
            var baseTableName = XafTypesInfo.XpoTypeInfoSource.GetEntityClassInfo(typeInfo.Type).Table.Name;
            if (statement.TableName == baseTableName)
                return statement.Alias;
            foreach (var node in statement.SubNodes)
            {
            	if (node.TableName == baseTableName)
                    return node.Alias;
            }
            
            return null;
        }


        private bool IsSystemTable(string name) {
            bool ret = false;
            if (Application == null || Application.Model == null)
                return false;

            foreach (IModelFilterDataStoreSystemTable systemTable in ((IModelApplicationFilterDataStore)Application.Model).FilterDataStoreSystemTables) {
                if (systemTable.Name == name)
                    ret = true;
            }
            return ret;
        }

        public bool FilterIsShared(string tableName, string providerName) {
            bool ret = false;

            if (_tablesDictionary.ContainsKey(tableName)) {
                IModelClass modelClass = GetModelClass(tableName);
                if (modelClass != null && ((IModelClassDisabledDataStoreFilters)modelClass).DisabledDataStoreFilters.Where(
                        childNode => childNode.Name == providerName).FirstOrDefault() != null) ret = true;
            }
            return ret;
        }

        IModelClass GetModelClass(string tableName) {
            return Application.Model.BOModel[_tablesDictionary[tableName].FullName];
        }

        Type GetObjectType(string tableName)
        {
            Type result = null;
            if (_tablesDictionary.TryGetValue(tableName, out result))
                return result;
            else
                return null;
        }
    }
}
