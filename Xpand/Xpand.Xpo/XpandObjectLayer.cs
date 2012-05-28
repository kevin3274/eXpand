using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using System.Collections;
using DevExpress.Data.Filtering;
using DevExpress.Xpo.Metadata;

namespace Xpand.Xpo
{
    public class XpandObjectLayer : SimpleObjectLayer,IObjectLayer
    {

        public XpandObjectLayer(IDataLayer dataLayer)
            : base(dataLayer)
        {
            
        }

        


        void IObjectLayer.CommitChanges(Session session, ICollection fullListForDelete, ICollection completeListForSave)
        {
            if (ObjectLayerCommitChanges != null)
                ObjectLayerCommitChanges(this, new ObjectLayerCommitChangesEventArgs(session, fullListForDelete, completeListForSave));
            CommitChanges(session, fullListForDelete, completeListForSave);
        }

        List<object[]> IObjectLayer.SelectData(Session session, ObjectsQuery query, CriteriaOperatorCollection properties, CriteriaOperatorCollection groupProperties, CriteriaOperator groupCriteria)
        {
            if (ObjectLayerSelectData != null)
                ObjectLayerSelectData(this, new ObjectLayerSelectDataEventArgs(session, query, properties, groupProperties, groupCriteria));
            return SelectData(session, query, properties, groupProperties, groupCriteria);
        }

        ICollection[] IObjectLayer.GetObjectsByKey(Session session, ObjectsByKeyQuery[] queries)
        {
            if (ObjectLayerGetObjectsByKey != null)
                ObjectLayerGetObjectsByKey(this, new ObjectLayerGetObjectsByKeyEventArgs(session, queries));
            return GetObjectsByKey(session, queries);
        }

        ICollection[] IObjectLayer.LoadObjects(Session session, ObjectsQuery[] queries)
        {
            if (ObjectLayerLoadObjects != null)
                ObjectLayerLoadObjects(this, new ObjectLayerLoadObjectsEventArgs(session, queries));
            return LoadObjects(session, queries);
        }

        object[] IObjectLayer.LoadCollectionObjects(Session session, XPMemberInfo refProperty, object ownerObject)
        {
            if (ObjectLayerLoadCollectionObjects != null)
                ObjectLayerLoadCollectionObjects(this, new ObjectLayerLoadCollectionObjectsEventArgs(session, refProperty, ownerObject));
            return LoadCollectionObjects(session, refProperty, ownerObject);
        }

        public event EventHandler<ObjectLayerCommitChangesEventArgs> ObjectLayerCommitChanges;

        public event EventHandler<ObjectLayerGetObjectsByKeyEventArgs> ObjectLayerGetObjectsByKey;

        public event EventHandler<ObjectLayerLoadCollectionObjectsEventArgs> ObjectLayerLoadCollectionObjects;

        public event EventHandler<ObjectLayerLoadObjectsEventArgs> ObjectLayerLoadObjects;

        public event EventHandler<ObjectLayerSelectDataEventArgs> ObjectLayerSelectData;
    }

    public class ObjectLayerLoadCollectionObjectsEventArgs : EventArgs
    {

        public ObjectLayerLoadCollectionObjectsEventArgs(Session session, XPMemberInfo refProperty, object ownerObject)
        {
            this.Session = session;
            this.RefProperty = refProperty;
            this.OwnerObject = ownerObject;
        }

        public Session Session { get;private set; }

        public XPMemberInfo RefProperty { get;private set; }

        public object OwnerObject { get;private set; }
    }

    public class ObjectLayerSelectDataEventArgs : EventArgs
    {

        public ObjectLayerSelectDataEventArgs(Session session, ObjectsQuery query, CriteriaOperatorCollection properties, CriteriaOperatorCollection groupProperties, CriteriaOperator groupCriteria)
        {
            this.Session = session;
            this.Query = query;
            this.Properties = properties;
            this.GroupProperties = groupProperties;
            this.GroupCriteria = groupCriteria;
        }

        public Session Session { get; private set; }

        public ObjectsQuery Query { get;private set; }

        public CriteriaOperatorCollection Properties { get;private set; }

        public CriteriaOperatorCollection GroupProperties { get;private set; }

        public CriteriaOperator GroupCriteria { get;private set; }
    }

    public class ObjectLayerGetObjectsByKeyEventArgs : EventArgs
    {

        public ObjectLayerGetObjectsByKeyEventArgs(Session session, ObjectsByKeyQuery[] queries)
        {
            this.Session = session;
            this.Queries = queries;
        }

        public Session Session { get; set; }

        public ObjectsByKeyQuery[] Queries { get; set; }
    }

    public class ObjectLayerCommitChangesEventArgs : EventArgs
    {

        public ObjectLayerCommitChangesEventArgs(Session session, ICollection fullListForDelete, ICollection completeListForSave)
        {
            this.Session = session;
            this.FullListForDelete = fullListForDelete;
            this.CompleteListForSave = completeListForSave;
        }

        public Session Session { get; private set; }

        public ICollection FullListForDelete { get;private set; }

        public ICollection CompleteListForSave { get;private set; }
    }

    public class ObjectLayerLoadObjectsEventArgs : EventArgs
    {

        public ObjectLayerLoadObjectsEventArgs(Session session, ObjectsQuery[] queries)
        {
            this.Session = session;
            this.Queries = queries;
        }

        public Session Session { get;private set; }

        public ObjectsQuery[] Queries { get;private set; }
    }
}
