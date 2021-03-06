﻿using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using ToSic.SexyContent.GettingStarted;

namespace ToSic.SexyContent
{
    public partial class ViewApp : SexyViewContentOrApp, IActionable
    {
        /// <summary>
        /// Page Load event - preload template chooser if necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Reset messages visible states
            pnlMessage.Visible = false;
            pnlError.Visible = false;

            base.Page_Load(sender, e);
        }

        /// <summary>
        /// Process View if a Template has been set
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            try
            {

                // If there are no apps yet - show "getting started" frame
                if (IsEditable && UserInfo.IsInRole(PortalSettings.AdministratorRoleName) && !SexyContent.GetApps(ZoneId.Value, false, new PortalSettings(ModuleConfiguration.OwnerPortalID)).Any())
                {
                    pnlGetStarted.Visible = true;
                    var gettingStartedControl = (GettingStartedFrame)LoadControl("~/DesktopModules/ToSIC_SexyContent/SexyContent/GettingStarted/GettingStartedFrame.ascx");
                    gettingStartedControl.ModuleID = this.ModuleId;
                    gettingStartedControl.ModuleConfiguration = this.ModuleConfiguration;
                    pnlGetStarted.Controls.Add(gettingStartedControl);
                }

                // If not fully configured, show stuff
                if (UserMayEditThisModule)
                    pnlTemplateChooser.Visible = true;

                if (AppId.HasValue && !Sexy.PortalIsConfigured(Server, ControlPath))
                    Sexy.ConfigurePortal(Server);

                if (AppId.HasValue && Elements.Any() && Elements.First().TemplateId.HasValue)
                    ProcessView(phOutput, pnlError, pnlMessage);

            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

    }
}