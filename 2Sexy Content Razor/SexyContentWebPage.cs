﻿using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using ToSic.SexyContent.DataSources;
using ToSic.SexyContent.DataSources.Tokens;
using ToSic.SexyContent.Engines;
using ToSic.SexyContent.Engines.TokenEngine;
using ToSic.SexyContent.Razor.Helpers;
using System.Collections.Generic;
using ToSic.Eav;
using ToSic.Eav.DataSources;
using ToSic.SexyContent.Search;

namespace ToSic.SexyContent.Razor
{
    public abstract class SexyContentWebPage : WebPageBase
    {
        #region Helpers

        protected internal DnnHelper Dnn { get; internal set; }

        protected internal HtmlHelper Html { get; internal set; }

        protected internal UrlHelper Url { get; internal set; }

        // <2sic>
        protected internal dynamic Content { get; internal set; }
        protected internal dynamic Presentation { get; internal set; }
        protected internal dynamic ListContent { get; internal set; }
        protected internal dynamic ListPresentation { get; internal set; }
        protected internal new App App { get; internal set; }
        protected internal List<Element> List { get; internal set; }
        protected internal ViewDataSource Data { get; internal set; }
        protected internal SexyContent Sexy { get; set; }

        /// <summary>
        /// Transform a IEntity to a DynamicEntity as dynamic object
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public dynamic AsDynamic(IEntity entity)
        {
            return new DynamicEntity(entity, new[] { System.Threading.Thread.CurrentThread.CurrentCulture.Name }, Sexy);
        }

        /// <summary>
        /// Makes sure a dynamicEntity could be wrapped in AsDynamic()
        /// </summary>
        /// <param name="dynamicEntity"></param>
        /// <returns></returns>
        public dynamic AsDynamic(dynamic dynamicEntity)
        {
            return dynamicEntity;
        }

        /// <summary>
        /// Returns the value of a KeyValuePair as DynamicEntity
        /// </summary>
        /// <param name="entityKeyValuePair"></param>
        /// <returns></returns>
        public dynamic AsDynamic(KeyValuePair<int, IEntity> entityKeyValuePair)
        {
            return AsDynamic(entityKeyValuePair.Value);
        }

		/// <summary>
		/// In case AsDynamic is used with Data["name"]
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public IEnumerable<dynamic> AsDynamic(IDataStream stream)
        {
            return AsDynamic(stream.List);//.Select(e => AsDynamic(e.Value));
        }

		/// <summary>
		/// In case AsDynamic is used with Data["name"].List
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public IEnumerable<dynamic> AsDynamic(IDictionary<int, IEntity> list)
		{
			return list.Select(e => AsDynamic(e.Value));
		}

        /// <summary>
        /// Transform a DynamicEntity dynamic object back to a IEntity instance
        /// </summary>
        /// <param name="dynamicEntity"></param>
        /// <returns></returns>
        public IEntity AsEntity(dynamic dynamicEntity)
        {
            return ((DynamicEntity)dynamicEntity).Entity;
        }

        /// <summary>
        /// Returns a list of DynamicEntities
        /// </summary>
        /// <param name="entities">List of entities</param>
        /// <returns></returns>
        public IEnumerable<dynamic> AsDynamic(IEnumerable<IEntity> entities)
        {
            return entities.Select(e => AsDynamic(e));
        }
        

        // </2sic>

        #endregion

        #region BaseClass Overrides

        protected override void ConfigurePage(WebPageBase parentPage)
        {
            base.ConfigurePage(parentPage);

            // Child pages need to get their context from the Parent
            Context = parentPage.Context;

            // Return if parent page is not a SexyContentWebPage
            if (parentPage.GetType().BaseType != typeof(SexyContentWebPage)) return;
            
            Dnn = ((SexyContentWebPage) parentPage).Dnn;
            Html = ((SexyContentWebPage) parentPage).Html;
            Url = ((SexyContentWebPage) parentPage).Url;

            Content = ((SexyContentWebPage) parentPage).Content;
            Presentation = ((SexyContentWebPage) parentPage).Presentation;
            ListContent = ((SexyContentWebPage) parentPage).ListContent;
            ListPresentation = ((SexyContentWebPage) parentPage).ListPresentation;
            List = ((SexyContentWebPage) parentPage).List;
            Data = ((SexyContentWebPage) parentPage).Data;
            App = ((SexyContentWebPage) parentPage).App;
        }

        #endregion


        private IConfigurationProvider _configurationProvider;
        private IConfigurationProvider ConfigurationProvider
        {
            get
            {
                if (_configurationProvider == null)
                {
                    _configurationProvider = Data.In["Default"].Source.ConfigurationProvider;
                }
                return _configurationProvider;
            }
        }

        protected IDataSource CreateSource(string typeName = "", IDataSource inSource = null, IConfigurationProvider configurationProvider = null)
        {
            if (configurationProvider == null)
                configurationProvider = ConfigurationProvider;

            if (inSource != null)
                return DataSource.GetDataSource(typeName, inSource.ZoneId, inSource.AppId, inSource, configurationProvider);

            var initialSource = SexyContent.GetInitialDataSource(SexyContent.GetZoneID(Dnn.Portal.PortalId).Value, App.AppId, SexyContent.HasEditPermission(Dnn.Module));
            return typeName != "" ? DataSource.GetDataSource(typeName, initialSource.ZoneId, initialSource.AppId, initialSource, configurationProvider) : initialSource;
        }

        protected T CreateSource<T>(IDataSource inSource = null, IConfigurationProvider configurationProvider = null)
        {
            if (configurationProvider == null)
                configurationProvider = ConfigurationProvider;

            if (inSource != null)
                return DataSource.GetDataSource<T>(inSource.ZoneId, inSource.AppId, inSource, configurationProvider);

            var initialSource = SexyContent.GetInitialDataSource(SexyContent.GetZoneID(Dnn.Portal.PortalId).Value, App.AppId, SexyContent.HasEditPermission(Dnn.Module));
            return DataSource.GetDataSource<T>(initialSource.ZoneId, initialSource.AppId, initialSource, configurationProvider);
        }

		/// <summary>
		/// Create a source with initial stream to attach...
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="inStream"></param>
		/// <returns></returns>
		protected T CreateSource<T>(IDataStream inStream)
		{
			// if it has a source, then use this, otherwise it's null and that works too. Reason: some sources like DataTable or SQL won't have an upstream source
			T src = CreateSource<T>(inStream.Source);
			
			IDataTarget srcDs = (IDataTarget) src;
			srcDs.In.Clear();
			srcDs.In.Add(DataSource.DefaultStreamName, inStream);
			return src;
		}


        /// <summary>
        /// Creates instances of the shared pages with the given relative path
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public dynamic CreateInstance(string relativePath)
        {
            var path = NormalizePath(relativePath);

            if(!File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(path)))
                throw new FileNotFoundException("The shared file does not exist.", path);

            var webPage = (SexyContentWebPage)CreateInstanceFromVirtualPath(path);
            webPage.ConfigurePage(this);
            return webPage;
        }


        /// <summary>
        /// Override this to have your code change the (already initialized) Data object. 
        /// If you don't override this, nothing will be changed/customized. 
        /// </summary>
        public virtual void CustomizeData()
        {
        }

        public virtual void CustomizeSearch(Dictionary<string, List<ISearchInfo>> searchInfos, ModuleInfo moduleInfo, DateTime beginDate)
        {
        }

        public InstancePurposes InstancePurpose { get; set; }

    }

    // <2sic> Removed DotNetNukeWebPage<T>:DotNetNukeWebPage
    // </2sic>

    
}