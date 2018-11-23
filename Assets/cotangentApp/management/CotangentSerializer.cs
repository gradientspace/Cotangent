using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using g3;
using f3;
using gs;

namespace cotangent
{
    public class CotangentSerializer
    {
        public SceneSerializer.EmitOptions SerializeOptions = SceneSerializer.EmitOptions.Default;


        public void StoreCurrent(string path)
        {
            DebugUtil.Log("[CotangentSerializer] Saving scene to " + path);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(path, settings)) {

                writer.WriteStartElement("CotangentFile");

                XMLOutputStream stream = new XMLOutputStream(writer);
                StoreScene(stream, CC.ActiveScene);

                StoreDataModel(writer);

                writer.WriteEndElement();
            }
        }



        public void RestoreToCurrent(string path)
        {
            DebugUtil.Log("[CotangentSerializer] Restoring scene from " + path);

            XmlDocument doc = new XmlDocument();
            try {
                doc.Load(path);
            }catch (Exception) {
                DebugUtil.Log("[CotangentSerializer] failed to read XmlDocument");
                throw;
            }

            // begin restore
            CCActions.BeginRestoreExistingScene();

            // restore scene objects
            XMLInputStream stream = new XMLInputStream() { xml = doc };
            SceneSerializer serializer = new SceneSerializer() {
                SOFactory = new SOFactory()
            };
            serializer.Restore(stream, CC.ActiveScene);

            // complete scene restore

            CCActions.CompleteRestoreExistingScene();

            // restore datamodel
            RestoreDataModel(doc);
        }



        /// <summary>
        /// serialize scene, with SO filter
        /// </summary>
        protected virtual void StoreScene(IOutputStream o, FScene scene)
        {
            SceneSerializer serializer = new SceneSerializer();
            serializer.PushEmitOptions(SerializeOptions);
            serializer.SOFilterF = SerializeSOFilter;
            serializer.Store(o, scene);
        }

        /// <summary>
        /// Serializer SO filter
        /// If you return false from this object for an SO, it is not serialized.
        /// </summary>
        protected virtual bool SerializeSOFilter(SceneObject so)
        {
            // [RMS] force skip any non-print-mesh objects for now
            if (so is PrintMeshSO == false)
                return false;

            return true;
        }


        /// <summary>
        /// Serialize data model.
        /// </summary>
        protected virtual void StoreDataModel(XmlWriter writer)
        {
            writer.WriteStartElement("DataModel");

            StoreClientDataModelData_Start(writer);

            // todo: store custom data here

            StoreClientDataModelData_End(writer);

            writer.WriteEndElement();
        }
        protected virtual void StoreClientDataModelData_Start(XmlWriter writer)
        {
        }
        protected virtual void StoreClientDataModelData_End(XmlWriter writer)
        {
        }







        /// <summary>
        /// parse the DataModel section of the save file, and restore the scene/datamodel as necessary
        /// </summary>
        protected virtual void RestoreDataModel(XmlDocument xml)
        {
            // look up root datamodel (should only be one)
            XmlNodeList datamodels = xml.SelectNodes("//DataModel");
            XmlNode rootNode = datamodels[0];

            RestoreClientDataModelData_Start(rootNode);

            // restore custom data here

            RestoreClientDataModelData_End(rootNode);

        }
        protected virtual void RestoreClientDataModelData_Start(XmlNode rootNode)
        {
        }
        protected virtual void RestoreClientDataModelData_End(XmlNode rootNode)
        {
        }



    }









}
